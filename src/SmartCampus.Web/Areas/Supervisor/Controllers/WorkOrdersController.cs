using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Areas.Supervisor.Controllers;

[Area("Supervisor")]
[Authorize(Roles = "DepartmentSupervisor,CampusAdmin,SuperAdmin")]
public class WorkOrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkOrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Fetch work orders for supervisor's department or teams
        var workOrders = await _context.WorkOrders
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Location)
                    .ThenInclude(l => l!.Floor)
                        .ThenInclude(f => f!.Building)
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Priority)
            .Include(w => w.Department)
            .Include(w => w.Team)
            .Include(w => w.Assignments)
                .ThenInclude(a => a.WorkerUser)
            .OrderByDescending(w => w.AssignedAtUtc)
            .ToListAsync();

        return View(workOrders);
    }

    [HttpGet]
    public async Task<IActionResult> Assign(int id)
    {
        var workOrder = await _context.WorkOrders
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Location)
                    .ThenInclude(l => l!.Floor)
                        .ThenInclude(f => f!.Building)
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Images)
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Priority)
            .Include(w => w.Department)
            .Include(w => w.Assignments)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (workOrder == null) return NotFound();

        // Fetch team members for the department
        var teamMembers = await _context.TeamMembers
            .Include(tm => tm.WorkerUser)
            .Include(tm => tm.Team)
            .Where(tm => tm.Team!.DepartmentId == workOrder.DepartmentId && tm.IsActive)
            .ToListAsync();

        // Calculate active workload for auto-balancing
        var activeStatuses = new[] { WorkOrderStatus.Assigned, WorkOrderStatus.WorkAccepted, WorkOrderStatus.InProgress };
        var activeWorkloads = await _context.WorkOrderAssignments
            .Include(a => a.WorkOrder)
            .Where(a => activeStatuses.Contains(a.WorkOrder!.Status))
            .GroupBy(a => a.WorkerUserId)
            .Select(g => new { WorkerUserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.WorkerUserId, x => x.Count);

        ViewBag.TeamMembers = teamMembers;
        ViewBag.WorkerWorkloads = activeWorkloads;

        return View(workOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int workOrderId, List<string> workerUserIds)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var workOrder = await _context.WorkOrders
            .Include(w => w.Complaint)
            .Include(w => w.Assignments)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId);

        if (workOrder == null) return NotFound();

        if (workerUserIds == null || !workerUserIds.Any())
        {
            TempData["ErrorMessage"] = "Please select at least one maintenance technician.";
            return RedirectToAction(nameof(Assign), new { id = workOrderId });
        }

        foreach (var workerId in workerUserIds)
        {
            if (!workOrder.Assignments.Any(a => a.WorkerUserId == workerId))
            {
                var assignment = new WorkOrderAssignment
                {
                    WorkOrderId = workOrderId,
                    WorkerUserId = workerId,
                    AssignedAtUtc = DateTime.UtcNow
                };
                _context.WorkOrderAssignments.Add(assignment);

                // Notify technician
                _context.Notifications.Add(new Notification
                {
                    RecipientUserId = workerId,
                    Title = $"New Task Assigned: {workOrder.WorkOrderNumber}",
                    Message = $"You have been assigned to maintenance ticket {workOrder.Complaint?.TicketNumber}: '{workOrder.Complaint?.Title}'.",
                    TargetUrl = $"/Worker/Tasks/MyTasks",
                    NotificationType = "Info",
                    CreatedAtUtc = DateTime.UtcNow
                });
            }
        }

        workOrder.Status = WorkOrderStatus.Assigned;

        // Log audit history
        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = workOrder.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = workOrder.Complaint!.Status,
            NewStatus = ComplaintStatus.Assigned,
            Action = "TechniciansAllocated",
            Remarks = $"Department Supervisor allocated {workerUserIds.Count} technician(s) to Work Order {workOrder.WorkOrderNumber}.",
            TimestampUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Technician(s) successfully allocated to Work Order {workOrder.WorkOrderNumber}.";
        return RedirectToAction(nameof(Index));
    }
}
