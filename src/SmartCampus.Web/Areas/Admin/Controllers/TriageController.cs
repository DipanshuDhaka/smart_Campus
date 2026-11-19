using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;
using SmartCampus.Web.Services;

namespace SmartCampus.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,CampusAdmin")]
public class TriageController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ISmartRecommenderService _smartRecommender;

    public TriageController(ApplicationDbContext context, ISmartRecommenderService smartRecommender)
    {
        _context = context;
        _smartRecommender = smartRecommender;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? statusFilter, int? categoryFilter, int? buildingFilter)
    {
        var query = _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.Priority)
            .Include(c => c.Location)
                .ThenInclude(l => l!.Floor)
                    .ThenInclude(f => f!.Building)
            .Include(c => c.ReporterUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ComplaintStatus>(statusFilter, out var parsedStatus))
        {
            query = query.Where(c => c.Status == parsedStatus);
        }

        if (categoryFilter.HasValue && categoryFilter.Value > 0)
        {
            query = query.Where(c => c.CategoryId == categoryFilter.Value);
        }

        if (buildingFilter.HasValue && buildingFilter.Value > 0)
        {
            query = query.Where(c => c.Location!.Floor!.BuildingId == buildingFilter.Value);
        }

        var complaints = await query.OrderByDescending(c => c.CreatedAtUtc).ToListAsync();

        ViewBag.Categories = await _context.IssueCategories.Where(c => c.IsActive).ToListAsync();
        ViewBag.Buildings = await _context.Buildings.Where(b => b.IsActive).ToListAsync();
        ViewBag.StatusFilter = statusFilter;
        ViewBag.CategoryFilter = categoryFilter;
        ViewBag.BuildingFilter = buildingFilter;

        return View(complaints);
    }

    [HttpGet]
    public async Task<IActionResult> Review(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var complaint = await _context.Complaints
            .Include(c => c.Category)
                .ThenInclude(cat => cat!.DefaultDepartment)
            .Include(c => c.Priority)
            .Include(c => c.Location)
                .ThenInclude(l => l!.Floor)
                    .ThenInclude(f => f!.Building)
            .Include(c => c.Images)
            .Include(c => c.ReporterUser)
            .Include(c => c.History)
            .Include(c => c.WorkOrders)
                .ThenInclude(w => w.Department)
            .Include(c => c.WorkOrders)
                .ThenInclude(w => w.Team)
                    .ThenInclude(t => t!.SupervisorUser)
            .Include(c => c.WorkOrders)
                .ThenInclude(w => w.Assignments)
                    .ThenInclude(a => a.WorkerUser)
            .FirstOrDefaultAsync(c => c.ComplaintId == id);

        if (complaint == null) return NotFound();

        // 1. Move from Submitted to UnderReview upon opening
        if (complaint.Status == ComplaintStatus.Submitted)
        {
            complaint.Status = ComplaintStatus.UnderReview;
            _context.ComplaintHistories.Add(new ComplaintHistory
            {
                ComplaintId = complaint.ComplaintId,
                ActorUserId = currentUserId!,
                PreviousStatus = ComplaintStatus.Submitted,
                NewStatus = ComplaintStatus.UnderReview,
                Action = "TriageOpened",
                Remarks = "Ticket opened for Control Desk inspection.",
                TimestampUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        // 2. Duplicate Detection Heuristic: search complaints at same location & category in last 48 hours
        var cutoff = DateTime.UtcNow.AddHours(-48);
        var duplicates = await _context.Complaints
            .Include(c => c.ReporterUser)
            .Where(c => c.ComplaintId != complaint.ComplaintId &&
                        c.LocationId == complaint.LocationId &&
                        c.CategoryId == complaint.CategoryId &&
                        c.CreatedAtUtc >= cutoff &&
                        c.Status != ComplaintStatus.Rejected)
            .ToListAsync();

        ViewBag.PotentialDuplicates = duplicates;
        ViewBag.Priorities = await _context.Priorities.Include(p => p.SLAPolicy).OrderBy(p => p.SeverityLevel).ToListAsync();
        ViewBag.Departments = await _context.Departments
            .Include(d => d.Teams)
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        ViewBag.Teams = await _context.Teams
            .Include(t => t.SupervisorUser)
            .Include(t => t.Department)
            .Where(t => t.IsActive)
            .OrderBy(t => t.TeamName)
            .ToListAsync();

        // 3. Load Available Maintenance Technicians
        ViewBag.Technicians = await _context.TeamMembers
            .Include(tm => tm.WorkerUser)
            .Include(tm => tm.Team)
            .Where(tm => tm.IsActive)
            .OrderBy(tm => tm.WorkerUser!.Email)
            .ToListAsync();

        // 4. AI / Keyword Heuristic Trade Recommendation
        var recommendation = await _smartRecommender.RecommendDepartmentAsync(
            complaint.Title, 
            complaint.Description, 
            complaint.Category?.CategoryName);
        ViewBag.Recommendation = recommendation;

        return View(complaint);
    }

    [HttpGet]
    public Task<IActionResult> Details(int id) => Review(id);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int complaintId, int priorityId, int departmentId, int? teamId, string? technicianUserId, string? instructions)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var complaint = await _context.Complaints
            .Include(c => c.Location)
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);

        if (complaint == null) return NotFound();

        // 1. Enforce "1 Assignment Only" Rule: Prevent duplicate dispatch
        var existingWorkOrder = await _context.WorkOrders.FirstOrDefaultAsync(w => w.ComplaintId == complaintId);
        if (existingWorkOrder != null || (complaint.Status != ComplaintStatus.Submitted && complaint.Status != ComplaintStatus.UnderReview))
        {
            TempData["ErrorMessage"] = $"Complaint {complaint.TicketNumber} has already been assigned under Work Order {existingWorkOrder?.WorkOrderNumber ?? "an active work order"}. Each complaint can only be assigned once.";
            return RedirectToAction(nameof(Review), new { id = complaintId });
        }

        var priority = await _context.Priorities
            .Include(p => p.SLAPolicy)
            .FirstOrDefaultAsync(p => p.PriorityId == priorityId);

        if (priority == null) return BadRequest("Invalid Priority specified.");

        // Calculate SLA Deadlines
        var slaPolicy = priority.SLAPolicy;
        if (slaPolicy != null)
        {
            complaint.ResponseDeadlineUtc = DateTime.UtcNow.AddMinutes(slaPolicy.ResponseTargetMinutes);
            complaint.WorkStartDeadlineUtc = DateTime.UtcNow.AddMinutes(slaPolicy.WorkStartTargetMinutes);
            complaint.ResolutionDeadlineUtc = DateTime.UtcNow.AddMinutes(slaPolicy.ResolutionTargetMinutes);
        }
        else
        {
            // Fallback default 24h
            complaint.ResolutionDeadlineUtc = DateTime.UtcNow.AddHours(24);
        }

        complaint.PriorityId = priorityId;
        complaint.Status = ComplaintStatus.Assigned;

        // Generate child WorkOrder
        var woCount = await _context.WorkOrders.CountAsync() + 1;
        var woNumber = $"WO-{DateTime.UtcNow.Year}-{woCount:D5}";

        var workOrder = new WorkOrder
        {
            WorkOrderNumber = woNumber,
            ComplaintId = complaint.ComplaintId,
            DepartmentId = departmentId,
            TeamId = teamId,
            Status = WorkOrderStatus.Assigned,
            WorkSummary = instructions,
            AssignedAtUtc = DateTime.UtcNow
        };

        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();

        // 2. Direct Technician Allocation / Auto-Dispatch
        // If technician is explicitly chosen, allocate them. Otherwise auto-allocate primary available technician from the selected team or department.
        string? allocatedWorkerId = technicianUserId;
        if (string.IsNullOrWhiteSpace(allocatedWorkerId) && teamId.HasValue)
        {
            allocatedWorkerId = await _context.TeamMembers
                .Where(tm => tm.TeamId == teamId.Value && tm.IsActive)
                .Select(tm => tm.WorkerUserId)
                .FirstOrDefaultAsync();
        }

        if (string.IsNullOrWhiteSpace(allocatedWorkerId) && departmentId > 0)
        {
            allocatedWorkerId = await _context.TeamMembers
                .Include(tm => tm.Team)
                .Where(tm => tm.Team!.DepartmentId == departmentId && tm.IsActive)
                .Select(tm => tm.WorkerUserId)
                .FirstOrDefaultAsync();
        }

        string workerRemarks = "";
        if (!string.IsNullOrWhiteSpace(allocatedWorkerId))
        {
            _context.WorkOrderAssignments.Add(new WorkOrderAssignment
            {
                WorkOrderId = workOrder.WorkOrderId,
                WorkerUserId = allocatedWorkerId,
                AssignedAtUtc = DateTime.UtcNow
            });

            var workerUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == allocatedWorkerId);
            workerRemarks = $" Allocated technician: {workerUser?.Email?.Split('@')[0] ?? "Technician"}.";

            // Notify Technician directly
            _context.Notifications.Add(new Notification
            {
                RecipientUserId = allocatedWorkerId,
                Title = $"New Task Assigned: {woNumber}",
                Message = $"You have been assigned to maintenance ticket {complaint.TicketNumber}: '{complaint.Title}'.",
                TargetUrl = "/Worker/Tasks/MyTasks",
                NotificationType = "Info",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        // Audit History
        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = complaint.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.UnderReview,
            NewStatus = ComplaintStatus.Assigned,
            Action = "TriageApprovedAndDispatched",
            Remarks = $"Complaint approved at priority '{priority.PriorityName}'. Dispatched to Department ID {departmentId} with Work Order {woNumber}.{workerRemarks}",
            TimestampUtc = DateTime.UtcNow
        });

        // Notify Reporter
        _context.Notifications.Add(new Notification
        {
            RecipientUserId = complaint.ReporterUserId,
            Title = $"Complaint Approved: {complaint.TicketNumber}",
            Message = $"Your complaint has been approved (Priority: {priority.PriorityName}) and dispatched to maintenance. Target resolution: {complaint.ResolutionDeadlineUtc:dd MMM, hh:mm tt}.",
            TargetUrl = $"/Reporter/Complaints/Details/{complaint.ComplaintId}",
            NotificationType = "Success",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Notify Assigned Supervisor / Team Head
        string? supervisorId = null;
        string squadLabel = "Maintenance Department";
        if (teamId.HasValue)
        {
            var team = await _context.Teams.FirstOrDefaultAsync(t => t.TeamId == teamId.Value);
            supervisorId = team?.SupervisorUserId;
            if (team != null) squadLabel = team.TeamName;
        }
        else
        {
            var deptTeam = await _context.Teams.FirstOrDefaultAsync(t => t.DepartmentId == departmentId && t.SupervisorUserId != null);
            supervisorId = deptTeam?.SupervisorUserId;
        }

        if (!string.IsNullOrEmpty(supervisorId))
        {
            _context.Notifications.Add(new Notification
            {
                RecipientUserId = supervisorId,
                Title = $"New Work Order Dispatched: {woNumber}",
                Message = $"Ticket {complaint.TicketNumber} ({priority.PriorityName}) assigned to squad '{squadLabel}'.{workerRemarks}",
                TargetUrl = $"/Supervisor/WorkOrders/Assign/{workOrder.WorkOrderId}",
                NotificationType = "Info",
                CreatedAtUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = $"Complaint {complaint.TicketNumber} approved! Work Order {woNumber} generated and dispatched.{workerRemarks}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int complaintId, string rejectionReason)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);

        if (complaint == null) return NotFound();

        if (string.IsNullOrWhiteSpace(rejectionReason))
        {
            TempData["ErrorMessage"] = "A mandatory rejection justification reason must be provided.";
            return RedirectToAction(nameof(Review), new { id = complaintId });
        }

        complaint.Status = ComplaintStatus.Rejected;
        complaint.RejectionReason = rejectionReason.Trim();

        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = complaint.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.UnderReview,
            NewStatus = ComplaintStatus.Rejected,
            Action = "TriageRejected",
            Remarks = $"Complaint rejected by Control Desk: {rejectionReason.Trim()}",
            TimestampUtc = DateTime.UtcNow
        });

        // Notify Reporter
        _context.Notifications.Add(new Notification
        {
            RecipientUserId = complaint.ReporterUserId,
            Title = $"Complaint Rejected: {complaint.TicketNumber}",
            Message = $"Your reported issue could not be processed. Reason: {rejectionReason.Trim()}",
            TargetUrl = $"/Reporter/Complaints/Details/{complaint.ComplaintId}",
            NotificationType = "Danger",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Complaint {complaint.TicketNumber} has been rejected.";
        return RedirectToAction(nameof(Index));
    }
}
