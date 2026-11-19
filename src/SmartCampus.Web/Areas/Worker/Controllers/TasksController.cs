using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;
using SmartCampus.Web.Services;

namespace SmartCampus.Web.Areas.Worker.Controllers;

[Area("Worker")]
[Authorize(Roles = "MaintenanceWorker")]
public class TasksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storageService;

    public TasksController(ApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> MyTasks()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var assignments = await _context.WorkOrderAssignments
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
                    .ThenInclude(c => c!.Location)
                        .ThenInclude(l => l!.Floor)
                            .ThenInclude(f => f!.Building)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
                    .ThenInclude(c => c!.Priority)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
                    .ThenInclude(c => c!.Images)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Evidence)
            .Where(a => a.WorkerUserId == currentUserId)
            .OrderByDescending(a => a.AssignedAtUtc)
            .ToListAsync();

        // Also fetch active work orders dispatched to this worker's department or squads that are open for claiming
        var myTeamIds = await _context.TeamMembers
            .Where(tm => tm.WorkerUserId == currentUserId && tm.IsActive)
            .Select(tm => tm.TeamId)
            .ToListAsync();

        var myDeptIds = await _context.TeamMembers
            .Include(tm => tm.Team)
            .Where(tm => tm.WorkerUserId == currentUserId && tm.IsActive)
            .Select(tm => tm.Team!.DepartmentId)
            .Distinct()
            .ToListAsync();

        var squadWorkOrders = await _context.WorkOrders
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Location)
                    .ThenInclude(l => l!.Floor)
                        .ThenInclude(f => f!.Building)
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Priority)
            .Include(w => w.Department)
            .Include(w => w.Team)
            .Include(w => w.Assignments)
            .Where(w => (myTeamIds.Contains(w.TeamId ?? 0) || myDeptIds.Contains(w.DepartmentId))
                     && !w.Assignments.Any(a => a.WorkerUserId == currentUserId)
                     && w.Status == WorkOrderStatus.Assigned)
            .OrderByDescending(w => w.AssignedAtUtc)
            .ToListAsync();

        ViewBag.SquadWorkOrders = squadWorkOrders;

        return View(assignments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Claim(int workOrderId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var wo = await _context.WorkOrders
            .Include(w => w.Complaint)
            .Include(w => w.Assignments)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId);

        if (wo == null) return NotFound();

        if (!wo.Assignments.Any(a => a.WorkerUserId == currentUserId))
        {
            _context.WorkOrderAssignments.Add(new WorkOrderAssignment
            {
                WorkOrderId = workOrderId,
                WorkerUserId = currentUserId!,
                AssignedAtUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = $"Work Order {wo.WorkOrderNumber} claimed successfully. You can now accept and commence work.";
        return RedirectToAction(nameof(MyTasks));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int workOrderId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var assignment = await _context.WorkOrderAssignments
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
            .FirstOrDefaultAsync(a => a.WorkOrderId == workOrderId && a.WorkerUserId == currentUserId);

        if (assignment == null) return NotFound();

        var wo = assignment.WorkOrder!;
        assignment.AcceptedAtUtc = DateTime.UtcNow;
        wo.Status = WorkOrderStatus.WorkAccepted;

        if (wo.Complaint != null && wo.Complaint.Status == ComplaintStatus.Assigned)
        {
            wo.Complaint.Status = ComplaintStatus.WorkAccepted;
        }

        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = wo.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.Assigned,
            NewStatus = ComplaintStatus.WorkAccepted,
            Action = "WorkerAcceptedTask",
            Remarks = $"Technician acknowledged task dispatch for Work Order {wo.WorkOrderNumber}.",
            TimestampUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Task {wo.WorkOrderNumber} accepted. Proceed to site to commence work.";
        return RedirectToAction(nameof(MyTasks));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartWork(int workOrderId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var assignment = await _context.WorkOrderAssignments
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
            .FirstOrDefaultAsync(a => a.WorkOrderId == workOrderId && a.WorkerUserId == currentUserId);

        if (assignment == null) return NotFound();

        var wo = assignment.WorkOrder!;
        wo.WorkStartedAtUtc = DateTime.UtcNow;
        wo.Status = WorkOrderStatus.InProgress;

        if (wo.Complaint != null)
        {
            wo.Complaint.Status = ComplaintStatus.InProgress;
        }

        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = wo.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.WorkAccepted,
            NewStatus = ComplaintStatus.InProgress,
            Action = "WorkStartedOnSite",
            Remarks = $"Technician initiated on-site physical repair for Work Order {wo.WorkOrderNumber}.",
            TimestampUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Status updated to 'In Progress'. Work has started on-site.";
        return RedirectToAction(nameof(MyTasks));
    }

    [HttpGet]
    public async Task<IActionResult> Complete(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var assignment = await _context.WorkOrderAssignments
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
                    .ThenInclude(c => c!.Location)
                        .ThenInclude(l => l!.Floor)
                            .ThenInclude(f => f!.Building)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
                    .ThenInclude(c => c!.Images)
            .FirstOrDefaultAsync(a => a.WorkOrderId == id && a.WorkerUserId == currentUserId);

        if (assignment == null) return NotFound();

        return View(assignment.WorkOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int workOrderId, string workSummary, List<IFormFile>? completionPhotos)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var assignment = await _context.WorkOrderAssignments
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Team)
            .FirstOrDefaultAsync(a => a.WorkOrderId == workOrderId && a.WorkerUserId == currentUserId);

        if (assignment == null) return NotFound();

        if (string.IsNullOrWhiteSpace(workSummary))
        {
            TempData["ErrorMessage"] = "Please provide a brief summary of the repair and materials used.";
            return RedirectToAction(nameof(Complete), new { id = workOrderId });
        }

        if (completionPhotos == null || !completionPhotos.Any())
        {
            TempData["ErrorMessage"] = "Mandatory: Upload at least one photographic proof of completed repair.";
            return RedirectToAction(nameof(Complete), new { id = workOrderId });
        }

        var wo = assignment.WorkOrder!;
        wo.WorkCompletedAtUtc = DateTime.UtcNow;
        wo.WorkSummary = workSummary.Trim();
        wo.Status = WorkOrderStatus.WorkCompleted;

        // Save evidence photos
        foreach (var photo in completionPhotos.Take(3))
        {
            var (success, fileUrl, error) = await _storageService.SaveImageAsync(photo, "evidence");
            if (success && fileUrl != null)
            {
                _context.WorkOrderEvidences.Add(new WorkOrderEvidence
                {
                    WorkOrderId = wo.WorkOrderId,
                    WorkerUserId = currentUserId!,
                    ImageUrl = fileUrl,
                    Notes = workSummary.Trim(),
                    UploadedAtUtc = DateTime.UtcNow
                });
            }
        }

        // CRITICAL ARCHITECTURAL RULE: Transition to AwaitingVerification (NOT VerifiedResolved)
        wo.Status = WorkOrderStatus.AwaitingVerification;
        if (wo.Complaint != null)
        {
            wo.Complaint.Status = ComplaintStatus.AwaitingVerification;
        }

        assignment.CompletedAtUtc = DateTime.UtcNow;

        // Audit History
        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = wo.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.InProgress,
            NewStatus = ComplaintStatus.AwaitingVerification,
            Action = "WorkSubmittedForVerification",
            Remarks = $"Technician completed field repair and uploaded after-photos. Sent to Independent Verification Gate.",
            TimestampUtc = DateTime.UtcNow
        });

        // Notify Supervisor
        if (wo.Team?.SupervisorUserId != null)
        {
            _context.Notifications.Add(new Notification
            {
                RecipientUserId = wo.Team.SupervisorUserId,
                Title = $"Inspection Ready: {wo.WorkOrderNumber}",
                Message = $"Work completed on ticket {wo.Complaint?.TicketNumber}. On-site physical inspection and sign-off required.",
                TargetUrl = $"/Supervisor/Verification/Inspect/{wo.WorkOrderId}",
                NotificationType = "Warning",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Repair submitted! Work Order {wo.WorkOrderNumber} has moved to 'Awaiting Verification' for Department Supervisor physical inspection.";
        return RedirectToAction(nameof(MyTasks));
    }
}
