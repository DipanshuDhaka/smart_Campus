# SmartCampus — Granular Step-by-Step Build Log & Engineering Methodology

**Project:** SmartCampus (Intelligent Campus Issue Reporting, Workforce Allocation & Resolution Verification System)  
**Reference Campus:** Lovely Professional University (LPU), Punjab, India  
**Document Purpose:** Complete chronological, auditable engineering log detailing every tool, command, architectural design pattern, implementation phase, and verification proof.  
**Version:** 1.0.0  

---

## Table of Contents
1. [Engineering Methodologies & Architectural Principles](#1-engineering-methodologies--architectural-principles)
2. [Tooling & Technical Stack Specifications](#2-tooling--technical-stack-specifications)
3. [Phase 1: Planning, Requirements & Specification (Completed)](#3-phase-1-planning-requirements--specification-completed)
4. [Phase 2: Environment Setup, Solution Scaffolding & Packages](#4-phase-2-environment-setup-solution-scaffolding--packages)
5. [Phase 3: Database Domain Entities, DbContext & LPU Campus Seeder](#5-phase-3-database-domain-entities-dbcontext--lpu-campus-seeder)
6. [Phase 4: Google OAuth 2.0, Institutional Whitelist & Identity RBAC](#6-phase-4-google-oauth-20-institutional-whitelist--identity-rbac)
7. [Phase 5: Dynamic Campus Location Hierarchy & QR Code Generation](#7-phase-5-dynamic-campus-location-hierarchy--qr-code-generation)
8. [Phase 6: Student & Teacher Complaint Reporting Module](#8-phase-6-student--teacher-complaint-reporting-module)
9. [Phase 7: Campus Control Desk Triage & Duplicate Detection Heuristic](#9-phase-7-campus-control-desk-triage--duplicate-detection-heuristic)
10. [Phase 8: Department & Workforce Team Management](#10-phase-8-department--workforce-team-management)
11. [Phase 9: Work Orders & Workforce Allocation Engine](#11-phase-9-work-orders--workforce-allocation-engine)
12. [Phase 10: Independent Verification Gate (Core Differentiator)](#12-phase-10-independent-verification-gate-core-differentiator)
13. [Phase 11: SLA Monitoring & Multi-Tier Escalation Engine](#13-phase-11-sla-monitoring--multi-tier-escalation-engine)
14. [Phase 12: Interactive Campus Map & Dynamic QR Integration](#14-phase-12-interactive-campus-map--dynamic-qr-integration)
15. [Phase 13: Operations Analytics & Executive Dashboards](#15-phase-13-operations-analytics--executive-dashboards)
16. [Phase 14: Advanced Optimization & Real-Time Push Channels](#16-phase-14-advanced-optimization--real-time-push-channels)
17. [Phase 15: Rigorous Verification, Hardening & Deployment](#17-phase-15-rigorous-verification-hardening--deployment)

---

## 1. Engineering Methodologies & Architectural Principles

### 1.1 Get Shit Done (GSD) Spec-Driven Development
The development adheres strictly to the 5-phase milestone loop:
1. **Discuss:** Clarify requirements, institutional constraints, and non-goals before touching code. Externalize requirements to `PRD.md` and `docs/SRS.md`.
2. **Plan:** Decompose all work into ordered, atomic steps with explicit inputs and verifiable outputs.
3. **Execute:** Implement each module cleanly, maintaining documentation and avoiding extraneous dependencies.
4. **Verify:** Test and validate builds, schema constraints, CLI commands, and UI rendering after every module.
5. **Ship:** Review diffs, update `STATE.md`, `progress.txt`, and stage commits.

### 1.2 Ralph Loop Externalized Memory Pattern
To guarantee consistency across multi-step execution:
- Single source of truth for task objectives: `PRD.md` with binary checkboxes (`- [ ]` / `- [x]`).
- Single source of truth for execution logs: `progress.txt`.
- No reliance on ephemeral conversational context.

### 1.3 LPU UMS Design Aesthetic Principle
To ensure the interface reflects an authentic, formal university enterprise portal (rather than generic or AI-styled templates):
- **Color Palette:** Deep Navy Blue (`#0f2b48` / `#1B365D`), Crisp White (`#ffffff`), Slate Gray borders and tables (`#e2e8f0`, `#f8fafc`), Teal accent (`#0d9488`).
- **Typography:** Clean sans-serif system fonts (`Segoe UI`, system font stack), crisp letter-spacing, clear font weights.
- **Components:** Formal university top navigation banner with institution branding, user profile badge with Registration/Employee ID, breadcrumb trails, structured data tables with sharp status badges, card layouts with clean borders (no fuzzy AI neon drop shadows).

---

## 2. Tooling & Technical Stack Specifications

### 2.1 Backend & Database
- **Framework:** ASP.NET Core MVC on .NET 10 LTS (`10.0.302 SDK`, Runtime `10.0.10`).
- **Language:** C# 13 / 14 with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.
- **Database Engine:** Microsoft SQL Server 2022 / LocalDB (`SmartCampusDB`).
- **ORM:** Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`).
- **Identity Framework:** `Microsoft.AspNetCore.Identity.EntityFrameworkCore`.

### 2.2 External Services & Client Libraries
- **Google OAuth 2.0:** `Microsoft.AspNetCore.Authentication.Google`.
- **QR Code Engine:** `QRCoder` (.NET library for SVG/PNG generation).
- **Frontend Assets:** Bootstrap 5.3, Bootstrap Icons, Leaflet.js 1.9.4 (OpenStreetMap), Chart.js 4.4.x.

---

## 3. Phase 1: Planning, Requirements & Specification (Completed)

### 3.1 Step 1.1 — Requirements Analysis & Scope Definition
- Analyzed the operational requirements of Lovely Professional University (600+ acres, 60+ academic blocks, 14+ hostels, 30,000+ residents).
- Formalized 5 primary user roles:
  1. Student / Teacher (Reporter)
  2. Campus Administrator (Control Desk)
  3. Department Supervisor (Team Head)
  4. Maintenance Worker (Technician)
  5. Super Administrator
- Established the central institutional axiom: **`Work Completed != Resolved`**. Independent verification by a trade supervisor is a mandatory physical checkpoint.

### 3.2 Step 1.2 — Core Deliverables Authored
- Authored [`PRD.md`](file:///c:/Users/hp/Desktop/SmartCampus/PRD.md) capturing complete business logic, non-goals, and sequential 15-phase task checklist.
- Authored [`docs/SRS.md`](file:///c:/Users/hp/Desktop/SmartCampus/docs/SRS.md) following IEEE 830 standards with 12 functional areas (`FR-AUTH` through `FR-FEEDBACK`) and strict NFR security constraints.
- Authored [`docs/USE_CASE_SPECIFICATION.md`](file:///c:/Users/hp/Desktop/SmartCampus/docs/USE_CASE_SPECIFICATION.md) with Mermaid actor hierarchies, sequence diagrams, state machines, and 10 detailed use cases.
- Authored [`docs/DATABASE_ERD.md`](file:///c:/Users/hp/Desktop/SmartCampus/docs/DATABASE_ERD.md) specifying the 11 relational modules of `SmartCampusDB` with constraints, foreign keys, and sub-50ms performance indexing.
- Initialized [`STATE.md`](file:///c:/Users/hp/Desktop/SmartCampus/STATE.md), [`CONTEXT.md`](file:///c:/Users/hp/Desktop/SmartCampus/CONTEXT.md), and [`progress.txt`](file:///c:/Users/hp/Desktop/SmartCampus/progress.txt).

---

## 4. Phase 2: Environment Setup, Solution Scaffolding & Packages

### 4.1 Step 2.1 — Solution & Project Creation
Commands executed to scaffold the enterprise multi-layer MVC application:
```powershell
dotnet new sln -n SmartCampus
dotnet new mvc -n SmartCampus.Web -o src/SmartCampus.Web
dotnet sln add src/SmartCampus.Web/SmartCampus.Web.csproj
```

### 4.2 Step 2.2 — Package Installation
Installed official enterprise NuGet packages into `SmartCampus.Web`:
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.x)
- `Microsoft.EntityFrameworkCore.Tools` (10.0.x)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.x)
- `Microsoft.AspNetCore.Authentication.Google` (10.0.x)
- `QRCoder` (1.6.x)

### 4.3 Step 2.3 — Area Folder Topology Setup
Established role-governed MVC Areas conforming to LPU UMS modularity:
- `Areas/Admin/` (Control Desk & Super Admin)
- `Areas/Supervisor/` (Department Team Heads & Verification)
- `Areas/Worker/` (Maintenance Technicians)
- `Areas/Reporter/` (Student & Faculty Portal)

---

## 5. Phase 3: Database Domain Entities, DbContext & LPU Campus Seeder

### 5.1 Step 3.1 — Domain Modeling Across 11 Modules
Created relational entities in `src/SmartCampus.Web/Models/Entities/`:
- `IdentityExtensions.cs`: `UserProfile` linked 1-to-1 with `IdentityUser`, storing institutional registration numbers and department affiliations.
- `CampusSpatial.cs`: `Building`, `Floor`, and `Location` (Indoor/Outdoor) with GPS coordinates and QR code GUID keys.
- `IssueCatalog.cs`: `IssueCategory` and `IssueSubcategory` with trade department mappings.
- `WorkforceEntities.cs`: `Department`, `Team`, `TeamMember`, `Skill`, and `WorkerSkill` with active workload states.
- `ComplaintEntities.cs`: `Complaint`, `ComplaintImage`, and `ComplaintHistory` representing the complete lifecycle and immutable audit trail.
- `WorkOrderEntities.cs`: `WorkOrder`, `WorkOrderAssignment`, `WorkOrderEvidence`, `Verification`, and `VerificationImage`.
- `SystemSupportEntities.cs`: `SLAEscalation`, `ComplaintFeedback`, `Notification`, and `AuditLog`.

### 5.2 Step 3.2 — ApplicationDbContext & Fluent API Configuration
Created `src/SmartCampus.Web/Data/ApplicationDbContext.cs` inheriting from `IdentityDbContext<IdentityUser>`:
- Configured cascading delete protection (`DeleteBehavior.Restrict`) on critical relational joins.
- Added high-performance composite non-clustered indexes on `(Status, CreatedAtUtc)`, `(LocationId, CategoryId, CreatedAtUtc)` for duplicate detection, and `(WorkOrderId, WorkerUserId)`.

### 5.3 Step 3.3 — LPU Reference Campus Data Seeder
Created `src/SmartCampus.Web/Data/DbInitializer.cs`:
- Seeded the 5 system roles (`SuperAdmin`, `CampusAdmin`, `DepartmentSupervisor`, `MaintenanceWorker`, `StudentTeacher`).
- Seeded institutional email whitelist (`@lpu.co.in`, `@lpu.in`).
- Seeded SLA policies: Critical (60 min response, 60 min resolution), High (120 min / 360 min), Medium (240 min / 1440 min), Low (480 min / 4320 min).
- Seeded 7 maintenance departments: Electrical, Plumbing, Civil, HVAC, IT Infrastructure, Housekeeping, Horticulture.
- Seeded LPU campus topology: Block 32 (Computer Science), Block 34 (Mechanical Engineering), Block 38 (Bio-Technology), Central Library, UniMall & Student Plaza, Boys Hostel 1 (BH-1), Girls Hostel 1 (GH-1), and outdoor campus zones.
- Seeded default test accounts for all 5 roles.
- Seeded representative pilot complaints across lifecycle stages (Submitted, Assigned, AwaitingVerification, and Closed with ratings).

---

## 6. Phase 4: Google OAuth 2.0, Institutional Whitelist & Identity RBAC

### 6.1 Step 4.1 — Institutional Domain Whitelist Validator
Created `src/SmartCampus.Web/Services/IEmailDomainValidator.cs` and `EmailDomainValidator.cs` querying `AllowedEmailDomains` table with fallback to configurable application domains.

### 6.2 Step 4.2 — AccountController & Authentication Flows
Created `src/SmartCampus.Web/Controllers/AccountController.cs` providing:
- Google OAuth 2.0 challenge and callback handler (`ExternalLoginCallback`).
- Server-side email verification and domain whitelist enforcement.
- Role-based redirection (`RedirectToRoleDashboard`).
- 1-click evaluation demo role switcher for institutional review and grading.

### 6.3 Step 4.3 — LPU UMS Visual Theme & Portals
- Authored `src/SmartCampus.Web/wwwroot/css/lpu-ums.css` delivering authentic LPU UMS branding (`#0f2b48` navy blue header, gold banners, crisp borders, and ID badges).
- Built responsive dashboards for all roles:
  * Admin Control Desk: `Areas/Admin/Views/Dashboard/Index.cshtml`
  * Department Supervisor: `Areas/Supervisor/Views/Dashboard/Index.cshtml`
  * Maintenance Worker: `Areas/Worker/Views/Dashboard/Index.cshtml`
  * Student/Faculty: `Areas/Reporter/Views/Dashboard/Index.cshtml`

---

## 7. Phase 5: Dynamic Campus Location Hierarchy & QR Code Generation

### 7.1 Step 5.1 — QR Code Generation Engine
Created `src/SmartCampus.Web/Services/IQRService.cs` and `QRService.cs` leveraging `QRCoder` to dynamically render Base64-encoded PNG and vector SVG QR codes encoding URL `https://smartcampus.lpu.in/Report?locId={GUID}`.

### 7.2 Step 5.2 — Location Hierarchy CRUD & API
- Created `src/SmartCampus.Web/Areas/Admin/Controllers/BuildingsController.cs` and views for managing Buildings, Floors, and Rooms/Outdoor Locations.
- Created `src/SmartCampus.Web/Controllers/LocationApiController.cs` exposing cascading dropdown endpoints (`/api/location/floors/{buildingId}`, `/api/location/rooms/{floorId}`), QR resolution endpoint (`/api/location/resolve-qr/{guid}`), and QR download modal.
- Created `src/SmartCampus.Web/Views/Home/CampusMap.cshtml` with interactive Leaflet.js GIS map.

---

## 8. Phase 6: Student & Teacher Complaint Reporting Module

### 8.1 Step 6.1 — Secure File Storage Service
Created `src/SmartCampus.Web/Services/IFileStorageService.cs` and `FileStorageService.cs` enforcing strict MIME verification, extension whitelist, randomized GUID filenames, and 10MB size cap under `wwwroot/uploads/`.

### 8.2 Step 6.2 — Complaint Submission & Tracking UI
- Created `src/SmartCampus.Web/Areas/Reporter/Controllers/ComplaintsController.cs`.
- `Create.cshtml`: Cascading building/floor/room selector with QR code auto-lock, trade category selector, and multi-photo upload with live image preview.
- `Index.cshtml`: "My Complaints" tabular view with real-time status pill badges.
- `Details.cshtml`: 10-stage visual progress stepper, side-by-side photographic verification display, immutable audit history timeline, 5-star rating submission, and 48-hour dispute/reopen workflow.

---

## 9. Phase 7: Campus Control Desk Triage & Duplicate Detection Heuristic

### 9.1 Step 7.1 — Real-Time Triage Console
Created `src/SmartCampus.Web/Areas/Admin/Controllers/TriageController.cs` and `Index.cshtml` filtering by Building, Priority, Department, and Status.

### 9.2 Step 7.2 — Duplicate Detection Heuristic
In `Review.cshtml`, the system automatically scans for complaints submitted within the past 48 hours sharing the identical location and category, warning control desk operators before duplicate work orders are dispatched.

### 9.3 Step 7.3 — SLA Policy Deadline Calculation
Upon ticket approval, the system fetches the SLA policy corresponding to the assigned priority tier and computes exact response, work-start, and resolution UTC deadlines stored on the ticket.

---

## 10. Phase 8: Department & Workforce Team Management

### 10.1 Step 8.1 — Department & Roster Management
Created `src/SmartCampus.Web/Areas/Admin/Controllers/DepartmentsController.cs` and views:
- `Index.cshtml`: Department directory with live active workload counters.
- `Teams.cshtml`: Trade squad configuration with designated Supervisors (Team Heads).
- `Workers.cshtml`: Technician roster with availability status toggles (`Available`, `Busy`, `Overloaded`, `OnLeave`).

---

## 11. Phase 9: Work Orders & Workforce Allocation Engine

### 11.1 Step 9.1 — Supervisor Work Order Dispatch
Created `src/SmartCampus.Web/Areas/Supervisor/Controllers/WorkOrdersController.cs` and `Assign.cshtml` enabling supervisors to allocate one or more technicians to a work order.

### 11.2 Step 9.2 — Mobile-Optimized Technician Task Desk
Created `src/SmartCampus.Web/Areas/Worker/Controllers/TasksController.cs` and views:
- `MyTasks.cshtml`: Task execution desk with quick state transitions (`Accept Task`, `Start On-Site Work`).
- `Complete.cshtml`: Completion workflow requiring mandatory upload of after-repair photographs and repair summary, transitioning the work order to `AwaitingVerification`.

---

## 12. Phase 10: Independent Verification Gate (Core Differentiator)

### 12.1 Step 10.1 — Verification Gate Governance
Strictly implemented the architectural rule: **`Work Completed != Resolved`**. Technicians are programmatically barred from verifying or closing their own work (`workOrder.Assignments.Any(a => a.WorkerUserId == currentUserId)` throws 403 Forbidden).

### 12.2 Step 10.2 — Side-by-Side Photographic Inspection Console
Created `src/SmartCampus.Web/Areas/Supervisor/Controllers/VerificationController.cs` and `Inspect.cshtml`:
- Displays reporter issue photo side-by-side with technician repair photo.
- Approval signs off on the work, transitions status to `VerifiedResolved`, and triggers reporter notification.
- Rejection requires defect notes, moving the task to `ReworkRequired` and returning it to the technician's queue.

---

## 13. Phase 11: SLA Monitoring & Multi-Tier Escalation Engine

### 13.1 Step 11.1 — Background SLA Hosted Service
Created `src/SmartCampus.Web/Services/SLAEngineHostedService.cs` running continuously:
- **50% elapsed:** Tier 1 technician reminder notification.
- **75% elapsed:** Tier 2 supervisor warning notification.
- **100% elapsed:** Work order marked `IsOverdue = true`, logged to `SLAEscalations` table, and high-priority escalation pushed to Control Desk.

### 13.2 Step 11.2 — SLA Compliance Monitor
Created `src/SmartCampus.Web/Areas/Admin/Controllers/SLAMonitorController.cs` and `Index.cshtml` displaying live SLA compliance %, overdue work orders, and audit log.

---

## 14. Phase 12: Interactive Campus Map & Dynamic QR Integration

### 14.1 Step 14.1 — Leaflet GIS Campus Map
Deployed `src/SmartCampus.Web/Views/Home/CampusMap.cshtml` with custom LPU campus boundaries (`31.2536° N, 75.7037° E`), interactive building markers, and live issue status pins.

### 14.2 Step 14.2 — Web Camera QR Scanner
Created `src/SmartCampus.Web/Views/Home/ScanQR.cshtml` utilizing the HTML5 camera stream to scan room doorplate QR codes and instantly navigate to the pre-filled complaint submission form.

---

## 15. Phase 13: Operations Analytics & Executive Dashboards

### 15.1 Step 15.1 — Chart.js Visual Analytics Dashboard
Created `src/SmartCampus.Web/Areas/Admin/Controllers/AnalyticsController.cs` and `Index.cshtml`:
- **Category Distribution (Doughnut Chart):** Volume by trade category.
- **Department Performance (Bar Chart):** Assigned vs Verified Resolved work orders.
- **Campus Hotspots (Horizontal Bar Chart):** Top 5 buildings with highest incident frequency.
- **Executive Metrics:** Total Complaints, Verified Resolved, SLA Compliance Rate %, MTTR (hours), and Student/Staff Rating.

### 15.2 Step 15.2 — Institutional Notification Center
Created `src/SmartCampus.Web/Controllers/NotificationsController.cs` and `Index.cshtml` managing user alerts, unread counts, and deep ticket navigation.

---

## 16. Phase 14: Advanced Optimization & Real-Time Push Channels

### 16.1 Step 16.1 — Intelligent Department Recommender
Created `src/SmartCampus.Web/Services/ISmartRecommenderService.cs` and `SmartRecommenderService.cs` tokenizing issue titles and descriptions against weighted trade vocabularies to recommend target departments with confidence percentages and crew sizes in `TriageController.Review`.

### 16.2 Step 16.2 — Workload Auto-Balancing Algorithm
In `WorkOrdersController.Assign` and `Assign.cshtml`, the system counts active task assignments per technician, displaying an auto-balancing meter and sorting available technicians with the lowest active task count first.

### 16.3 Step 16.3 — SignalR Real-Time Notification Hub
Created `src/SmartCampus.Web/Hubs/NotificationHub.cs` and registered `/hubs/notifications` in `Program.cs`. Client-side script `site.js` connects to the hub and displays non-intrusive institutional toast alerts upon ticket assignments and SLA breaches.

---

## 17. Phase 15: Rigorous Verification, Hardening & Deployment

### 17.1 Step 17.1 — Security Audit & Validation
- **Anti-Forgery Protection:** All mutating HTTP POST actions protected with `[ValidateAntiForgeryToken]`.
- **Upload Sandboxing:** Hashed directory storage, MIME-type validation, and randomized GUID filenames prevent path traversal.
- **SQL Injection Immunity:** 100% parameterized queries via EF Core LINQ.
- **RBAC Boundaries:** Strict role policies and tenant boundaries enforced across all 4 MVC areas.

### 17.2 Step 17.2 — Production Build Verification
Verified complete solution compilation with 0 errors:
```powershell
dotnet build -c Release src/SmartCampus.Web/SmartCampus.Web.csproj
# Build succeeded. 0 Error(s). 4 Warning(s).
```

### 17.3 Step 17.3 — Seeded Pilot Data
Seeded 4 realistic complaints covering all major lifecycle stages (Submitted, Assigned, AwaitingVerification, Closed) for evaluation.

