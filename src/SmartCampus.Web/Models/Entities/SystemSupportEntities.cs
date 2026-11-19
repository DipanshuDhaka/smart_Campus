using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Models.Entities;

public class SLAEscalation
{
    [Key]
    public int EscalationId { get; set; }

    public int WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    public EscalationTier EscalationTier { get; set; }

    public DateTime TriggeredAtUtc { get; set; } = DateTime.UtcNow;

    public int BreachMinutes { get; set; }

    public bool IsAcknowledged { get; set; } = false;

    [MaxLength(450)]
    public string? AcknowledgedByUserId { get; set; }

    [ForeignKey(nameof(AcknowledgedByUserId))]
    public IdentityUser? AcknowledgedByUser { get; set; }
}

public class ComplaintFeedback
{
    [Key]
    public int FeedbackId { get; set; }

    public int ComplaintId { get; set; }

    [ForeignKey(nameof(ComplaintId))]
    public Complaint? Complaint { get; set; }

    [Required]
    [MaxLength(450)]
    public string ReporterUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(ReporterUserId))]
    public IdentityUser? ReporterUser { get; set; }

    [Range(1, 5)]
    public int RatingStars { get; set; } = 5;

    [MaxLength(1000)]
    public string? Comments { get; set; }

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
}

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    [Required]
    [MaxLength(450)]
    public string RecipientUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(RecipientUserId))]
    public IdentityUser? RecipientUser { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TargetUrl { get; set; }

    [MaxLength(50)]
    public string NotificationType { get; set; } = "Info"; // "Info", "Warning", "Danger", "Success"

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    [Key]
    public long AuditId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityId { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    [MaxLength(50)]
    public string? IpAddress { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
