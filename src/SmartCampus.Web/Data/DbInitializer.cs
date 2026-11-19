using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCampus.Web.Models.Entities;
using SmartCampus.Web.Models.Enums;

namespace SmartCampus.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database exists
        await context.Database.EnsureCreatedAsync();

        // Ensure column sizes are expanded on existing tables
        try
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE [Complaints] ALTER COLUMN [ReporterContactPhone] nvarchar(50) NULL");
        }
        catch
        {
            // Column may already be nvarchar(50) or table newly created
        }

        // 1. Seed Roles
        string[] roles = ["SuperAdmin", "CampusAdmin", "DepartmentSupervisor", "MaintenanceWorker", "StudentTeacher"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Allowed Email Domains
        if (!await context.AllowedEmailDomains.AnyAsync())
        {
            context.AllowedEmailDomains.AddRange(
                new AllowedEmailDomain { DomainName = "lpu.co.in", Description = "Official LPU Faculty & Administration Domain", DefaultRole = "StudentTeacher", IsActive = true },
                new AllowedEmailDomain { DomainName = "lpu.in", Description = "Official LPU Student Domain", DefaultRole = "StudentTeacher", IsActive = true },
                new AllowedEmailDomain { DomainName = "gmail.com", Description = "Permitted for Prototype and External Testing", DefaultRole = "StudentTeacher", IsActive = true },
                new AllowedEmailDomain { DomainName = "smartcampus.lpu.in", Description = "Internal SmartCampus System Staff Domain", DefaultRole = "CampusAdmin", IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // 3. Seed Priorities & SLA Policies
        if (!await context.Priorities.AnyAsync())
        {
            var p1 = new Priority { PriorityId = (int)PriorityLevel.Critical, PriorityName = "Critical", SeverityLevel = 1, BadgeColor = "danger" };
            var p2 = new Priority { PriorityId = (int)PriorityLevel.High, PriorityName = "High", SeverityLevel = 2, BadgeColor = "warning" };
            var p3 = new Priority { PriorityId = (int)PriorityLevel.Medium, PriorityName = "Medium", SeverityLevel = 3, BadgeColor = "info" };
            var p4 = new Priority { PriorityId = (int)PriorityLevel.Low, PriorityName = "Low", SeverityLevel = 4, BadgeColor = "secondary" };

            context.Priorities.AddRange(p1, p2, p3, p4);
            await context.SaveChangesAsync();

            context.SLAPolicies.AddRange(
                new SLAPolicy { PriorityId = p1.PriorityId, ResponseTargetMinutes = 15, WorkStartTargetMinutes = 30, ResolutionTargetMinutes = 60, IsActive = true },
                new SLAPolicy { PriorityId = p2.PriorityId, ResponseTargetMinutes = 60, WorkStartTargetMinutes = 120, ResolutionTargetMinutes = 360, IsActive = true },
                new SLAPolicy { PriorityId = p3.PriorityId, ResponseTargetMinutes = 240, WorkStartTargetMinutes = 480, ResolutionTargetMinutes = 1440, IsActive = true },
                new SLAPolicy { PriorityId = p4.PriorityId, ResponseTargetMinutes = 720, WorkStartTargetMinutes = 1440, ResolutionTargetMinutes = 4320, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // 4. Seed Departments
        if (!await context.Departments.AnyAsync())
        {
            context.Departments.AddRange(
                new Department { DepartmentCode = "ELEC", DepartmentName = "Electrical & Power Maintenance", ContactEmail = "electrical@lpu.co.in", ContactPhone = "+91-1824-517101", IsActive = true },
                new Department { DepartmentCode = "PLMB", DepartmentName = "Plumbing & Water Supply", ContactEmail = "plumbing@lpu.co.in", ContactPhone = "+91-1824-517102", IsActive = true },
                new Department { DepartmentCode = "HVAC", DepartmentName = "HVAC & Climate Control", ContactEmail = "hvac@lpu.co.in", ContactPhone = "+91-1824-517103", IsActive = true },
                new Department { DepartmentCode = "CARP", DepartmentName = "Carpentry & Furniture Works", ContactEmail = "carpentry@lpu.co.in", ContactPhone = "+91-1824-517104", IsActive = true },
                new Department { DepartmentCode = "CIVIL", DepartmentName = "Civil Works & Structural Maintenance", ContactEmail = "civil@lpu.co.in", ContactPhone = "+91-1824-517105", IsActive = true },
                new Department { DepartmentCode = "NETIT", DepartmentName = "Campus Network & IT Systems", ContactEmail = "netadmin@lpu.co.in", ContactPhone = "+91-1824-517106", IsActive = true },
                new Department { DepartmentCode = "SANI", DepartmentName = "Housekeeping & Campus Sanitation", ContactEmail = "sanitation@lpu.co.in", ContactPhone = "+91-1824-517107", IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // 5. Seed Issue Categories & Subcategories
        if (!await context.IssueCategories.AnyAsync())
        {
            var elecDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "ELEC");
            var plmbDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "PLMB");
            var hvacDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "HVAC");
            var carpDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "CARP");
            var civilDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "CIVIL");
            var itDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "NETIT");

            var catElec = new IssueCategory { CategoryName = "Electrical & Lighting", Description = "Wiring, lights, switchboards, and fan repairs", IconCssClass = "bi-lightning-charge", DefaultDepartmentId = elecDept.DepartmentId, IsActive = true };
            var catPlmb = new IssueCategory { CategoryName = "Plumbing & Sanitation", Description = "Water leaks, washroom taps, drainage blockages", IconCssClass = "bi-droplet-fill", DefaultDepartmentId = plmbDept.DepartmentId, IsActive = true };
            var catHvac = new IssueCategory { CategoryName = "HVAC & Cooling", Description = "AC units, ventilation, and chiller issues", IconCssClass = "bi-snow", DefaultDepartmentId = hvacDept.DepartmentId, IsActive = true };
            var catCarp = new IssueCategory { CategoryName = "Carpentry & Furniture", Description = "Desks, chairs, podiums, doors and locks", IconCssClass = "bi-hammer", DefaultDepartmentId = carpDept.DepartmentId, IsActive = true };
            var catCivil = new IssueCategory { CategoryName = "Civil & Infrastructure", Description = "Walls, tiles, plaster, ceiling leaks, road potholes", IconCssClass = "bi-building-fill-gear", DefaultDepartmentId = civilDept.DepartmentId, IsActive = true };
            var catIt = new IssueCategory { CategoryName = "Classroom IT & Audio-Visual", Description = "Projectors, smart podiums, lab networking", IconCssClass = "bi-pc-display", DefaultDepartmentId = itDept.DepartmentId, IsActive = true };

            context.IssueCategories.AddRange(catElec, catPlmb, catHvac, catCarp, catCivil, catIt);
            await context.SaveChangesAsync();

            context.IssueSubcategories.AddRange(
                new IssueSubcategory { CategoryId = catElec.CategoryId, SubcategoryName = "Exposed High-Voltage Wiring", DefaultPriorityId = (int)PriorityLevel.Critical },
                new IssueSubcategory { CategoryId = catElec.CategoryId, SubcategoryName = "Broken Switchboard / Power Socket", DefaultPriorityId = (int)PriorityLevel.High },
                new IssueSubcategory { CategoryId = catElec.CategoryId, SubcategoryName = "Ceiling Fan Faulty / Non-functional", DefaultPriorityId = (int)PriorityLevel.Medium },
                new IssueSubcategory { CategoryId = catElec.CategoryId, SubcategoryName = "Corridor Lights Not Working", DefaultPriorityId = (int)PriorityLevel.Medium },

                new IssueSubcategory { CategoryId = catPlmb.CategoryId, SubcategoryName = "Major Water Pipe Burst", DefaultPriorityId = (int)PriorityLevel.Critical },
                new IssueSubcategory { CategoryId = catPlmb.CategoryId, SubcategoryName = "Washroom Basin Leakage", DefaultPriorityId = (int)PriorityLevel.Medium },
                new IssueSubcategory { CategoryId = catPlmb.CategoryId, SubcategoryName = "Drainage Blocked / Overflow", DefaultPriorityId = (int)PriorityLevel.High },

                new IssueSubcategory { CategoryId = catHvac.CategoryId, SubcategoryName = "Server Room AC Failure", DefaultPriorityId = (int)PriorityLevel.Critical },
                new IssueSubcategory { CategoryId = catHvac.CategoryId, SubcategoryName = "Lecture Theatre Split AC Not Cooling", DefaultPriorityId = (int)PriorityLevel.Medium },

                new IssueSubcategory { CategoryId = catCarp.CategoryId, SubcategoryName = "Damaged Classroom Benches / Desks", DefaultPriorityId = (int)PriorityLevel.Low },
                new IssueSubcategory { CategoryId = catCarp.CategoryId, SubcategoryName = "Classroom Door Lock Jammed", DefaultPriorityId = (int)PriorityLevel.High },

                new IssueSubcategory { CategoryId = catCivil.CategoryId, SubcategoryName = "Ceiling Plaster Falling / Safety Risk", DefaultPriorityId = (int)PriorityLevel.High },
                new IssueSubcategory { CategoryId = catCivil.CategoryId, SubcategoryName = "Broken Floor Tiles", DefaultPriorityId = (int)PriorityLevel.Low },

                new IssueSubcategory { CategoryId = catIt.CategoryId, SubcategoryName = "Classroom Projector Down", DefaultPriorityId = (int)PriorityLevel.High },
                new IssueSubcategory { CategoryId = catIt.CategoryId, SubcategoryName = "Lab Ethernet Outage", DefaultPriorityId = (int)PriorityLevel.High }
            );
            await context.SaveChangesAsync();
        }

        // 6. Seed LPU Campus Spatial Hierarchy (Buildings, Floors, Locations)
        if (!await context.Buildings.AnyAsync())
        {
            var b32 = new Building { BuildingCode = "BLK-32", BuildingName = "Block 32 (School of Computer Science & Engineering)", ZoneType = BuildingZoneType.Academic, TotalFloors = 8, Latitude = 31.254100m, Longitude = 75.702800m, Description = "High-density academic building with computer labs, faculty rooms, and lecture theatres." };
            var b34 = new Building { BuildingCode = "BLK-34", BuildingName = "Block 34 (School of Mechanical & Civil Engineering)", ZoneType = BuildingZoneType.Academic, TotalFloors = 6, Latitude = 31.253200m, Longitude = 75.701900m, Description = "Engineering workshops, CAD labs, and heavy mechanical machinery halls." };
            var b38 = new Building { BuildingCode = "BLK-38", BuildingName = "Block 38 (School of Electronics & Bio-Engineering)", ZoneType = BuildingZoneType.Academic, TotalFloors = 7, Latitude = 31.252500m, Longitude = 75.704200m, Description = "Electronics labs, IoT research facilities, and clean rooms." };
            var lib = new Building { BuildingCode = "LIB", BuildingName = "Dr. Radhakrishnan Central Library", ZoneType = BuildingZoneType.Academic, TotalFloors = 5, Latitude = 31.255000m, Longitude = 75.704500m, Description = "Central 5-story university library, digital knowledge repository, and reading halls." };
            var mall = new Building { BuildingCode = "MALL", BuildingName = "UniMall & Student Activity Center", ZoneType = BuildingZoneType.Commercial, TotalFloors = 4, Latitude = 31.254500m, Longitude = 75.706000m, Description = "Retail outlets, food courts, post office, and student recreation spaces." };
            var bh1 = new Building { BuildingCode = "BH-01", BuildingName = "Boys Hostel 1 (Vivekananda Hostel)", ZoneType = BuildingZoneType.ResidentialHostel, TotalFloors = 8, Latitude = 31.256000m, Longitude = 75.701000m, Description = "High-rise male undergraduate student residential block." };
            var gh1 = new Building { BuildingCode = "GH-01", BuildingName = "Girls Hostel 1 (Sarojini Hostel)", ZoneType = BuildingZoneType.ResidentialHostel, TotalFloors = 8, Latitude = 31.251000m, Longitude = 75.708000m, Description = "High-rise female undergraduate student residential block." };
            var outZone = new Building { BuildingCode = "OUT-01", BuildingName = "Outdoor Campus Grounds & Boulevard", ZoneType = BuildingZoneType.UtilityInfrastructure, TotalFloors = 1, Latitude = 31.253600m, Longitude = 75.703700m, Description = "Perimeter roads, streetlights, main gates, and parking areas." };

            context.Buildings.AddRange(b32, b34, b38, lib, mall, bh1, gh1, outZone);
            await context.SaveChangesAsync();

            // Floors for Block 32
            var f0_32 = new Floor { BuildingId = b32.BuildingId, FloorNumber = 0, FloorName = "Ground Floor" };
            var f1_32 = new Floor { BuildingId = b32.BuildingId, FloorNumber = 1, FloorName = "1st Floor" };
            var f4_32 = new Floor { BuildingId = b32.BuildingId, FloorNumber = 4, FloorName = "4th Floor (Computing Labs)" };
            var f7_32 = new Floor { BuildingId = b32.BuildingId, FloorNumber = 7, FloorName = "7th Floor (Dean & Faculty Office)" };

            // Floors for Library & Hostel
            var f1_lib = new Floor { BuildingId = lib.BuildingId, FloorNumber = 1, FloorName = "1st Floor Reading Hall" };
            var f3_bh1 = new Floor { BuildingId = bh1.BuildingId, FloorNumber = 3, FloorName = "3rd Floor Dormitory Wing" };
            var f0_out = new Floor { BuildingId = outZone.BuildingId, FloorNumber = 0, FloorName = "Ground Level" };

            context.Floors.AddRange(f0_32, f1_32, f4_32, f7_32, f1_lib, f3_bh1, f0_out);
            await context.SaveChangesAsync();

            // Locations with unique GUIDs for QR reporting
            context.Locations.AddRange(
                new Location { FloorId = f4_32.FloorId, RoomNumber = "32-401", LocationName = "Advanced Computing & AI Lab 1", LocationType = LocationType.ComputerLab, Latitude = 31.254120m, Longitude = 75.702810m },
                new Location { FloorId = f4_32.FloorId, RoomNumber = "32-402", LocationName = "Software Engineering Lab 2", LocationType = LocationType.ComputerLab, Latitude = 31.254130m, Longitude = 75.702820m },
                new Location { FloorId = f1_32.FloorId, RoomNumber = "32-105", LocationName = "Smart Lecture Hall 105", LocationType = LocationType.LectureHall, Latitude = 31.254050m, Longitude = 75.702750m },
                new Location { FloorId = f0_32.FloorId, RoomNumber = "32-G-Wash", LocationName = "Ground Floor Male Restroom", LocationType = LocationType.Washroom, Latitude = 31.254010m, Longitude = 75.702700m },
                new Location { FloorId = f1_lib.FloorId, RoomNumber = "LIB-102", LocationName = "Digital Reference Hall", LocationType = LocationType.LectureHall, Latitude = 31.255050m, Longitude = 75.704550m },
                new Location { FloorId = f3_bh1.FloorId, RoomNumber = "BH1-312", LocationName = "Room 312 (Triple Occupancy)", LocationType = LocationType.HostelRoom, Latitude = 31.256050m, Longitude = 75.701050m },
                new Location { FloorId = f0_out.FloorId, RoomNumber = "SL-32-04", LocationName = "Streetlight Pole #32-04 (Near CS Lawn)", LocationType = LocationType.OutdoorPathway, Latitude = 31.253900m, Longitude = 75.702950m }
            );
            await context.SaveChangesAsync();
        }

        // 7. Seed Test Users & Profiles for All 5 Roles
        await SeedUserAsync(userManager, context, "superadmin@smartcampus.lpu.in", "Admin@12345", "SuperAdmin", "Prof. R. K. Sharma", "EMP-0001", "+91-9876500001");
        await SeedUserAsync(userManager, context, "controldesk@smartcampus.lpu.in", "Admin@12345", "CampusAdmin", "Officer Harpreet Singh", "EMP-0042", "+91-9876500042");
        
        var elecSupervisor = await SeedUserAsync(userManager, context, "elec.head@smartcampus.lpu.in", "Super@12345", "DepartmentSupervisor", "Er. Manjit Kumar (Electrical Head)", "EMP-0115", "+91-9876500115");
        var plmbSupervisor = await SeedUserAsync(userManager, context, "plmb.head@smartcampus.lpu.in", "Super@12345", "DepartmentSupervisor", "Er. Gurdeep Singh (Plumbing Head)", "EMP-0118", "+91-9876500118");

        var workerRajesh = await SeedUserAsync(userManager, context, "technician.rajesh@smartcampus.lpu.in", "Worker@12345", "MaintenanceWorker", "Rajesh Verma (Senior Electrician)", "TECH-0301", "+91-9876500301");
        var workerVikram = await SeedUserAsync(userManager, context, "technician.vikram@smartcampus.lpu.in", "Worker@12345", "MaintenanceWorker", "Vikram Patel (Sanitary Plumber)", "TECH-0304", "+91-9876500304");

        await SeedUserAsync(userManager, context, "student.test@lpu.in", "Student@12345", "StudentTeacher", "Aarav Sharma (B.Tech CSE)", "12201948", "+91-9876500999");
        await SeedUserAsync(userManager, context, "faculty.test@lpu.co.in", "Faculty@12345", "StudentTeacher", "Dr. Priya Nair (Associate Professor)", "FAC-0824", "+91-9876500824");

        // 8. Seed Teams and Team Members
        if (!await context.Teams.AnyAsync() && elecSupervisor != null && workerRajesh != null)
        {
            var elecDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "ELEC");
            var plmbDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "PLMB");

            var elecTeam = new Team
            {
                TeamName = "Block 30-38 Electrical Quick Response Team",
                DepartmentId = elecDept.DepartmentId,
                SupervisorUserId = elecSupervisor.Id,
                IsActive = true
            };

            var plmbTeam = new Team
            {
                TeamName = "Campus Central Plumbing Squad",
                DepartmentId = plmbDept.DepartmentId,
                SupervisorUserId = plmbSupervisor!.Id,
                IsActive = true
            };

            context.Teams.AddRange(elecTeam, plmbTeam);
            await context.SaveChangesAsync();

            context.TeamMembers.AddRange(
                new TeamMember { TeamId = elecTeam.TeamId, WorkerUserId = workerRajesh.Id, AvailabilityStatus = WorkerAvailability.Available, IsActive = true },
                new TeamMember { TeamId = plmbTeam.TeamId, WorkerUserId = workerVikram!.Id, AvailabilityStatus = WorkerAvailability.Available, IsActive = true }
            );
            await context.SaveChangesAsync();
        }

        // 9. Seed Representative Pilot Complaints Across Lifecycle
        if (!await context.Complaints.AnyAsync())
        {
            var studentUser = await userManager.FindByEmailAsync("student.test@lpu.in");
            var facultyUser = await userManager.FindByEmailAsync("faculty.test@lpu.co.in");

            var loc302 = await context.Locations.FirstOrDefaultAsync(l => l.RoomNumber == "302");
            var loc601 = await context.Locations.FirstOrDefaultAsync(l => l.RoomNumber == "601");
            var locMall = await context.Locations.FirstOrDefaultAsync(l => l.LocationName.Contains("Atrium"));
            var loc204 = await context.Locations.FirstOrDefaultAsync(l => l.RoomNumber == "204");

            var prioCritical = await context.Priorities.Include(p => p.SLAPolicy).FirstOrDefaultAsync(p => p.PriorityName == "Critical");
            var prioHigh = await context.Priorities.Include(p => p.SLAPolicy).FirstOrDefaultAsync(p => p.PriorityName == "High");
            var prioMedium = await context.Priorities.Include(p => p.SLAPolicy).FirstOrDefaultAsync(p => p.PriorityName == "Medium");
            var prioLow = await context.Priorities.Include(p => p.SLAPolicy).FirstOrDefaultAsync(p => p.PriorityName == "Low");

            var catElec = await context.IssueCategories.FirstOrDefaultAsync(c => c.CategoryName.Contains("Electrical")) ?? await context.IssueCategories.FirstOrDefaultAsync();
            var catPlmb = await context.IssueCategories.FirstOrDefaultAsync(c => c.CategoryName.Contains("Plumbing")) ?? await context.IssueCategories.FirstOrDefaultAsync();
            var catCivil = await context.IssueCategories.FirstOrDefaultAsync(c => c.CategoryName.Contains("Civil")) ?? await context.IssueCategories.FirstOrDefaultAsync();

            var elecDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "ELEC");
            var plmbDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "PLMB");
            var civilDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "CIVIL");

            if (studentUser != null && loc302 != null && prioCritical != null)
            {
                // Ticket 1: Submitted (Triage Queue Demo)
                var ticket1 = new Complaint
                {
                    TicketNumber = "SC-2026-00001",
                    ReporterUserId = studentUser.Id,
                    LocationId = loc302.LocationId,
                    CategoryId = catElec!.CategoryId,
                    PriorityId = prioCritical.PriorityId,
                    Title = "Exposed wiring and sparking switchboard in Classroom 302",
                    Description = "Live wires exposed behind the multimedia podium switchboard. Sparking observed when turning on the projector power switch.",
                    Status = ComplaintStatus.Submitted,
                    CreatedAtUtc = DateTime.UtcNow.AddMinutes(-25),
                    ReporterContactPhone = "+91-9876500999"
                };
                context.Complaints.Add(ticket1);
                await context.SaveChangesAsync();

                context.ComplaintImages.Add(new ComplaintImage
                {
                    ComplaintId = ticket1.ComplaintId,
                    ImageUrl = "/images/sample-evidence.svg"
                });

                context.ComplaintHistories.Add(new ComplaintHistory
                {
                    ComplaintId = ticket1.ComplaintId,
                    ActorUserId = studentUser.Id,
                    PreviousStatus = ComplaintStatus.Submitted,
                    NewStatus = ComplaintStatus.Submitted,
                    Action = "TicketSubmitted",
                    Remarks = "Ticket logged via student portal.",
                    TimestampUtc = DateTime.UtcNow.AddMinutes(-25)
                });
            }

            if (facultyUser != null && loc601 != null && prioHigh != null && plmbDept != null && workerVikram != null)
            {
                // Ticket 2: Assigned (Technician Task Execution Demo)
                var ticket2 = new Complaint
                {
                    TicketNumber = "SC-2026-00002",
                    ReporterUserId = facultyUser.Id,
                    LocationId = loc601.LocationId,
                    CategoryId = catPlmb!.CategoryId,
                    PriorityId = prioHigh.PriorityId,
                    Title = "Major pipeline rupture and continuous flooding in washroom",
                    Description = "Main inlet pipe under the sink has ruptured causing continuous water leakage onto the corridor floor.",
                    Status = ComplaintStatus.Assigned,
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-2),
                    ResponseDeadlineUtc = DateTime.UtcNow.AddMinutes(30),
                    ResolutionDeadlineUtc = DateTime.UtcNow.AddHours(4),
                    ReporterContactPhone = "+91-9876500824"
                };
                context.Complaints.Add(ticket2);
                await context.SaveChangesAsync();

                var wo2 = new WorkOrder
                {
                    WorkOrderNumber = "WO-2026-00001",
                    ComplaintId = ticket2.ComplaintId,
                    DepartmentId = plmbDept.DepartmentId,
                    Status = WorkOrderStatus.Assigned,
                    AssignedAtUtc = DateTime.UtcNow.AddHours(-1)
                };
                context.WorkOrders.Add(wo2);
                await context.SaveChangesAsync();

                context.WorkOrderAssignments.Add(new WorkOrderAssignment
                {
                    WorkOrderId = wo2.WorkOrderId,
                    WorkerUserId = workerVikram.Id,
                    AssignedAtUtc = DateTime.UtcNow.AddHours(-1)
                });
            }

            if (studentUser != null && locMall != null && prioMedium != null && elecDept != null && workerRajesh != null)
            {
                // Ticket 3: AwaitingVerification (Independent Supervisory Verification Gate Demo)
                var ticket3 = new Complaint
                {
                    TicketNumber = "SC-2026-00003",
                    ReporterUserId = studentUser.Id,
                    LocationId = locMall.LocationId,
                    CategoryId = catElec!.CategoryId,
                    PriorityId = prioMedium.PriorityId,
                    Title = "Streetlight flickering and dark pathway near UniMall plaza",
                    Description = "High-mast luminaire #UM-04 is completely dark causing safety concern during evening student hours.",
                    Status = ComplaintStatus.AwaitingVerification,
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-5),
                    ResponseDeadlineUtc = DateTime.UtcNow.AddHours(-4),
                    ResolutionDeadlineUtc = DateTime.UtcNow.AddHours(19)
                };
                context.Complaints.Add(ticket3);
                await context.SaveChangesAsync();

                context.ComplaintImages.Add(new ComplaintImage
                {
                    ComplaintId = ticket3.ComplaintId,
                    ImageUrl = "/images/sample-evidence.svg"
                });

                var wo3 = new WorkOrder
                {
                    WorkOrderNumber = "WO-2026-00002",
                    ComplaintId = ticket3.ComplaintId,
                    DepartmentId = elecDept.DepartmentId,
                    Status = WorkOrderStatus.AwaitingVerification,
                    WorkSummary = "Replaced faulty MeanWell LED driver and secured luminaire terminal box. Tested at 230V full illumination.",
                    AssignedAtUtc = DateTime.UtcNow.AddHours(-4),
                    WorkStartedAtUtc = DateTime.UtcNow.AddHours(-3),
                    WorkCompletedAtUtc = DateTime.UtcNow.AddMinutes(-30)
                };
                context.WorkOrders.Add(wo3);
                await context.SaveChangesAsync();

                context.WorkOrderAssignments.Add(new WorkOrderAssignment
                {
                    WorkOrderId = wo3.WorkOrderId,
                    WorkerUserId = workerRajesh.Id,
                    AssignedAtUtc = DateTime.UtcNow.AddHours(-4),
                    AcceptedAtUtc = DateTime.UtcNow.AddHours(-3),
                    CompletedAtUtc = DateTime.UtcNow.AddMinutes(-30)
                });

                context.WorkOrderEvidences.Add(new WorkOrderEvidence
                {
                    WorkOrderId = wo3.WorkOrderId,
                    WorkerUserId = workerRajesh.Id,
                    ImageUrl = "/images/sample-repaired.svg",
                    Notes = "Work completed with new LED driver tested and verified."
                });
            }

            if (studentUser != null && loc204 != null && prioLow != null && civilDept != null && elecSupervisor != null)
            {
                // Ticket 4: VerifiedResolved & Closed (Rating & Closed Ticket Demo)
                var ticket4 = new Complaint
                {
                    TicketNumber = "SC-2026-00004",
                    ReporterUserId = studentUser.Id,
                    LocationId = loc204.LocationId,
                    CategoryId = catCivil!.CategoryId,
                    PriorityId = prioLow.PriorityId,
                    Title = "Broken desk frame and squeaking door hinge in Faculty Cabin 204",
                    Description = "Desk support leg is bent and main entry door hinge requires lubrication and realignment.",
                    Status = ComplaintStatus.Closed,
                    CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
                    ResolvedAtUtc = DateTime.UtcNow.AddDays(-1),
                    ResolutionDeadlineUtc = DateTime.UtcNow.AddDays(1)
                };
                context.Complaints.Add(ticket4);
                await context.SaveChangesAsync();

                var wo4 = new WorkOrder
                {
                    WorkOrderNumber = "WO-2026-00003",
                    ComplaintId = ticket4.ComplaintId,
                    DepartmentId = civilDept.DepartmentId,
                    Status = WorkOrderStatus.VerifiedResolved,
                    WorkSummary = "Re-welded metal brace and installed brass heavy-duty hinge.",
                    AssignedAtUtc = DateTime.UtcNow.AddDays(-2),
                    WorkCompletedAtUtc = DateTime.UtcNow.AddDays(-1)
                };
                context.WorkOrders.Add(wo4);
                await context.SaveChangesAsync();

                context.Verifications.Add(new Verification
                {
                    WorkOrderId = wo4.WorkOrderId,
                    VerifierUserId = elecSupervisor.Id,
                    IsApproved = true,
                    InspectionNotes = "Physical on-site inspection passed. Door swings smoothly and desk welded frame tested under 50kg load.",
                    VerifiedAtUtc = DateTime.UtcNow.AddDays(-1)
                });

                context.ComplaintFeedbacks.Add(new ComplaintFeedback
                {
                    ComplaintId = ticket4.ComplaintId,
                    ReporterUserId = studentUser.Id,
                    RatingStars = 5,
                    Comments = "Excellent response by maintenance staff. Repaired before my class began!",
                    SubmittedAtUtc = DateTime.UtcNow.AddHours(-18)
                });
            }

            await context.SaveChangesAsync();
        }
    }

    private static async Task<IdentityUser?> SeedUserAsync(
        UserManager<IdentityUser> userManager,
        ApplicationDbContext context,
        string email,
        string password,
        string role,
        string fullName,
        string regNumber,
        string phone)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return existingUser;
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            PhoneNumber = phone
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);

            context.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FullName = fullName,
                RegistrationNumber = regNumber,
                PhoneNumber = phone,
                IsApprovedByAdmin = true
            });
            await context.SaveChangesAsync();
            return user;
        }

        return null;
    }
}
