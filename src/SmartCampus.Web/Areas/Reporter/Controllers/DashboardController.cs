using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Areas.Reporter.Controllers;

[Area("Reporter")]
[Authorize(Roles = "StudentTeacher,CampusAdmin,SuperAdmin")]
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

        var myComplaints = await _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.Priority)
            .Include(c => c.Location)
                .ThenInclude(l => l!.Floor)
                    .ThenInclude(f => f!.Building)
            .Where(c => c.ReporterUserId == currentUserId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync();

        ViewBag.TotalLogged = myComplaints.Count;
        ViewBag.ActiveCount = myComplaints.Count(c => c.Status != ComplaintStatus.VerifiedResolved && c.Status != ComplaintStatus.Closed && c.Status != ComplaintStatus.Rejected);
        ViewBag.ResolvedCount = myComplaints.Count(c => c.Status == ComplaintStatus.VerifiedResolved || c.Status == ComplaintStatus.Closed);

        return View(myComplaints);
    }
}
