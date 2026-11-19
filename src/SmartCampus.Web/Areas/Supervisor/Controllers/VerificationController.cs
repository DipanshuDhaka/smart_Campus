using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Data;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;
using SmartCampus.Web.Services;

namespace SmartCampus.Web.Areas.Supervisor.Controllers;

[Area("Supervisor")]
[Authorize(Roles = "DepartmentSupervisor,CampusAdmin,SuperAdmin")]
public class VerificationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _storageService;

    public VerificationController(ApplicationDbContext context, IFileStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var queue = await _context.WorkOrders
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Location)
                    .ThenInclude(l => l!.Floor)
                        .ThenInclude(f => f!.Building)
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.Images)
            .Include(w => w.Evidence)
            .Include(w => w.Assignments)
                .ThenInclude(a => a.WorkerUser)
            .Where(w => w.Status == WorkOrderStatus.AwaitingVerification || w.Status == WorkOrderStatus.WorkCompleted)
            .OrderByDescending(w => w.WorkCompletedAtUtc)
            .ToListAsync();

        return View(queue);
    }

    [HttpGet]
    public async Task<IActionResult> Inspect(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
                .ThenInclude(a => a.WorkerUser)
            .Include(w => w.Evidence)
            .Include(w => w.Verifications)
            .FirstOrDefaultAsync(w => w.WorkOrderId == id);

        if (workOrder == null) return NotFound();

        // CARDINAL SECURITY RULE: A worker must not approve their own completed work!
        if (workOrder.Assignments.Any(a => a.WorkerUserId == currentUserId) && !User.IsInRole("SuperAdmin"))
        {
            TempData["ErrorMessage"] = "Security Violation: A technician is strictly prohibited from verifying or approving their own completed work!";
            return RedirectToAction(nameof(Index));
        }

        return View(workOrder);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int workOrderId, string inspectionNotes, IFormFile? verificationPhoto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var workOrder = await _context.WorkOrders
            .Include(w => w.Complaint)
                .ThenInclude(c => c!.WorkOrders)
            .Include(w => w.Assignments)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId);

        if (workOrder == null) return NotFound();

        // CARDINAL SECURITY RULE ENFORCEMENT
        if (workOrder.Assignments.Any(a => a.WorkerUserId == currentUserId) && !User.IsInRole("SuperAdmin"))
        {
            TempData["ErrorMessage"] = "Security Violation: A technician is strictly prohibited from verifying or approving their own completed work!";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(inspectionNotes))
        {
            TempData["ErrorMessage"] = "Mandatory: Enter on-site inspection sign-off remarks.";
            return RedirectToAction(nameof(Inspect), new { id = workOrderId });
        }

        var verification = new Verification
        {
            WorkOrderId = workOrderId,
            VerifierUserId = currentUserId!,
            IsApproved = true,
            InspectionNotes = inspectionNotes.Trim(),
            VerifiedAtUtc = DateTime.UtcNow
        };

        _context.Verifications.Add(verification);
        await _context.SaveChangesAsync();

        // Save inspector verification proof photo if attached
        if (verificationPhoto != null)
        {
            var (success, fileUrl, error) = await _storageService.SaveImageAsync(verificationPhoto, "verification");
            if (success && fileUrl != null)
            {
                _context.VerificationImages.Add(new VerificationImage
                {
                    VerificationId = verification.VerificationId,
                    ImageUrl = fileUrl,
                    UploadedAtUtc = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        workOrder.Status = WorkOrderStatus.VerifiedResolved;

        // Check if all sibling work orders are verified
        var allVerified = workOrder.Complaint?.WorkOrders.All(w => w.WorkOrderId == workOrderId || w.Status == WorkOrderStatus.VerifiedResolved) ?? true;
        if (allVerified && workOrder.Complaint != null)
        {
            workOrder.Complaint.Status = ComplaintStatus.VerifiedResolved;
            workOrder.Complaint.ResolvedAtUtc = DateTime.UtcNow;

            // Notify original reporter
            _context.Notifications.Add(new Notification
            {
                RecipientUserId = workOrder.Complaint.ReporterUserId,
                Title = $"Issue Resolved & Verified: {workOrder.Complaint.TicketNumber}",
                Message = $"Your maintenance complaint '{workOrder.Complaint.Title}' has been independently inspected and approved by the Department Supervisor. Please rate our service.",
                TargetUrl = $"/Reporter/Complaints/Details/{workOrder.Complaint.ComplaintId}",
                NotificationType = "Success",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        // Audit History
        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = workOrder.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.AwaitingVerification,
            NewStatus = allVerified ? ComplaintStatus.VerifiedResolved : ComplaintStatus.AwaitingVerification,
            Action = "IndependentVerificationApproved",
            Remarks = $"Department Supervisor independently inspected and signed off Work Order {workOrder.WorkOrderNumber}. Notes: {inspectionNotes.Trim()}",
            TimestampUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Work Order {workOrder.WorkOrderNumber} has been successfully verified and resolved!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRework(int workOrderId, string defectDescription, IFormFile? defectPhoto)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var workOrder = await _context.WorkOrders
            .Include(w => w.Complaint)
            .Include(w => w.Assignments)
            .FirstOrDefaultAsync(w => w.WorkOrderId == workOrderId);

        if (workOrder == null) return NotFound();

        if (string.IsNullOrWhiteSpace(defectDescription))
        {
            TempData["ErrorMessage"] = "Mandatory: Detail the defects requiring rework.";
            return RedirectToAction(nameof(Inspect), new { id = workOrderId });
        }

        var verification = new Verification
        {
            WorkOrderId = workOrderId,
            VerifierUserId = currentUserId!,
            IsApproved = false,
            InspectionNotes = "Physical inspection failed. Rework mandated.",
            DefectDescription = defectDescription.Trim(),
            VerifiedAtUtc = DateTime.UtcNow
        };

        _context.Verifications.Add(verification);
        await _context.SaveChangesAsync();

        if (defectPhoto != null)
        {
            var (success, fileUrl, error) = await _storageService.SaveImageAsync(defectPhoto, "verification");
            if (success && fileUrl != null)
            {
                _context.VerificationImages.Add(new VerificationImage
                {
                    VerificationId = verification.VerificationId,
                    ImageUrl = fileUrl,
                    UploadedAtUtc = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }

        workOrder.Status = WorkOrderStatus.ReworkRequired;
        if (workOrder.Complaint != null)
        {
            workOrder.Complaint.Status = ComplaintStatus.ReworkRequired;
        }

        // Notify assigned technicians
        foreach (var assignment in workOrder.Assignments)
        {
            _context.Notifications.Add(new Notification
            {
                RecipientUserId = assignment.WorkerUserId,
                Title = $"Rework Mandated: {workOrder.WorkOrderNumber}",
                Message = $"Supervisor rejected repair during physical inspection. Defect notes: '{defectDescription.Trim()}'. Please rectify on site.",
                TargetUrl = $"/Worker/Tasks/MyTasks",
                NotificationType = "Danger",
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        // Audit History
        _context.ComplaintHistories.Add(new ComplaintHistory
        {
            ComplaintId = workOrder.ComplaintId,
            ActorUserId = currentUserId!,
            PreviousStatus = ComplaintStatus.AwaitingVerification,
            NewStatus = ComplaintStatus.ReworkRequired,
            Action = "IndependentVerificationRejected",
            Remarks = $"Supervisor rejected repair during on-site inspection. Defects: {defectDescription.Trim()}. Status set to Rework Required.",
            TimestampUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Work Order {workOrder.WorkOrderNumber} rejected and sent back to technician for mandatory rework.";
        return RedirectToAction(nameof(Index));
    }
}
