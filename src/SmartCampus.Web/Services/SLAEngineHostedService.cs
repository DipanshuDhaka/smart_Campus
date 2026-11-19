using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Services;

public class SLAEngineHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SLAEngineHostedService> _logger;
    private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(60);

    public SLAEngineHostedService(IServiceProvider serviceProvider, ILogger<SLAEngineHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SLA Monitoring & Escalation Background Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await MonitorAndEscalateAsync();
                await Task.Delay(_pollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // Graceful host shutdown
            }
            catch (Exception ex)
            {
                if (stoppingToken.IsCancellationRequested) break;
                _logger.LogError(ex, "Error occurred during SLA monitoring cycle.");
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        _logger.LogInformation("SLA Monitoring & Escalation Background Service stopped.");
    }

    public async Task MonitorAndEscalateAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;

        // Resolve Control Desk Admin User ID for notifications
        var adminUserId = await context.Users
            .Where(u => u.Email == "controldesk@smartcampus.lpu.in" || u.UserName == "controldesk@smartcampus.lpu.in")
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(adminUserId))
        {
            adminUserId = await context.Users
                .Where(u => u.Email == "superadmin@smartcampus.lpu.in" || u.UserName == "superadmin@smartcampus.lpu.in")
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }

        // Fetch active complaints with configured deadlines using AsSplitQuery to avoid EF warning 20504
        var activeComplaints = await context.Complaints
            .AsSplitQuery()
            .Include(c => c.WorkOrders)
                .ThenInclude(w => w.Assignments)
            .Include(c => c.WorkOrders)
                .ThenInclude(w => w.Team)
            .Where(c => c.Status != ComplaintStatus.VerifiedResolved &&
                        c.Status != ComplaintStatus.Closed &&
                        c.Status != ComplaintStatus.Rejected &&
                        c.ResolutionDeadlineUtc.HasValue)
            .ToListAsync();

        foreach (var complaint in activeComplaints)
        {
            var totalDuration = (complaint.ResolutionDeadlineUtc!.Value - complaint.CreatedAtUtc).TotalMinutes;
            if (totalDuration <= 0) continue;

            var elapsedMinutes = (now - complaint.CreatedAtUtc).TotalMinutes;
            var percentElapsed = (elapsedMinutes / totalDuration) * 100.0;

            // 1. TIER 3: 100% SLA Breached
            if (now > complaint.ResolutionDeadlineUtc.Value && !complaint.IsOverdue)
            {
                complaint.IsOverdue = true;

                foreach (var wo in complaint.WorkOrders)
                {
                    wo.IsOverdue = true;

                    // Log escalation breach (check if already logged to prevent duplicates)
                    bool alreadyBreached = await context.SLAEscalations
                        .AnyAsync(e => e.WorkOrderId == wo.WorkOrderId && e.EscalationTier == EscalationTier.Tier3_Breach);

                    if (!alreadyBreached)
                    {
                        context.SLAEscalations.Add(new SLAEscalation
                        {
                            WorkOrderId = wo.WorkOrderId,
                            EscalationTier = EscalationTier.Tier3_Breach,
                            TriggeredAtUtc = now,
                            BreachMinutes = (int)(now - complaint.ResolutionDeadlineUtc.Value).TotalMinutes,
                            IsAcknowledged = false
                        });
                    }
                }

                // Notify Control Desk Admin with verified User Id
                if (!string.IsNullOrEmpty(adminUserId))
                {
                    await SafeAddNotificationAsync(
                        context,
                        adminUserId,
                        $"CRITICAL SLA BREACH: Ticket {complaint.TicketNumber}",
                        $"Ticket '{complaint.Title}' has exceeded its resolution deadline by {(int)(now - complaint.ResolutionDeadlineUtc.Value).TotalMinutes} minutes.",
                        $"/Admin/Triage/Review/{complaint.ComplaintId}",
                        "Critical",
                        now);
                }

                _logger.LogWarning("SLA Breached for Ticket {TicketNumber}", complaint.TicketNumber);
            }
            // 2. TIER 2: 75% Warning
            else if (percentElapsed >= 75.0 && percentElapsed < 100.0)
            {
                foreach (var wo in complaint.WorkOrders.Where(w => w.Team?.SupervisorUserId != null))
                {
                    bool alreadyWarned = await context.SLAEscalations
                        .AnyAsync(e => e.WorkOrderId == wo.WorkOrderId && e.EscalationTier == EscalationTier.Tier2_Warning);

                    if (!alreadyWarned)
                    {
                        context.SLAEscalations.Add(new SLAEscalation
                        {
                            WorkOrderId = wo.WorkOrderId,
                            EscalationTier = EscalationTier.Tier2_Warning,
                            TriggeredAtUtc = now,
                            BreachMinutes = 0,
                            IsAcknowledged = false
                        });

                        await SafeAddNotificationAsync(
                            context,
                            wo.Team!.SupervisorUserId,
                            $"SLA Warning (75% Elapsed): {wo.WorkOrderNumber}",
                            $"Work Order {wo.WorkOrderNumber} has consumed 75% of allocated resolution time. Please expedite team.",
                            $"/Supervisor/WorkOrders",
                            "Warning",
                            now);
                    }
                }
            }
            // 3. TIER 1: 50% Reminder
            else if (percentElapsed >= 50.0 && percentElapsed < 75.0)
            {
                foreach (var wo in complaint.WorkOrders)
                {
                    foreach (var a in wo.Assignments)
                    {
                        bool alreadyReminded = await context.SLAEscalations
                            .AnyAsync(e => e.WorkOrderId == wo.WorkOrderId && e.EscalationTier == EscalationTier.Tier1_Reminder);

                        if (!alreadyReminded)
                        {
                            context.SLAEscalations.Add(new SLAEscalation
                            {
                                WorkOrderId = wo.WorkOrderId,
                                EscalationTier = EscalationTier.Tier1_Reminder,
                                TriggeredAtUtc = now,
                                BreachMinutes = 0,
                                IsAcknowledged = false
                            });

                            await SafeAddNotificationAsync(
                                context,
                                a.WorkerUserId,
                                $"SLA Reminder (50% Elapsed): {wo.WorkOrderNumber}",
                                $"Half of the allotted SLA time has elapsed for ticket {complaint.TicketNumber}.",
                                $"/Worker/Tasks/MyTasks",
                                "Info",
                                now);
                        }
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private async Task SafeAddNotificationAsync(
        ApplicationDbContext context,
        string? recipientUserId,
        string title,
        string message,
        string targetUrl,
        string notificationType,
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(recipientUserId)) return;

        var userExists = await context.Users.AnyAsync(u => u.Id == recipientUserId);
        if (!userExists)
        {
            _logger.LogWarning("Notification '{Title}' skipped because RecipientUserId '{RecipientUserId}' was not found in AspNetUsers.", title, recipientUserId);
            return;
        }

        context.Notifications.Add(new Notification
        {
            RecipientUserId = recipientUserId,
            Title = title,
            Message = message,
            TargetUrl = targetUrl,
            NotificationType = notificationType,
            CreatedAtUtc = createdAtUtc
        });
    }
}
