using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Models.Entities;

public class Complaint
{
    [Key]
    public int ComplaintId { get; set; }

    [Required]
    [MaxLength(30)]
    public string TicketNumber { get; set; } = string.Empty; // e.g. "SC-2026-00042"

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public int LocationId { get; set; }

    [ForeignKey(nameof(LocationId))]
    public Location? Location { get; set; }

    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public IssueCategory? Category { get; set; }

    public int? PriorityId { get; set; }

    [ForeignKey(nameof(PriorityId))]
    public Priority? Priority { get; set; }

    [Required]
    [MaxLength(450)]
    public string ReporterUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(ReporterUserId))]
    public IdentityUser? ReporterUser { get; set; }

    [MaxLength(50)]
    public string? ReporterContactPhone { get; set; }

    public ComplaintStatus Status { get; set; } = ComplaintStatus.Submitted;

    public DateTime? ResponseDeadlineUtc { get; set; }
    public DateTime? WorkStartDeadlineUtc { get; set; }
    public DateTime? ResolutionDeadlineUtc { get; set; }

    public bool IsOverdue { get; set; } = false;

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ClosedAtUtc { get; set; }

    public ICollection<ComplaintImage> Images { get; set; } = new List<ComplaintImage>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    public ICollection<ComplaintHistory> History { get; set; } = new List<ComplaintHistory>();
    public ICollection<ComplaintFeedback> Feedbacks { get; set; } = new List<ComplaintFeedback>();
}

public class ComplaintImage
{
    [Key]
    public int ImageId { get; set; }

    public int ComplaintId { get; set; }

    [ForeignKey(nameof(ComplaintId))]
    public Complaint? Complaint { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty; // Relative URL e.g. /uploads/complaints/...

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}

public class ComplaintHistory
{
    [Key]
    public int HistoryId { get; set; }

    public int ComplaintId { get; set; }

    [ForeignKey(nameof(ComplaintId))]
    public Complaint? Complaint { get; set; }

    [Required]
    [MaxLength(450)]
    public string ActorUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(ActorUserId))]
    public IdentityUser? ActorUser { get; set; }

    public ComplaintStatus PreviousStatus { get; set; }
    public ComplaintStatus NewStatus { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty; // e.g. "TriageApproved", "AssignedToWorker", "VerifiedBySupervisor"

    public string? Remarks { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
