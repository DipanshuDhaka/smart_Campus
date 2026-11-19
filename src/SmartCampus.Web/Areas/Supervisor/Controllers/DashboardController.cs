using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Areas.Supervisor.Controllers;

[Area("Supervisor")]
[Authorize(Roles = "DepartmentSupervisor,CampusAdmin,SuperAdmin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.PendingVerification = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.AwaitingVerification);
        ViewBag.ActiveWorkOrders = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Assigned || w.Status == WorkOrderStatus.WorkAccepted || w.Status == WorkOrderStatus.InProgress);
        ViewBag.ReworkCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.ReworkRequired);
        ViewBag.VerifiedCount = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.VerifiedResolved);

        // Verification Queue (Tasks submitted by technicians with completion photos awaiting physical inspection)
        var verificationQueue = await _context.WorkOrders
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Location)
                    .ThenInclude(l => l!.Floor)
                        .ThenInclude(f => f!.Building)
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Images)
            .Include(w => w.Department)
            .Include(w => w.Assignments)
                .ThenInclude(a => a.WorkerUser)
            .Include(w => w.Evidence)
            .Where(w => w.Status == WorkOrderStatus.AwaitingVerification || w.Status == WorkOrderStatus.WorkCompleted)
            .OrderByDescending(w => w.WorkCompletedAtUtc)
            .Take(10)
            .ToListAsync();

        return View(verificationQueue);
    }
}
