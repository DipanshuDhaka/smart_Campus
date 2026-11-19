using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "SuperAdmin,CampusAdmin")]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DepartmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .Include(d => d.Teams)
                .ThenInclude(t => t.Members)
            .Include(d => d.Teams)
                .ThenInclude(t => t.SupervisorUser)
            .Include(d => d.WorkOrders)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        return View(departments);
    }

    [HttpGet]
    public async Task<IActionResult> Teams(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Teams)
                .ThenInclude(t => t.SupervisorUser)
            .Include(d => d.Teams)
                .ThenInclude(t => t.Members)
                    .ThenInclude(m => m.WorkerUser)
            .FirstOrDefaultAsync(d => d.DepartmentId == id);

        if (department == null) return NotFound();

        // Get users in DepartmentSupervisor role for dropdown
        var supervisors = await _userManager.GetUsersInRoleAsync("DepartmentSupervisor");
        ViewBag.Supervisors = supervisors;

        return View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTeam(int departmentId, string teamName, string supervisorUserId)
    {
        if (string.IsNullOrWhiteSpace(teamName) || string.IsNullOrWhiteSpace(supervisorUserId))
        {
            TempData["ErrorMessage"] = "Team name and supervisor are required.";
            return RedirectToAction(nameof(Teams), new { id = departmentId });
        }

        var team = new Team
        {
            DepartmentId = departmentId,
            TeamName = teamName.Trim(),
            SupervisorUserId = supervisorUserId,
            IsActive = true
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Team '{teamName}' created successfully.";
        return RedirectToAction(nameof(Teams), new { id = departmentId });
    }

    [HttpGet]
    public async Task<IActionResult> Workers(int teamId)
    {
        var team = await _context.Teams
            .Include(t => t.Department)
            .Include(t => t.SupervisorUser)
            .Include(t => t.Members)
                .ThenInclude(m => m.WorkerUser)
            .FirstOrDefaultAsync(t => t.TeamId == teamId);

        if (team == null) return NotFound();

        var workersInRole = await _userManager.GetUsersInRoleAsync("MaintenanceWorker");
        ViewBag.AvailableWorkers = workersInRole;
        ViewBag.Skills = await _context.Skills.OrderBy(s => s.SkillName).ToListAsync();

        return View(team);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkerToTeam(int teamId, string workerUserId)
    {
        if (string.IsNullOrWhiteSpace(workerUserId))
        {
            TempData["ErrorMessage"] = "Please select a technician.";
            return RedirectToAction(nameof(Workers), new { teamId });
        }

        var exists = await _context.TeamMembers.AnyAsync(m => m.TeamId == teamId && m.WorkerUserId == workerUserId);
        if (exists)
        {
            TempData["ErrorMessage"] = "Worker is already a member of this trade team.";
            return RedirectToAction(nameof(Workers), new { teamId });
        }

        _context.TeamMembers.Add(new TeamMember
        {
            TeamId = teamId,
            WorkerUserId = workerUserId,
            AvailabilityStatus = WorkerAvailability.Available,
            IsActive = true
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Technician successfully added to team roster.";
        return RedirectToAction(nameof(Workers), new { teamId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAvailability(int memberId, WorkerAvailability status)
    {
        var member = await _context.TeamMembers.FindAsync(memberId);
        if (member == null) return NotFound();

        member.AvailabilityStatus = status;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Worker availability updated to '{status}'.";
        return RedirectToAction(nameof(Workers), new { teamId = member.TeamId });
    }
}
