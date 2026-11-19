using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;
using SmartCampus.Web.Services;

namespace SmartCampus.Web.Areas.Reporter.Controllers;

[Area("Reporter")]
[Authorize(Roles = "StudentTeacher,CampusAdmin,SuperAdmin")]
public class ComplaintsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storageService;

    public ComplaintsController(ApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var complaints = await _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.Priority)
            .Include(c => c.Location)
                .ThenInclude(l => l!.Floor)
                    .ThenInclude(f => f!.Building)
            .Where(c => c.ReporterUserId == currentUserId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync();

        return View(complaints);
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? locId, int? buildingId)
    {
        ViewBag.Categories = await _context.IssueCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        ViewBag.Buildings = await _context.Buildings
            .Where(b => b.IsActive)
            .OrderBy(b => b.BuildingCode)
            .ToListAsync();

        if (locId.HasValue)
        {
            var location = await _context.Locations
                .Include(l => l.Floor)
                    .ThenInclude(f => f!.Building)
                .FirstOrDefaultAsync(l => l.LocationGuid == locId.Value && l.IsActive);

            if (location != null)
            {
                ViewBag.LockedLocation = location;
                ViewBag.LockedBuildingId = location.Floor?.BuildingId;
                ViewBag.LockedFloorId = location.FloorId;
                ViewBag.LockedLocationId = location.LocationId;
            }
        }
        else if (buildingId.HasValue)
        {
            ViewBag.SelectedBuildingId = buildingId.Value;
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string title,
        string description,
        int categoryId,
        int locationId,
        string? contactPhone,
        List<IFormFile>? photos)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description) || locationId <= 0 || categoryId <= 0)
        {
            TempData["ErrorMessage"] = "Please fill in all mandatory fields (Title, Description, Category, Location).";
            return RedirectToAction(nameof(Create));
        }

        // Generate unique ticket number
        var year = DateTime.UtcNow.Year;
        var count = await _context.Complaints.CountAsync() + 1;
        var ticketNumber = $"SC-{year}-{count:D5}";

        // Defensively sanitize and truncate input strings to prevent SQL truncation errors
        var safeTitle = title.Trim();
        if (safeTitle.Length > 200) safeTitle = safeTitle[..200];

        string? safePhone = null;
        if (!string.IsNullOrWhiteSpace(contactPhone))
        {
            var trimmed = contactPhone.Trim();
            safePhone = trimmed.Length > 50 ? trimmed[..50] : trimmed;
        }

        var complaint = new Complaint
        {
            TicketNumber = ticketNumber,
            Title = safeTitle,
            Description = description.Trim(),
            LocationId = locationId,
            CategoryId = categoryId,
            ReporterUserId = currentUserId!,
            ReporterContactPhone = safePhone,
            Status = ComplaintStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();

        // Handle uploaded issue photographs (up to 3)
        if (photos != null && photos.Count > 0)
        {
            foreach (var photo in photos.Take(3))
            {
                var (success, fileUrl, error) = await _storageService.SaveImageAsync(photo, "complaints");
                if (success && fileUrl != null)
                {
                    _context.ComplaintImages.Add(new ComplaintImage
                    {
                        ComplaintId = complaint.ComplaintId,
                        ImageUrl = fileUrl,
                        FileSizeBytes = photo.Length,
                        UploadedAtUtc = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        // Log to immutable ComplaintHistory
        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = complaint.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.Submitted,
            NewStatus = ComplaintStatus.Submitted,
            Action = "IssueReported",
            Remarks = "Complaint officially logged into campus maintenance queue.",
            TimestampUtc = DateTime.UtcNow
        });

        // Create in-app notification
        _context.Notifications.Add(new Notification
        {
            RecipientUserId = currentUserId!,
            Title = $"Issue Logged: {ticketNumber}",
            Message = $"Your maintenance report for '{complaint.Title}' has been received by the Campus Control Desk.",
            TargetUrl = $"/Reporter/Complaints/Details/{complaint.ComplaintId}",
            NotificationType = "Info",
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Complaint {ticketNumber} submitted successfully. Track resolution progress below.";
        return RedirectToAction(nameof(Details), new { id = complaint.ComplaintId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var complaint = await _context.Complaints
            .Include(c => c.Category)
            .Include(c => c.Priority)
            .Include(c => c.Location)
                .ThenInclude(l => l!.Floor)
                    .ThenInclude(f => f!.Building)
            .Include(c => c.Images)
            .Include(c => c.WorkOrders)
                .ThenInclude(w => w.Department)
            .Include(c => c.WorkOrders)
                .ThenInclude(w => w.Evidence)
            .Include(c => c.WorkOrders)
                .ThenInclude(w => w.Verifications)
            .Include(c => c.History)
                .ThenInclude(h => h.ActorUser)
            .Include(c => c.Feedbacks)
            .FirstOrDefaultAsync(c => c.ComplaintId == id);

        if (complaint == null) return NotFound();

        // Enforce RBAC data scoping: reporters can only view their own complaints unless admin
        if (complaint.ReporterUserId != currentUserId && !User.IsInRole("CampusAdmin") && !User.IsInRole("SuperAdmin"))
        {
            return Forbid();
        }

        return View(complaint);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitFeedback(int complaintId, int ratingStars, string? comments)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId && c.ReporterUserId == currentUserId);

        if (complaint == null) return NotFound();

        if (complaint.Status != ComplaintStatus.VerifiedResolved)
        {
            TempData["ErrorMessage"] = "Feedback can only be submitted after supervisor independent verification.";
            return RedirectToAction(nameof(Details), new { id = complaintId });
        }

        _context.ComplaintFeedbacks.Add(new ComplaintFeedback
        {
            ComplaintId = complaintId,
            ReporterUserId = currentUserId!,
            RatingStars = Math.Clamp(ratingStars, 1, 5),
            Comments = comments?.Trim(),
            SubmittedAtUtc = DateTime.UtcNow
        });

        complaint.Status = ComplaintStatus.Closed;
        complaint.ClosedAtUtc = DateTime.UtcNow;

        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = complaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.VerifiedResolved,
            NewStatus = ComplaintStatus.Closed,
            Action = "FeedbackSubmitted",
            Remarks = $"Reporter submitted {ratingStars}-star feedback. Ticket formally closed.",
            TimestampUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Thank you for your rating! Your complaint is now closed.";
        return RedirectToAction(nameof(Details), new { id = complaintId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReopenTicket(int complaintId, string disputeReason)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var complaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.ComplaintId == complaintId && c.ReporterUserId == currentUserId);

        if (complaint == null) return NotFound();

        if (string.IsNullOrWhiteSpace(disputeReason))
        {
            TempData["ErrorMessage"] = "Please specify the reason for reopening this ticket.";
            return RedirectToAction(nameof(Details), new { id = complaintId });
        }

        complaint.Status = ComplaintStatus.Reopened;

        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = complaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.VerifiedResolved,
            NewStatus = ComplaintStatus.Reopened,
            Action = "DisputedAndReopened",
            Remarks = $"Reporter disputed resolution: {disputeReason.Trim()}",
            TimestampUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Ticket reopened and escalated back to Control Desk for review.";
        return RedirectToAction(nameof(Details), new { id = complaintId });
    }
}
