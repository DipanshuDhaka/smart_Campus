using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Models.Entities;

public class WorkOrder
{
    [Key]
    public int WorkOrderId { get; set; }

    [Required]
    [MaxLength(30)]
    public string WorkOrderNumber { get; set; } = string.Empty; // e.g. "WO-2026-00018"

    public int ComplaintId { get; set; }

    [ForeignKey(nameof(ComplaintId))]
    public Complaint? Complaint { get; set; }

    public int DepartmentId { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    public int? TeamId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }

    public WorkOrderStatus Status { get; set; } = WorkOrderStatus.Assigned;

    public string? WorkSummary { get; set; } // Technician completion remarks / materials replaced

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? WorkStartedAtUtc { get; set; }
    public DateTime? WorkCompletedAtUtc { get; set; }

    public bool IsOverdue { get; set; } = false;

    public ICollection<WorkOrderAssignment> Assignments { get; set; } = new List<WorkOrderAssignment>();
    public ICollection<WorkOrderEvidence> Evidence { get; set; } = new List<WorkOrderEvidence>();
    public ICollection<Verification> Verifications { get; set; } = new List<Verification>();
    public ICollection<SLAEscalation> SLAEscalations { get; set; } = new List<SLAEscalation>();
}

public class WorkOrderAssignment
{
    [Key]
    public int AssignmentId { get; set; }

    public int WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    [Required]
    [MaxLength(450)]
    public string WorkerUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(WorkerUserId))]
    public IdentityUser? WorkerUser { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public class WorkOrderEvidence
{
    [Key]
    public int EvidenceId { get; set; }

    public int WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    [Required]
    [MaxLength(450)]
    public string WorkerUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(WorkerUserId))]
    public IdentityUser? WorkerUser { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty; // Completion photo

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}

public class Verification
{
    [Key]
    public int VerificationId { get; set; }

    public int WorkOrderId { get; set; }

    [ForeignKey(nameof(WorkOrderId))]
    public WorkOrder? WorkOrder { get; set; }

    [Required]
    [MaxLength(450)]
    public string VerifierUserId { get; set; } = string.Empty; // Supervisor / Team Head

    [ForeignKey(nameof(VerifierUserId))]
    public IdentityUser? VerifierUser { get; set; }

    public bool IsApproved { get; set; } // true = Approved (VerifiedResolved), false = Rejected (ReworkRequired)

    [Required]
    public string InspectionNotes { get; set; } = string.Empty;

    public string? DefectDescription { get; set; } // Mandatory if IsApproved == false

    public DateTime VerifiedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<VerificationImage> VerificationImages { get; set; } = new List<VerificationImage>();
}

public class VerificationImage
{
    [Key]
    public int VerificationImageId { get; set; }

    public int VerificationId { get; set; }

    [ForeignKey(nameof(VerificationId))]
    public Verification? Verification { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty; // Inspector physical proof photo

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
