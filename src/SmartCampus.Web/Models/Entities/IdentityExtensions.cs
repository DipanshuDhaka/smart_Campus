using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SmartCampus.Web.Models.Entities;

public class AllowedEmailDomain
{
    [Key]
    public int DomainId { get; set; }

    [Required]
    [MaxLength(150)]
    public string DomainName { get; set; } = string.Empty; // e.g. "lpu.co.in", "lpu.in", "gmail.com"

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(50)]
    public string DefaultRole { get; set; } = "StudentTeacher";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class UserProfile
{
    [Key]
    public int ProfileId { get; set; }

    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public IdentityUser? User { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; } // LPU Student Reg No or Staff ID, e.g. 12201948

    [MaxLength(50)]
    public string? PhoneNumber { get; set; }

    public int? DepartmentId { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public Department? Department { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    public bool IsApprovedByAdmin { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
