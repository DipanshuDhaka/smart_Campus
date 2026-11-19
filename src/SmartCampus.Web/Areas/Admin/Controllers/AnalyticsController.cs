using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,CampusAdmin")]
public class AnalyticsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AnalyticsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalComplaints = await _context.Complaints.CountAsync();
        var resolvedComplaints = await _context.Complaints
            .Where(c => c.Status == ComplaintStatus.VerifiedResolved || c.Status == ComplaintStatus.Closed)
            .ToListAsync();

        var onTimeCount = resolvedComplaints
            .Count(c => c.ResolvedAtUtc.HasValue && c.ResolutionDeadlineUtc.HasValue && c.ResolvedAtUtc <= c.ResolutionDeadlineUtc);

        var complianceRate = resolvedComplaints.Count > 0 
            ? (double)onTimeCount / resolvedComplaints.Count * 100.0 
            : 100.0;

        // Calculate MTTR in hours
        var totalResolutionHours = resolvedComplaints
            .Where(c => c.ResolvedAtUtc.HasValue)
            .Select(c => (c.ResolvedAtUtc!.Value - c.CreatedAtUtc).TotalHours)
            .ToList();

        var averageMttrHours = totalResolutionHours.Count > 0 
            ? totalResolutionHours.Average() 
            : 0.0;

        // Average user rating
        var ratings = await _context.ComplaintFeedbacks.Select(f => f.RatingStars).ToListAsync();
        var averageRating = ratings.Count > 0 ? ratings.Average() : 5.0;

        // 1. Category Distribution
        var categoryStats = await _context.Complaints
            .Include(c => c.Category)
            .GroupBy(c => c.Category != null ? c.Category.CategoryName : "Other")
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        // 2. Department Workload
        var departmentStats = await _context.Departments
            .Include(d => d.WorkOrders)
            .Select(d => new
            {
                d.DepartmentCode,
                Total = d.WorkOrders.Count,
                Resolved = d.WorkOrders.Count(w => w.Status == WorkOrderStatus.VerifiedResolved)
            })
            .ToListAsync();

        // 3. Top Problem Hotspots (Buildings)
        var buildingHotspots = await _context.Complaints
            .Include(c => c.Location)
                .ThenInclude(l => l!.Floor)
                    .ThenInclude(f => f!.Building)
            .Where(c => c.Location != null && c.Location.Floor != null && c.Location.Floor.Building != null)
            .GroupBy(c => c.Location!.Floor!.Building!.BuildingCode)
            .Select(g => new { Building = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalComplaints = totalComplaints;
        ViewBag.ResolvedComplaints = resolvedComplaints.Count;
        ViewBag.ComplianceRate = complianceRate.ToString("F1");
        ViewBag.AverageMttrHours = averageMttrHours.ToString("F1");
        ViewBag.AverageRating = averageRating.ToString("F1");

        ViewBag.CategoryLabels = categoryStats.Select(c => c.Category).ToList();
        ViewBag.CategoryCounts = categoryStats.Select(c => c.Count).ToList();

        ViewBag.DeptLabels = departmentStats.Select(d => d.DepartmentCode).ToList();
        ViewBag.DeptTotal = departmentStats.Select(d => d.Total).ToList();
        ViewBag.DeptResolved = departmentStats.Select(d => d.Resolved).ToList();

        ViewBag.HotspotLabels = buildingHotspots.Select(h => h.Building).ToList();
        ViewBag.HotspotCounts = buildingHotspots.Select(h => h.Count).ToList();

        return View();
    }
}
