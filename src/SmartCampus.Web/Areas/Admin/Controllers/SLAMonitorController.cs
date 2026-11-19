using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,CampusAdmin")]
public class SLAMonitorController : Controller
{
    private readonly ApplicationDbContext _context;

    public SLAMonitorController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalComplaints = await _context.Complaints.CountAsync();
        var overdueComplaints = await _context.Complaints
            .Include(c => c.Location)
                .ThenInclude(l => l!.Floor)
                    .ThenInclude(f => f!.Building)
            .Include(c => c.Category)
            .Include(c => c.Priority)
            .Where(c => c.IsOverdue)
            .OrderByDescending(c => c.ResolutionDeadlineUtc)
            .ToListAsync();

        var resolvedComplaints = await _context.Complaints
            .Where(c => c.Status == ComplaintStatus.VerifiedResolved || c.Status == ComplaintStatus.Closed)
            .ToListAsync();

        var onTimeResolved = resolvedComplaints
            .Count(c => c.ResolvedAtUtc.HasValue && c.ResolutionDeadlineUtc.HasValue && c.ResolvedAtUtc <= c.ResolutionDeadlineUtc);

        var complianceRate = resolvedComplaints.Count > 0 
            ? (double)onTimeResolved / resolvedComplaints.Count * 100.0 
            : 100.0;

        var escalations = await _context.SLAEscalations
            .Include(e => e.WorkOrder)
                .ThenInclude(w => w!.Complaint)
            .OrderByDescending(e => e.TriggeredAtUtc)
            .Take(20)
            .ToListAsync();

        ViewBag.TotalComplaints = totalComplaints;
        ViewBag.OverdueCount = overdueComplaints.Count;
        ViewBag.ComplianceRate = complianceRate.ToString("F1");
        ViewBag.Escalations = escalations;

        return View(overdueComplaints);
    }
}
