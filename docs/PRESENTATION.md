# SmartCampus: Project Presentation Deck & Academic Defense Script

**Presentation Title:** SmartCampus: Intelligent Campus Issue Reporting, Workforce Allocation & Resolution Verification System  
**Institution:** Lovely Professional University (LPU), Phagwara, Punjab, India  
**Degree & Department:** B.Tech. in Computer Science & Engineering  
**Academic Session:** 2025 – 2026  
**Technology Stack:** ASP.NET Core MVC (.NET 10 LTS), EF Core 10, SQL Server, Bootstrap 5 (LPU UMS Theme)  
**GitHub Repository:** [https://github.com/DipanshuDhaka/smart_Campus](https://github.com/DipanshuDhaka/smart_Campus)

---

### Project Personnel & Evaluation Metadata

| Role | Details |
| :--- | :--- |
| **Submitted To (Project Evaluator):** | **Mr. Aryan Chauhan** (Teacher ID: 35200), Department of CSE, Lovely Professional University |
| **Submitted By (Group Project):** | **Dipanshu** (Reg No: 12323250) — *Lead Architecture, EF Core & Backend*<br/>**Vishnu** (Reg No: 12307182) — *Control Desk Triage, SLA Engine & SignalR*<br/>**Harsh** (Reg No: 12306879) — *Spatial GIS, QR Automation & UI/UX* |

---

## Slide 1: Title & Academic Identity

```
╔════════════════════════════════════════════════════════════════════════════════════════╗
║                             LOVELY PROFESSIONAL UNIVERSITY                             ║
║                       Department of Computer Science & Engineering                     ║
║                                                                                        ║
║                                      SMARTCAMPUS                                       ║
║         Intelligent Campus Issue Reporting, Workforce Allocation & Verification        ║
║                                                                                        ║
║   SUBMITTED BY:                                    SUBMITTED TO:                       ║
║   • Dipanshu (12323250)                            Mr. Aryan Chauhan (ID: 35200)       ║
║   • Vishnu   (12307182)                            Department of CSE, LPU              ║
║   • Harsh    (12306879)                                                                ║
╚════════════════════════════════════════════════════════════════════════════════════════╝
```

### Speaker Notes
> "Respected evaluator Aryan Chauhan sir, and members of the examination committee. Welcome to our capstone presentation on **SmartCampus**, presented by Dipanshu, Vishnu, and Harsh. SmartCampus is an enterprise-grade campus maintenance and workforce governance platform engineered specifically for large-scale academic institutions, modeled directly around Lovely Professional University. Today, we demonstrate how this system transforms campus facility management from informal, untracked channels into an auditable, automated, and quality-verified digital workflow."

---

## Slide 2: Real Campus Motivation & Operational Problem Statement

### 1. Campus Scale & Physical Plant
- **600+ contiguous acres** in Phagwara, Punjab.
- **60+ academic blocks** (Block 32 School of CSE, Block 34 Mechanical, Block 38 Electronics).
- **14+ residential hostels** (BH-1 to BH-8, GH-1 to GH-6), Central Library, UniMall, and outdoor utility networks.
- **30,000+ students and 5,000+ faculty and staff** generating dozens of daily breakdown requests across electrical, plumbing, civil, HVAC, and network domains.

### 2. The Operational Breakdown
1. **Informal & Lost Reporting:** WhatsApp groups, phone calls, and paper register entries lack audit history, unique ticket identifiers, and time-tracking.
2. **The "False Resolution" Trap:** Technicians mark breakdown complaints as "completed" without physical on-site repair. Students discover the fixture is still defective.
3. **Unequal Workforce Allocation:** Certain technicians are overwhelmed with 8+ tasks while others remain idle with 0 active work orders.
4. **SLA Neglect:** High-risk safety hazards (e.g. electrical sparking in computing labs, burst pipes in hostels) linger without automated escalation deadlines.

### 3. The SmartCampus Solution
- **10-Stage Auditable State Machine:** Tracks issues from student submission to verified supervisory closure.
- **The Cardinal Principle:** *"Work Completed ≠ Verified Resolved"*. Mandatory independent supervisory inspection.
- **Smart Recommender & Load Balancer:** Tokenizes problem descriptions, scores trade keywords, and distributes tasks to technicians with the lowest active workload.
- **Automated SLA Engine:** 60-second background daemon triggers 50% reminders, 75% warnings, and 100% breach escalations.

---

## Slide 3: System Architecture Paradigm & Technology Stack

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           PRESENTATION LAYER                                │
│   ASP.NET Core MVC 10 • Razor Views • Bootstrap 5 (LPU UMS Theme)           │
│   Leaflet.js GIS Campus Map • Chart.js Operational Analytics                │
├─────────────────────────────────────────────────────────────────────────────┤
│                           APPLICATION SERVICES                              │
│   Area Controllers: Admin, Supervisor, Worker, Reporter                     │
│   Smart Recommender • Workload Auto-Balancer • QRCoder • FileStorage        │
├─────────────────────────────────────────────────────────────────────────────┤
│                        EVENT & REAL-TIME SERVICES                           │
│   SLAEngineHostedService (60s loop) • SignalR NotificationHub (WebSockets)  │
│   3-Tier Escalation Engine • Dual-Factor Quality Verification Gate          │
├─────────────────────────────────────────────────────────────────────────────┤
│                           DATA & PERSISTENCE                                │
│   Microsoft SQL Server • EF Core 10 Code-First • ASP.NET Core Identity      │
│   Google OAuth 2.0 Domain Whitelist • Fluent API Referential Integrity      │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Slide 4: Unit 1 — Enterprise MVC Architecture & Area-Based Routing

### Syllabus Topic: Model-View-Controller (MVC) Pattern & Request Pipeline
- **Theoretical Foundation:** MVC decouples application logic into three distinct layers:
  1. *Model:* Domain data structures and business validation rules.
  2. *View:* Presentation rendering and dynamic UI display via Razor (`.cshtml`).
  3. *Controller:* HTTP request processing, action filter execution, and model-view coordination.
- **Area Partitioning:** ASP.NET Core Areas logically isolate large enterprise applications into modular sub-applications, each containing its own controllers, views, and routing tables.

### Practical Use Case in SmartCampus
- **4 Discrete Functional Areas:**
  - `Admin`: Central Control Desk, complaint triage, building spatial manager, operations analytics.
  - `Supervisor`: Trade department dispatch, technician roster allocation, verification quality gate.
  - `Worker`: Mobile field task portal, task acknowledgement, before/after evidence upload.
  - `Reporter`: Student/staff complaint creation, tracking timeline, feedback ratings.
- **Unified Master Layout:** All areas inherit from `_Layout.cshtml`, styled in the authentic LPU UMS visual aesthetic (navy `#0F2B48` and ochre gold `#D48A14`).
- **Code Artifact:** `src/SmartCampus.Web/Areas/Worker/Controllers/DashboardController.cs` annotated with `[Area("Worker")]` and `[Authorize(Roles = "MaintenanceWorker")]`.

---

## Slide 5: Unit 2 — Relational Data Modeling & EF Core Code-First ORM

### Syllabus Topic: Code-First ORM, Fluent API & Relational Constraints
- **Theoretical Foundation:** Object-Relational Mapping (ORM) maps strongly typed C# classes to relational database tables, eliminating raw string SQL vulnerabilities and data type mismatches.
- **Fluent API Configuration:** `OnModelCreating` configures composite primary keys, table relationships, foreign key constraints, and indexing pipelines.
- **Referential Integrity & Cascading:** Setting `DeleteBehavior.Restrict` prevents accidental cascading deletes of dependent child records when parent rows are modified.

### Practical Use Case in SmartCampus: 3-Tier Spatial Topology
- **Hierarchical Spatial Modeling:** `Building (1) -> Floor (N) -> Location (N)`. A room cannot exist without an assigned floor, and a floor cannot exist without a parent building.
- **Live Database Verification:** Populated with newly inserted campus assets:
  - Block 32: 2nd Floor (Robotics & IoT Wing) -> Room 32-201 (Robotics & Embedded Systems Lab).
  - Block 34: 1st Floor (Mechatronics & Structural Wing) -> Room 34-101 (Advanced Hydraulics Lab).
  - BH-01: 1st Floor (East Wing) -> Room BH1-101 (Resident Room 101).
- **11 Relational Modules (33 Tables):** Covering complaints, work orders, assignments, verifications, SLA policies, and audit histories.
- **Code Artifact:** `src/SmartCampus.Web/Data/ApplicationDbContext.cs`, using Fluent API to configure `LocationGuid` unique index and restrict cascade deletion.

---

## Slide 6: Unit 3 — Identity Management, OAuth 2.0 & Role Security

### Syllabus Topic: Claims-Based Authorization & OAuth 2.0 Whitelisting
- **Theoretical Foundation:** Authentication verifies user identity, while Authorization governs role-based privileges. OAuth 2.0 enables secure token delegation with trusted identity providers.
- **Anti-CSRF Protection:** Cryptographic anti-forgery tokens validate all state-mutating HTTP POST requests.
- **Defense-in-Depth:** Combining middleware authentication, declarative role attributes, and server-side claim verification.

### Practical Use Case in SmartCampus: 5-Tier RBAC & Single Allocation
- **Google SSO with Institutional Whitelist:** Permitted email domains (`@lpu.co.in`, `@lpu.in`) are stored in `AllowedEmailDomains` table, preventing unauthorized external access.
- **5-Tier Governance Matrix:**
  1. `StudentTeacher`: Report issues, view personal history, rate service.
  2. `CampusAdmin`: Campus Control Desk triage, duplicate detection, priority assignment.
  3. `DepartmentSupervisor`: Dispatch work orders, manage squads, inspect and verify repairs.
  4. `MaintenanceWorker`: Mobile field execution, accept/start tasks, upload completion proof.
  5. `SuperAdmin`: Global user management, domain authorization, SLA policy governance.
- **Single-Allocation Constraint:** Enforces that a work order can only be assigned once. Multi-assignment is locked with an atomic check `workOrder.Assignments.Any()`.
- **Code Artifact:** `AccountController.cs` domain validation, `WorkOrdersController.cs` single-allocation check.

---

## Slide 7: Unit 4 — Background Hosted Services & Real-Time SignalR

### Syllabus Topic: ASP.NET Core BackgroundService & WebSockets
- **Theoretical Foundation:** Asynchronous recurring background processes run outside HTTP request threads via `IHostedService` / `BackgroundService`. SignalR provides full-duplex WebSocket channels for instant server-to-client push updates.
- **Graceful Execution Loop:** `ExecuteAsync` handles continuous background execution with `CancellationToken` support.

### Practical Use Case in SmartCampus: Automated SLA Escalation Engine
- **Continuous 60-Second Scan:** `SLAEngineHostedService` runs as a managed daemon in the ASP.NET Core process, inspecting active tickets against configured `SLAPolicies`.
- **3-Tier Escalation Thresholds:**
  - *50% Elapsed:* Tier 1 Reminder to Assigned Maintenance Technician.
  - *75% Elapsed:* Tier 2 Warning to Department Supervisor.
  - *100% Elapsed:* Tier 3 Breach Escalation to Campus Control Desk & SuperAdmin.
- **SignalR Live Push:** Dispatches toast alerts over `/hubs/notifications` to active browser clients without polling.
- **Code Artifact:** `src/SmartCampus.Web/Services/SLAEngineHostedService.cs` and `NotificationHub.cs`.

---

## Slide 8: Unit 5 — Geospatial GIS Mapping & Vector QR Code Automation

### Syllabus Topic: Spatial Mapping & Matrix Barcode Encoding
- **Theoretical Foundation:** Translating WGS84 geographic coordinate pairs into interactive map layers, and encoding structured URLs into 2D matrix bit patterns using Reed-Solomon algebraic error correction algorithms.
- **Asynchronous RESTful APIs:** Client-side AJAX consuming lightweight JSON endpoints for dynamic UI cascading.

### Practical Use Case in SmartCampus: LPU GIS Map & Room Doorplates
- **Interactive LPU Campus GIS Map:** Leaflet.js map initialized at LPU campus coordinates (`31.2536° N, 75.7037° E`), plotting 60+ buildings with live incident hotspot markers.
- **Algorithmic Room Doorplates:** Every room generates a unique SVG/PNG QR doorplate using QRCoder. Scanning with a smartphone instantly pre-fills Building, Floor, and Room.
- **AJAX Cascading Dropdowns:** Selecting 'Block 32' fetches floors via `/api/LocationApi/floors/1`, and selecting '2nd Floor' fetches rooms via `/api/LocationApi/locations/2002`.
- **Code Artifact:** `LocationApiController.cs`, `CampusMap.cshtml`, `QRCodeService.cs`.

---

## Slide 9: Unit 6 — Software Verification Gates & Quality Engineering

### Syllabus Topic: Quality Assurance & Independent Verification Gates
- **Theoretical Foundation:** Separation of Duties dictates that executors must not verify their own work. Quality engineering requires an auditable handover gate where an independent supervisor validates physical evidence before closure.
- **Operational Reliability Metrics:** Mean Time to Resolution (MTTR), SLA compliance percentages, and failure hotspot densities.

### The Cardinal Principle: "Work Completed ≠ Verified Resolved"
- **Mandatory Quality Gate:** When a technician completes work, the ticket transitions to `AwaitingVerification`. It CANNOT be closed by the worker.
- **Physical Supervisory Audit:** The Department Supervisor physically visits the site, verifies before/after photos, enters qualitative remarks, and assigns a 1-5 star quality rating.
- **Substandard Repair Rework Loop:** If a repair is substandard, the supervisor formally rejects the handover, transitioning the ticket to `ReworkRequired` back to the field technician.
- **Code Artifact:** `VerificationController.cs` (Approve/Reject actions), `Verifications` relational table logging signed inspections.

---

## Slide 10: Live Workflow Demo — Steps 1 & 2 (Reporting & Triage)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 1: Student Issue Filing (Student: student.test@lpu.in)                 │
│ • Location: Block 32 -> 2nd Floor (Robotics Wing) -> Room 32-201            │
│ • Category: Electrical & Lighting                                           │
│ • Description: Short circuit & MCB sparking near workbench 3                │
│ • Result: Generates Ticket SC-2026-00005 (Status: Submitted)               │
│ • Evidence: docs/screenshots/wf_02_complaint_form_filled.png                │
├─────────────────────────────────────────────────────────────────────────────┤
│ STEP 2: Control Desk Triage (Officer Harpreet Singh)                        │
│ • Duplicate Detection: Scans past 48 hours for identical room/category      │
│ • Smart Recommender: Matches #sparking, #mcb -> Electrical Dept             │
│ • Priority: Critical (SLA: 1 Hour)                                          │
│ • Result: Dispatches Work Order WO-2026-00004                               │
│ • Evidence: docs/screenshots/wf_04_triage_review_page.png                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Slide 11: Live Workflow Demo — Steps 3 & 4 (Allocation & Execution)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 3: Supervisor Allocation (Er. Manjit Kumar - Electrical Head)          │
│ • Workload Auto-Balancing: Ranks technicians by lowest active task count    │
│ • Allocates Senior Electrician Rajesh Verma                                 │
│ • Single-Allocation Lock: Prevents duplicate allocations                    │
│ • Evidence: docs/screenshots/wf_07_supervisor_allocate_page.png             │
├─────────────────────────────────────────────────────────────────────────────┤
│ STEP 4: Technician Execution (Senior Electrician Rajesh Verma)              │
│ • Electrician accepts task -> Status: WorkAccepted                          │
│ • Starts on-site work -> Status: InProgress                                 │
│ • Replaces 32A Schneider MCB, balances circuit, tests 230V stable load      │
│ • Uploads after-repair photographic evidence & repair notes                 │
│ • Handover: Transitions to 'AwaitingVerification' (Cannot self-close)       │
│ • Evidence: docs/screenshots/wf_11_electrician_complete_form.png            │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Slide 12: Live Workflow Demo — Step 5 (Verification Gate & Closure)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ STEP 5A: Supervisor Quality Inspection (Er. Manjit Kumar)                   │
│ • Supervisor physically inspects Room 32-201 on site                        │
│ • Reviews before/after photos and conducts thermal scan (34°C normal)       │
│ • Awards 5-star quality rating and submits formal sign-off remarks          │
│ • Result: Approves ticket -> Transitions to 'VerifiedResolved'              │
│ • Evidence: docs/screenshots/wf_14_verification_inspect_gate.png            │
├─────────────────────────────────────────────────────────────────────────────┤
│ STEP 5B: Student Final Verified Resolution (student.test@lpu.in)            │
│ • Student views updated ticket with completed 10-stage timeline             │
│ • Verified resolved within 1-hour SLA target                                │
│ • Complete audit history preserved in database                              │
│ • Evidence: docs/screenshots/wf_16_student_verified_resolved_final.png      │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Slide 13: Key Engineering Challenges & Technical Solutions

| # | Challenge Encountered | Technical Root Cause | Engineering Solution Implemented |
| :--- | :--- | :--- | :--- |
| **1** | Multi-Assignment Bug | Supervisor could click "Allocate Squad" repeatedly | Enforced `workOrder.Assignments.Any()` check in controller and replaced button with "Squad Allocated" badge |
| **2** | Technician Dashboard Usability | Worker dashboard had disabled action buttons | Refactored `Dashboard/Index.cshtml` to dynamically render direct action forms (Accept, Start, Submit Evidence) |
| **3** | Spatial Dropdown Performance | Rendering 60+ blocks and 1000+ rooms bloated DOM | Built asynchronous RESTful `LocationApi` with dynamic cascading fetch pipelines on demand |
| **4** | Self-Verification Fraud Risk | Technicians attempting to approve their own tasks | Enforced server-side claim assertion `WorkerUserId != CurrentUserId` in `VerificationController.cs` |

---

## Slide 14: Student Team Contributions & Academic Outcomes

### 1. Dipanshu (Registration Number: 12323250)
- **Role:** Lead Architecture & Backend Engineering
- **Contributions:** Architected ASP.NET Core MVC solution across 4 Areas; designed 11 relational modules with EF Core 10 Fluent API; configured Google OAuth 2.0 whitelist; enforced single-allocation constraint and ACID transactions.

### 2. Vishnu (Registration Number: 12307182)
- **Role:** Control Desk Triage & SLA Real-Time Engine
- **Contributions:** Implemented Control Desk triage queue with 48-hour duplicate detection; developed `SLAEngineHostedService` 60-second background daemon; built SignalR notification hub and operations analytics dashboards.

### 3. Harsh (Registration Number: 12306879)
- **Role:** Spatial GIS, QR Automation & UI/UX Design
- **Contributions:** Implemented Leaflet.js interactive GIS map for 60+ LPU campus blocks; built QRCoder dynamic vector QR doorplate engine; designed authentic LPU UMS visual aesthetic with Bootstrap 5; engineered mobile technician task flow.

---

## Slide 15: Conclusion, GitHub Repository & Defense Q&A

### Summary of Engineering Accomplishments
- **Production-Ready Platform:** All 15 development phases complete and verified with 0 compilation errors across .NET 10 LTS.
- **Cardinal Verification Gate:** "Work Completed ≠ Verified Resolved" eliminates the False Resolution Trap.
- **Live Database Grounding:** Verified with 60+ buildings, floors, rooms, and complete lifecycle tickets.
- **Automated Governance:** 60-second SLA daemon, workload auto-balancer, and real-time SignalR alerts.
- **GitHub Repository:** [https://github.com/DipanshuDhaka/smart_Campus](https://github.com/DipanshuDhaka/smart_Campus) (All 187 files synchronized).

### Defense Closing Statement
> "Respected evaluator Aryan Chauhan sir, and members of the examination committee: SmartCampus demonstrates how academic software engineering principles—MVC design patterns, relational normalization, claims-based security, asynchronous hosted services, and independent verification gates—converge into a robust enterprise solution for Lovely Professional University. We thank you for your mentorship and are now open for technical questions."
