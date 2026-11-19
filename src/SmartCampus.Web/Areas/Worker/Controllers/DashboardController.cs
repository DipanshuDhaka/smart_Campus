using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Areas.Worker.Controllers;

[Area("Worker")]
[Authorize(Roles = "MaintenanceWorker")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Fetch tasks assigned to the logged-in technician
        var myTasks = await _context.WorkOrderAssignments
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
                    .ThenInclude(c => c!.Location)
                        .ThenInclude(l => l!.Floor)
                            .ThenInclude(f => f!.Building)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Complaint)
                    .ThenInclude(c => c!.Priority)
            .Include(a => a.WorkOrder)
                .ThenInclude(w => w!.Evidence)
            .Where(a => a.WorkerUserId == currentUserId)
            .OrderByDescending(a => a.AssignedAtUtc)
            .ToListAsync();

        ViewBag.TotalAssigned = myTasks.Count;
        ViewBag.InProgress = myTasks.Count(t => t.WorkOrder?.Status == WorkOrderStatus.InProgress);
        ViewBag.SubmittedVerification = myTasks.Count(t => t.WorkOrder?.Status == WorkOrderStatus.WorkCompleted || t.WorkOrder?.Status == WorkOrderStatus.AwaitingVerification);
        ViewBag.VerifiedResolved = myTasks.Count(t => t.WorkOrder?.Status == WorkOrderStatus.VerifiedResolved);

        return View(myTasks);
    }
}
