using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Models.Entities;

public class Priority
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PriorityId { get; set; } // 1=Critical, 2=High, 3=Medium, 4=Low

    [Required]
    [MaxLength(50)]
    public string PriorityName { get; set; } = string.Empty;

    public int SeverityLevel { get; set; }

    [MaxLength(20)]
    public string BadgeColor { get; set; } = "secondary"; // "danger", "warning", "info", "secondary"

    public SLAPolicy? SLAPolicy { get; set; }

    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}

public class SLAPolicy
{
    [Key]
    public int PolicyId { get; set; }

    public int PriorityId { get; set; }

    [ForeignKey(nameof(PriorityId))]
    public Priority? Priority { get; set; }

    public int ResponseTargetMinutes { get; set; } // Control desk review target

    public int WorkStartTargetMinutes { get; set; } // Worker check-in target

    public int ResolutionTargetMinutes { get; set; } // Complete verified resolution target

    public bool IsActive { get; set; } = true;
}

public class IssueCategory
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty; // e.g. "Electrical", "Plumbing", "HVAC", "Civil", "IT/Network"

    [MaxLength(255)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string IconCssClass { get; set; } = "bi-tools";

    public int DefaultDepartmentId { get; set; }

    [ForeignKey(nameof(DefaultDepartmentId))]
    public Department? DefaultDepartment { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<IssueSubcategory> Subcategories { get; set; } = new List<IssueSubcategory>();
    public ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}

public class IssueSubcategory
{
    [Key]
    public int SubcategoryId { get; set; }

    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public IssueCategory? Category { get; set; }

    [Required]
    [MaxLength(100)]
    public string SubcategoryName { get; set; } = string.Empty; // e.g. "Broken Ceiling Fan", "Water Tap Leakage", "Exposed Wire"

    public int? DefaultPriorityId { get; set; }

    [ForeignKey(nameof(DefaultPriorityId))]
    public Priority? DefaultPriority { get; set; }

    public bool IsActive { get; set; } = true;
}
