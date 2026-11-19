using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,CampusAdmin")]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalComplaints = await _context.Complaints.CountAsync();
        ViewBag.PendingTriage = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.Submitted || c.Status == ComplaintStatus.UnderReview);
        ViewBag.ActiveWorkOrders = await _context.WorkOrders.CountAsync(w => w.Status == WorkOrderStatus.Assigned || w.Status == WorkOrderStatus.WorkAccepted || w.Status == WorkOrderStatus.InProgress);
        ViewBag.AwaitingVerification = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.AwaitingVerification);
        ViewBag.OverdueBreaches = await _context.Complaints.CountAsync(c => c.IsOverdue);
        ViewBag.ResolvedCount = await _context.Complaints.CountAsync(c => c.Status == ComplaintStatus.VerifiedResolved || c.Status == ComplaintStatus.Closed);

        var recentComplaints = await _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.Priority)
            .Include(c => c.Location)
                .ThenInclude(l => l!.Floor)
                    .ThenInclude(f => f!.Building)
            .Include(c => c.ReporterUser)
            .OrderByDescending(c => c.CreatedAtUtc)
            .Take(10)
            .ToListAsync();

        var departments = await _context.Departments
            .Include(d => d.WorkOrders)
            .Take(6)
            .ToListAsync();

        ViewBag.Departments = departments;

        return View(recentComplaints);
    }
}
