using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Models.Entities;

public class Department
{
    [Key]
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(20)]
    public string DepartmentCode { get; set; } = string.Empty; // "ELEC", "PLMB", "HVAC", "CIVIL", "IT"

    [Required]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty; // "Electrical & Power Maintenance"

    [MaxLength(150)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public class Team
{
    [Key]
    public int TeamId { get; set; }

    public int DepartmentId { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    [Required]
    [MaxLength(100)]
    public string TeamName { get; set; } = string.Empty; // e.g. "Academic Block Electrical Squad A"

    [Required]
    [MaxLength(450)]
    public string SupervisorUserId { get; set; } = string.Empty; // Designated Team Head

    [ForeignKey(nameof(SupervisorUserId))]
    public IdentityUser? SupervisorUser { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
}

public class TeamMember
{
    [Key]
    public int TeamMemberId { get; set; }

    public int TeamId { get; set; }

    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }

    [Required]
    [MaxLength(450)]
    public string WorkerUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(WorkerUserId))]
    public IdentityUser? WorkerUser { get; set; }

    public WorkerAvailability AvailabilityStatus { get; set; } = WorkerAvailability.Available;

    public bool IsActive { get; set; } = true;
}

public class Skill
{
    [Key]
    public int SkillId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SkillName { get; set; } = string.Empty; // e.g. "High Voltage Certification", "Industrial Plumbing", "Split AC Servicing"

    [MaxLength(50)]
    public string TradeCategory { get; set; } = string.Empty; // "Electrical", "Plumbing", etc.

    public ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();
}

public class WorkerSkill
{
    [Key]
    public int WorkerSkillId { get; set; }

    [Required]
    [MaxLength(450)]
    public string WorkerUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(WorkerUserId))]
    public IdentityUser? WorkerUser { get; set; }

    public int SkillId { get; set; }

    [ForeignKey(nameof(SkillId))]
    public Skill? Skill { get; set; }

    public int ProficiencyLevel { get; set; } = 1; // 1=Junior, 2=Senior, 3=Master/Specialist
}
