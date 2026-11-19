using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Models.Entities;

namespace SmartCampus.Web.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<AllowedEmailDomain> AllowedEmailDomains => Set<AllowedEmailDomain>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Location> Locations => Set<Location>();

    public DbSet<Priority> Priorities => Set<Priority>();
    public DbSet<SLAPolicy> SLAPolicies => Set<SLAPolicy>();
    public DbSet<IssueCategory> IssueCategories => Set<IssueCategory>();
    public DbSet<IssueSubcategory> IssueSubcategories => Set<IssueSubcategory>();

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<WorkerSkill> WorkerSkills => Set<WorkerSkill>();

    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<ComplaintImage> ComplaintImages => Set<ComplaintImage>();
    public DbSet<ComplaintHistory> ComplaintHistories => Set<ComplaintHistory>();

    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderAssignment> WorkOrderAssignments => Set<WorkOrderAssignment>();
    public DbSet<WorkOrderEvidence> WorkOrderEvidences => Set<WorkOrderEvidence>();

    public DbSet<Verification> Verifications => Set<Verification>();
    public DbSet<VerificationImage> VerificationImages => Set<VerificationImage>();

    public DbSet<SLAEscalation> SLAEscalations => Set<SLAEscalation>();
    public DbSet<ComplaintFeedback> ComplaintFeedbacks => Set<ComplaintFeedback>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // AllowedEmailDomains
        builder.Entity<AllowedEmailDomain>()
            .HasIndex(d => d.DomainName)
            .IsUnique();

        // Buildings
        builder.Entity<Building>()
            .HasIndex(b => b.BuildingCode)
            .IsUnique();

        // Locations
        builder.Entity<Location>()
            .HasIndex(l => l.LocationGuid)
            .IsUnique();

        builder.Entity<Location>()
            .HasOne(l => l.Floor)
            .WithMany(f => f.Locations)
            .HasForeignKey(l => l.FloorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Floors
        builder.Entity<Floor>()
            .HasOne(f => f.Building)
            .WithMany(b => b.Floors)
            .HasForeignKey(f => f.BuildingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Departments
        builder.Entity<Department>()
            .HasIndex(d => d.DepartmentCode)
            .IsUnique();

        // Issue Categories
        builder.Entity<IssueCategory>()
            .HasOne(c => c.DefaultDepartment)
            .WithMany()
            .HasForeignKey(c => c.DefaultDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Teams
        builder.Entity<Team>()
            .HasOne(t => t.Department)
            .WithMany(d => d.Teams)
            .HasForeignKey(t => t.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Team>()
            .HasOne(t => t.SupervisorUser)
            .WithMany()
            .HasForeignKey(t => t.SupervisorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // TeamMembers
        builder.Entity<TeamMember>()
            .HasOne(tm => tm.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TeamMember>()
            .HasOne(tm => tm.WorkerUser)
            .WithMany()
            .HasForeignKey(tm => tm.WorkerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkerSkills
        builder.Entity<WorkerSkill>()
            .HasOne(ws => ws.WorkerUser)
            .WithMany()
            .HasForeignKey(ws => ws.WorkerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WorkerSkill>()
            .HasOne(ws => ws.Skill)
            .WithMany(s => s.WorkerSkills)
            .HasForeignKey(ws => ws.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        // Complaints
        builder.Entity<Complaint>()
            .HasIndex(c => c.TicketNumber)
            .IsUnique();

        builder.Entity<Complaint>()
            .HasIndex(c => new { c.ReporterUserId, c.Status, c.CreatedAtUtc });

        builder.Entity<Complaint>()
            .HasIndex(c => new { c.Status, c.PriorityId, c.LocationId });

        builder.Entity<Complaint>()
            .HasIndex(c => new { c.LocationId, c.CategoryId, c.CreatedAtUtc });

        builder.Entity<Complaint>()
            .HasOne(c => c.Location)
            .WithMany(l => l.Complaints)
            .HasForeignKey(c => c.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Complaint>()
            .HasOne(c => c.Category)
            .WithMany(cat => cat.Complaints)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Complaint>()
            .HasOne(c => c.Priority)
            .WithMany(p => p.Complaints)
            .HasForeignKey(c => c.PriorityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Complaint>()
            .HasOne(c => c.ReporterUser)
            .WithMany()
            .HasForeignKey(c => c.ReporterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ComplaintImages
        builder.Entity<ComplaintImage>()
            .HasOne(ci => ci.Complaint)
            .WithMany(c => c.Images)
            .HasForeignKey(ci => ci.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        // ComplaintHistory
        builder.Entity<ComplaintHistory>()
            .HasOne(ch => ch.Complaint)
            .WithMany(c => c.History)
            .HasForeignKey(ch => ch.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ComplaintHistory>()
            .HasOne(ch => ch.ActorUser)
            .WithMany()
            .HasForeignKey(ch => ch.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkOrders
        builder.Entity<WorkOrder>()
            .HasIndex(w => w.WorkOrderNumber)
            .IsUnique();

        builder.Entity<WorkOrder>()
            .HasIndex(w => new { w.Status, w.IsOverdue });

        builder.Entity<WorkOrder>()
            .HasOne(w => w.Complaint)
            .WithMany(c => c.WorkOrders)
            .HasForeignKey(w => w.ComplaintId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WorkOrder>()
            .HasOne(w => w.Department)
            .WithMany(d => d.WorkOrders)
            .HasForeignKey(w => w.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WorkOrder>()
            .HasOne(w => w.Team)
            .WithMany(t => t.WorkOrders)
            .HasForeignKey(w => w.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkOrderAssignments
        builder.Entity<WorkOrderAssignment>()
            .HasIndex(wa => new { wa.WorkerUserId, wa.AcceptedAtUtc, wa.CompletedAtUtc });

        builder.Entity<WorkOrderAssignment>()
            .HasOne(wa => wa.WorkOrder)
            .WithMany(wo => wo.Assignments)
            .HasForeignKey(wa => wa.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WorkOrderAssignment>()
            .HasOne(wa => wa.WorkerUser)
            .WithMany()
            .HasForeignKey(wa => wa.WorkerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // WorkOrderEvidence
        builder.Entity<WorkOrderEvidence>()
            .HasOne(we => we.WorkOrder)
            .WithMany(wo => wo.Evidence)
            .HasForeignKey(we => we.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<WorkOrderEvidence>()
            .HasOne(we => we.WorkerUser)
            .WithMany()
            .HasForeignKey(we => we.WorkerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Verifications
        builder.Entity<Verification>()
            .HasOne(v => v.WorkOrder)
            .WithMany(wo => wo.Verifications)
            .HasForeignKey(v => v.WorkOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Verification>()
            .HasOne(v => v.VerifierUser)
            .WithMany()
            .HasForeignKey(v => v.VerifierUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // VerificationImages
        builder.Entity<VerificationImage>()
            .HasOne(vi => vi.Verification)
            .WithMany(v => v.VerificationImages)
            .HasForeignKey(vi => vi.VerificationId)
            .OnDelete(DeleteBehavior.Cascade);

        // SLAEscalations
        builder.Entity<SLAEscalation>()
            .HasOne(se => se.WorkOrder)
            .WithMany(wo => wo.SLAEscalations)
            .HasForeignKey(se => se.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<SLAEscalation>()
            .HasOne(se => se.AcknowledgedByUser)
            .WithMany()
            .HasForeignKey(se => se.AcknowledgedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ComplaintFeedback
        builder.Entity<ComplaintFeedback>()
            .HasOne(cf => cf.Complaint)
            .WithMany(c => c.Feedbacks)
            .HasForeignKey(cf => cf.ComplaintId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ComplaintFeedback>()
            .HasOne(cf => cf.ReporterUser)
            .WithMany()
            .HasForeignKey(cf => cf.ReporterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Notifications
        builder.Entity<Notification>()
            .HasOne(n => n.RecipientUser)
            .WithMany()
            .HasForeignKey(n => n.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserProfiles
        builder.Entity<UserProfile>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserProfile>()
            .HasOne(p => p.Department)
            .WithMany()
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
