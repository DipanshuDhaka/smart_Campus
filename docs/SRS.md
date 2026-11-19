# Software Requirements Specification (SRS)
## SmartCampus: Intelligent Campus Issue Reporting, Workforce Allocation & Resolution Verification System

**Document Version:** 1.0.0  
**Date:** 2026-11-12  
**Target Environment:** ASP.NET Core MVC (.NET 10 LTS), Entity Framework Core 10, SQL Server (`SmartCampusDB`)  
**Reference Institution:** Lovely Professional University (LPU), Punjab, India  

---

## Table of Contents
1. [Introduction](#1-introduction)
   - 1.1 Purpose
   - 1.2 Scope
   - 1.3 Definitions, Acronyms, and Abbreviations
   - 1.4 References
2. [Overall Description](#2-overall-description)
   - 2.1 Product Perspective
   - 2.2 User Classes and Characteristics
   - 2.3 Operating Environment
   - 2.4 Design and Implementation Constraints
   - 2.5 Assumptions and Dependencies
3. [System Features & Functional Requirements](#3-system-features--functional-requirements)
   - 3.1 Authentication, Institutional Domain Validation & RBAC (FR-AUTH)
   - 3.2 Dynamic Campus Location & Spatial Hierarchy (FR-LOC)
   - 3.3 Student & Teacher Complaint Reporting (FR-COMP)
   - 3.4 Campus Control Desk Triage & SLA Assignment (FR-TRIAGE)
   - 3.5 Department & Workforce Team Management (FR-TEAM)
   - 3.6 Work Order Generation & Technician Allocation (FR-WO)
   - 3.7 Technician Task Execution & Evidence Submission (FR-TECH)
   - 3.8 Independent Physical Verification Gate (FR-VERIF)
   - 3.9 Automated SLA Monitoring & Multi-Tier Escalation (FR-SLA)
   - 3.10 Interactive Campus Issue Map & QR Code Integration (FR-MAP-QR)
   - 3.11 Operational Analytics & Executive Dashboards (FR-ANALYTICS)
   - 3.12 Notifications, Rating & Dispute Management (FR-FEEDBACK)
4. [External Interface Requirements](#4-external-interface-requirements)
   - 4.1 User Interfaces
   - 4.2 Hardware Interfaces
   - 4.3 Software & API Interfaces
5. [Non-Functional Requirements (NFR)](#5-non-functional-requirements-nfr)
   - 5.1 Security Requirements
   - 5.2 Performance & Scalability
   - 5.3 Reliability, Availability & Data Integrity
   - 5.4 Maintainability & Extensibility
6. [Data Dictionary & Entity Summary](#6-data-dictionary--entity-summary)

---

## 1. Introduction

### 1.1 Purpose
This Software Requirements Specification (SRS) establishes the complete functional and non-functional specifications for **SmartCampus**. The system is an enterprise facilities maintenance management portal designed to streamline infrastructure issue reporting, algorithmic workforce allocation, automated SLA deadline monitoring, and independent physical verification across high-density university campuses, using Lovely Professional University (LPU) as the primary deployment baseline.

### 1.2 Scope
The SmartCampus platform encompasses:
- Single Sign-On (SSO) authentication via Google OAuth 2.0 with institutional domain restrictions (e.g., `@lpu.co.in`, `@lpu.in`).
- Five distinct Role-Based Access Control (RBAC) tiers: Student/Teacher, Campus Control Desk (Administrator), Department Supervisor (Team Head), Maintenance Worker (Technician), and Super Administrator.
- Dynamic hierarchical location management spanning 600+ acres, 60+ multi-story academic blocks, residential hostels, and outdoor infrastructure.
- Complete 10-stage complaint lifecycle enforcing the architectural rule: **`Work Completed != Resolved`**.
- Rule-based workforce recommendation and workload balancing across maintenance trades (Electrical, Plumbing, HVAC, Carpentry, Civil, IT/Network, Sanitation).
- Background SLA monitoring with tiered threshold triggers (50% reminder, 75% supervisor warning, 100% administrator escalation).
- Leaflet/OpenStreetMap geospatial mapping and dynamic QR-code-assisted reporting.

### 1.3 Definitions, Acronyms, and Abbreviations
- **LPU:** Lovely Professional University.
- **RBAC:** Role-Based Access Control.
- **SLA:** Service Level Agreement.
- **Work Order (WO):** A discrete operational work package issued to a specific trade team to repair an identified problem.
- **Independent Verification:** A physical inspection performed by a designated Team Head (never the technician who performed the repair) with photographic proof to validate work quality prior to resolution.
- **MTTR:** Mean Time To Resolve.
- **QR Code:** Quick Response 2D matrix barcode linked to a specific campus room, block, or outdoor asset.

---

## 2. Overall Description

### 2.1 Product Perspective
SmartCampus operates as a centralized web application built on ASP.NET Core MVC (.NET 10 LTS) with backend SQL Server storage (`SmartCampusDB`). The application functions as an authoritative hub coordinating between three interconnected operational planes:
1. **The Reporting Plane:** Students, faculty, and campus staff reporting faults on the ground via mobile-responsive web pages or QR scans.
2. **The Command & Allocation Plane:** Control desk administrators and trade supervisors triaging tickets, prioritizing hazards, setting SLAs, and distributing work orders.
3. **The Field Execution & Verification Plane:** On-site technicians accepting tasks and uploading completion evidence, followed by supervisors executing independent verification sign-offs.

### 2.2 User Classes and Characteristics
- **Students & Teachers (Reporters):** Low technical overhead; require intuitive mobile-first forms, instant image attachments, real-time ticket progress timelines, and satisfaction rating controls.
- **Control Desk Officers (Campus Administrators):** Operational dispatchers; require fast search/filtering, duplicate detection heuristics, rapid priority classification, and cross-departmental work-order dispatch.
- **Department Supervisors (Team Heads):** Technical managers (e.g., Head of Electrical Maintenance); require team roster overviews, technician workload meters, and dedicated inspection interfaces for approving/rejecting completion evidence.
- **Maintenance Technicians (Workers):** Field tradespeople (electricians, plumbers, masons); require streamlined task checklists, simple "Accept / Start / Complete" status buttons, and camera upload inputs.
- **Super Administrators:** IT/Campus leadership; require administrative configuration of buildings, departments, email domain policies, SLA targets, and campus-wide analytical reports.

### 2.3 Operating Environment
- **Host OS:** Windows Server 2022 / Windows 11.
- **Runtime:** .NET 10 LTS Runtime (`Microsoft.AspNetCore.App 10.0.x`).
- **Database Engine:** Microsoft SQL Server 2022 / Azure SQL Database.
- **Client Browsers:** Google Chrome 120+, Microsoft Edge 120+, Safari 17+, Mozilla Firefox 120+ (Desktop & Mobile).

### 2.4 Design and Implementation Constraints
1. **Separation of Verification (Strict Constraint):** Under no circumstance may the worker assigned to a work order approve or verify their own completion. Verification requires a separate authorized user with the `DepartmentSupervisor` or `CampusAdmin` role.
2. **Zero Hardcoded Geography:** All academic blocks (Block 32, 34, 38, etc.), hostels (BH-1 to BH-8, GH-1 to GH-6), floors, and rooms must reside as data rows in the database, allowing instant adaptation to any campus.
3. **Secure File Storage:** Images must not be stored as large binary blobs in SQL Server tables. Images are persisted on disk/storage with unique GUID identifiers and verified MIME types, with only relative URLs stored in the database.

---

## 3. System Features & Functional Requirements

### 3.1 Authentication, Institutional Domain Validation & RBAC (FR-AUTH)
- **FR-AUTH-1:** The system shall authenticate users via Google OAuth 2.0. Passwords for Google accounts shall never be collected or stored.
- **FR-AUTH-2:** The backend shall validate the Google identity token on the server and check that `email_verified == true`.
- **FR-AUTH-3:** The backend shall check the user's email domain against the `AllowedEmailDomains` table. If the domain is inactive or not whitelisted, registration/login shall be denied with an informative error message.
- **FR-AUTH-4:** Upon first successful Google login, the user account shall be created in ASP.NET Core Identity with the default role of `StudentTeacher`.
- **FR-AUTH-5:** Elevation to `MaintenanceWorker`, `DepartmentSupervisor`, `CampusAdmin`, or `SuperAdmin` shall only be executable by an active Administrator.
- **FR-AUTH-6:** Controller actions and API endpoints shall be protected by `[Authorize(Roles = "...")]` attributes matching the RBAC matrix.

### 3.2 Dynamic Campus Location & Spatial Hierarchy (FR-LOC)
- **FR-LOC-1:** The system shall maintain a three-tier spatial hierarchy: Building -> Floor -> Location (Indoor Room / Outdoor Zone).
- **FR-LOC-2:** Buildings shall store Code, Name, Latitude, Longitude, Category (Academic, Hostel, Administrative, Utility, Commercial), and Total Floors.
- **FR-LOC-3:** Locations shall store Room Number / Name, Location Type (Classroom, Laboratory, Office, Washroom, Corridor, Outdoor Pathway, Parking, Drainage), and GPS coordinates.
- **FR-LOC-4:** The system shall generate a unique cryptographic GUID and QR code URL for each location (`/Report?locId={GUID}`).

### 3.3 Student & Teacher Complaint Reporting (FR-COMP)
- **FR-COMP-1:** Users shall be able to log a complaint by selecting Issue Category, Building, Floor, and Location, or by scanning a Location QR code to pre-populate location details.
- **FR-COMP-2:** Users shall be able to upload 1 to 3 JPEG/PNG/WebP photos documenting the issue (max 10MB each).
- **FR-COMP-3:** Users shall provide a title, description, and optional callback phone number.
- **FR-COMP-4:** The system shall immediately generate a unique tracking ticket number (e.g., `SC-2026-00492`) and set initial status to `Submitted`.
- **FR-COMP-5:** The "My Complaints" portal shall display all complaints created by the logged-in user with color-coded status badges, SLA targets, and visual audit timelines.

### 3.4 Campus Control Desk Triage & SLA Assignment (FR-TRIAGE)
- **FR-TRIAGE-1:** Campus Administrators shall access a centralized Triage Queue displaying all pending submissions.
- **FR-TRIAGE-2:** Administrators can set status to `UnderReview` upon opening a ticket.
- **FR-TRIAGE-3:** If a ticket is rejected (e.g., duplicate, spam, out of scope), the administrator must supply a mandatory rejection reason, and the status changes to `Rejected`.
- **FR-TRIAGE-4:** If approved, the administrator shall set the Priority (`Critical`, `High`, `Medium`, `Low`), which automatically calculates Response, Work-Start, and Resolution deadlines according to active `SLAPolicies`.
- **FR-TRIAGE-5:** The system shall execute duplicate detection: highlighting existing active complaints at the same location within the same category submitted in the last 24 hours.

### 3.5 Department & Workforce Team Management (FR-TEAM)
- **FR-TEAM-1:** The system shall support customizable departments (Electrical, Plumbing, HVAC, Carpentry, Civil/Masonry, Network/IT, Housekeeping/Sanitation).
- **FR-TEAM-2:** Each department shall have one or more Teams led by a Department Supervisor (Team Head).
- **FR-TEAM-3:** Maintenance workers shall be associated with a Team and tagged with trade skills (e.g., "High Voltage Certified", "Chiller Technician", "Sanitary Plumber").
- **FR-TEAM-4:** The system shall monitor each worker's active task count and present an availability indicator (Available: 0–1 tasks, Busy: 2–3 tasks, Overloaded: >3 tasks, On Leave).

### 3.6 Work Order Generation & Technician Allocation (FR-WO)
- **FR-WO-1:** Approved complaints shall generate one or more `WorkOrders`. Multi-trade incidents shall generate separate work orders per involved department.
- **FR-WO-2:** The system shall recommend the responsible department based on the complaint category.
- **FR-WO-3:** The Team Head or Administrator shall allocate one or more technicians to each Work Order.
- **FR-WO-4:** When dispatched, the complaint and work order status shall transition to `Assigned`, notifying assigned workers.

### 3.7 Technician Task Execution & Evidence Submission (FR-TECH)
- **FR-TECH-1:** Technicians shall access a mobile-optimized "My Tasks" portal displaying only work orders assigned to them.
- **FR-TECH-2:** The technician can click "Accept Task", updating the status to `WorkAccepted`.
- **FR-TECH-3:** When commencing physical repair on-site, the technician clicks "Start Work", updating the status to `InProgress`.
- **FR-TECH-4:** Upon finishing the repair, the technician MUST upload 1 to 3 "After-Repair" completion photographs and completion notes. Clicking "Submit for Verification" transitions the work order to `WorkCompleted`.
- **FR-TECH-5:** Transitioning to `WorkCompleted` SHALL NOT resolve the ticket; it triggers the transition to `AwaitingVerification`.

### 3.8 Independent Physical Verification Gate (FR-VERIF)
- **FR-VERIF-1:** Work orders in `WorkCompleted` / `AwaitingVerification` shall appear in the designated Department Supervisor's verification queue.
- **FR-VERIF-2:** The inspection portal shall display the initial problem photo side-by-side with the worker's completion photo.
- **FR-VERIF-3 (Golden Rule):** The system shall strictly prohibit the worker who performed the repair from approving verification.
- **FR-VERIF-4 (Verification Approval):** If the supervisor approves the physical inspection, they must upload an inspection sign-off photo/remarks, moving status to `VerifiedResolved`.
- **FR-VERIF-5 (Verification Rejection):** If the repair is deficient, the supervisor selects "Reject & Mandate Rework" with specific rework feedback and defect photos. Status returns to `ReworkRequired` / `InProgress`, alerting the assigned technician.

### 3.9 Automated SLA Monitoring & Multi-Tier Escalation (FR-SLA)
- **FR-SLA-1:** A background hosted service (`SLAEngineHostedService`) shall poll active work orders at 60-second intervals to compute elapsed time versus resolution deadlines.
- **FR-SLA-2 (Tier 1 - 50% Elapsed):** Sends in-app reminder to the assigned technician.
- **FR-SLA-3 (Tier 2 - 75% Elapsed):** Dispatches warning notification to the Department Supervisor.
- **FR-SLA-4 (Tier 3 - 100% Breached):** Marks the record as `IsOverdue = true` and dispatches escalation alerts to the Campus Control Desk and Administrator.
- **FR-SLA-5:** All SLA breaches shall be recorded in `SLAEscalations` with timestamp and breach duration for compliance reporting.

### 3.10 Interactive Campus Issue Map & QR Code Integration (FR-MAP-QR)
- **FR-MAP-QR-1:** An interactive Leaflet.js map shall render the LPU campus with markers for all buildings and outdoor complaint locations.
- **FR-MAP-QR-2:** Map markers shall be color-coded by urgency (Red = Critical, Orange = High, Yellow = Medium, Blue = Low, Green = Resolved).
- **FR-MAP-QR-3:** Users can filter the map by Building, Category, Priority, and Status.
- **FR-MAP-QR-4:** The system shall support a web-based QR camera scanner allowing users to scan room doorplates and jump directly to pre-filled report forms.

### 3.11 Operational Analytics & Executive Dashboards (FR-ANALYTICS)
- **FR-ANALYTICS-1:** Executive charts (Chart.js) shall display: Total Complaints, Resolved Complaints, Rejection Rate, Average MTTR (hours), and SLA Compliance percentage.
- **FR-ANALYTICS-2:** Department comparison breakdown: Total work orders handled, on-time completion rate, and rework rejection rate.
- **FR-ANALYTICS-3:** High-issue hotspot ranking: Top 10 campus blocks and locations generating the highest volume of maintenance complaints.

### 3.12 Notifications, Rating & Dispute Management (FR-FEEDBACK)
- **FR-FEEDBACK-1:** When a complaint reaches `VerifiedResolved`, the original reporter shall be notified via email and in-app alert.
- **FR-FEEDBACK-2:** The reporter can submit a 1 to 5 star rating with satisfaction comments.
- **FR-FEEDBACK-3:** If dissatisfied within a 48-hour dispute window, the reporter can request "Reopen Ticket" with justification remarks, returning the issue to the Control Desk.
- **FR-FEEDBACK-4:** After 48 hours without dispute, the system automatically transitions the status to `Closed`.

---

## 4. External Interface Requirements

### 4.1 User Interfaces
- Fully responsive HTML5 + Bootstrap 5 layout supporting mobile viewports (360px+), tablets, and desktop displays (1920px+).
- University branded palette: Navy Blue (`#0f2b48`), Teal (`#0d9488`), Neutral Gray (`#f8fafc`).
- Visual accessibility: High contrast text, ARIA attributes, semantic buttons, keyboard-navigable forms.

### 4.2 Software & API Interfaces
- **Google Identity API:** OAuth 2.0 endpoints for token exchange (`https://oauth2.googleapis.com/token`) and profile validation.
- **OpenStreetMap & Leaflet:** Public tile server integration (`https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png`).
- **QRCoder Library:** In-memory PNG QR generation.

---

## 5. Non-Functional Requirements (NFR)

### 5.1 Security Requirements
- **NFR-SEC-1:** Server-side anti-forgery verification (`ValidateAntiForgeryToken`) enforced across all mutating POST/PUT actions.
- **NFR-SEC-2:** Input sanitization to prevent Cross-Site Scripting (XSS) and SQL injection (EF Core parameterized queries).
- **NFR-SEC-3:** File upload restrictions: Allowed file extensions strictly limited to `.jpg`, `.jpeg`, `.png`, `.webp`. Maximum size: 10 MB per file. Uploaded files renamed to random GUIDs to prevent path traversal attacks.
- **NFR-SEC-4:** Strict separation of duties: Programmatic role checks preventing workers from invoking verification endpoints.

### 5.2 Performance & Scalability
- **NFR-PERF-1:** Page load response time under 1.5 seconds for all dashboard views on broadband connections.
- **NFR-PERF-2:** Database queries indexed by foreign keys, status enums, and timestamps to ensure sub-50ms query execution on tables exceeding 100,000 records.
- **NFR-PERF-3:** Background SLA calculation runs asynchronously without blocking web request worker threads.

### 5.3 Reliability, Availability & Data Integrity
- **NFR-REL-1:** Database transactions (`IDbContextTransaction`) utilized during multi-table state updates (e.g., verifying work orders and updating complaint status simultaneously).
- **NFR-REL-2:** Comprehensive audit logging (`AuditLogs` table) tracking every ticket status transition, assigning officer, timestamp, and previous/new values.

---

## 6. Data Dictionary & Entity Summary

| Entity Name | Primary Key | Foreign Keys / Associations | Purpose |
| :--- | :--- | :--- | :--- |
| `Building` | `BuildingId (int)` | None | Dynamic campus buildings & blocks |
| `Floor` | `FloorId (int)` | `BuildingId` | Floors belonging to buildings |
| `Location` | `LocationId (int)` | `FloorId` | Specific rooms, corridors, outdoor areas |
| `AllowedEmailDomain`| `DomainId (int)` | None | Configurable Google email domain whitelist |
| `IssueCategory` | `CategoryId (int)` | None | Primary fault categories (Electrical, Plumbing) |
| `SLAPolicy` | `PolicyId (int)` | `PriorityId` | Response and resolution deadline rules |
| `Department` | `DepartmentId (int)` | None | Operational maintenance divisions |
| `Team` | `TeamId (int)` | `DepartmentId`, `SupervisorUserId` | Trade teams led by Team Heads |
| `Complaint` | `ComplaintId (int)` | `LocationId`, `CategoryId`, `ReporterUserId` | Central fault ticket |
| `ComplaintImage` | `ImageId (int)` | `ComplaintId` | Attached photographs (before repair) |
| `WorkOrder` | `WorkOrderId (int)` | `ComplaintId`, `DepartmentId`, `TeamId` | Operational work package |
| `WorkOrderAssignment`|`AssignmentId (int)`| `WorkOrderId`, `WorkerUserId` | Worker-to-WorkOrder junction |
| `Verification` | `VerificationId (int)`| `WorkOrderId`, `VerifierUserId` | Independent quality inspection record |
| `VerificationImage` |`ImageId (int)` | `VerificationId` | Photographs taken during inspection |
| `ComplaintHistory` |`HistoryId (int)` | `ComplaintId`, `ActorUserId` | Immutable state transition audit trail |
| `SLAEscalation` | `EscalationId (int)` | `WorkOrderId` | Logged SLA breach and escalation events |
| `ComplaintFeedback` |`FeedbackId (int)` | `ComplaintId`, `ReporterUserId` | Post-resolution rating and comments |
| `Notification` | `NotificationId (int)`| `RecipientUserId` | In-app user notifications |
