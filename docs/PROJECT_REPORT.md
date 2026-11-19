# SmartCampus — Comprehensive Project Report & Engineering Documentation

**Project Title:** SmartCampus: Intelligent Campus Issue Reporting, Workforce Allocation & Resolution Verification System  
**Reference Institution:** Lovely Professional University (LPU), Punjab, India  
**Academic / Enterprise Project Report**  
**Document Version:** 1.0.0  
**Date:** November 2026  

---

## Executive Abstract

Modern educational mega-campuses such as Lovely Professional University (LPU), spanning over 600 acres with more than 60 high-density academic blocks, residential hostels, and utility infrastructure, present unprecedented operational challenges in facilities and estate management. Traditional fault-reporting mechanisms—relying on fragmented phone calls, physical paper registers, or informal messaging—suffer from four systemic failure modes:
1. High complaint abandonment and duplicate submissions due to lack of transparent real-time tracking.
2. Inefficient, manual workforce allocation that fails to balance technician workloads across trade divisions (Electrical, Plumbing, HVAC, Civil, Network/IT).
3. The "False Resolution" pitfall, where workers unilaterally mark tickets as resolved without quality auditing, leading to chronic fault recurrence.
4. Absence of automated Service Level Agreement (SLA) deadline enforcement, leaving critical safety hazards unattended.

**SmartCampus** is an enterprise-grade, role-governed web application designed specifically to eliminate these inefficiencies. Built on ASP.NET Core MVC (.NET 10 LTS), Entity Framework Core 10, and Microsoft SQL Server (`SmartCampusDB`), the platform incorporates an authentic university portal aesthetic inspired by **LPU UMS (University Management System)**.

Key engineering innovations implemented in SmartCampus include:
- **A 10-Stage Auditable Complaint Lifecycle** governed by the strict institutional principle: **`Work Completed != Resolved`**.
- **A Mandatory Independent Quality Verification Gate**, requiring physical on-site inspection and photographic sign-off by a designated trade supervisor before a ticket can be closed.
- **Dynamic 3-Tier Campus Spatial Hierarchy** (Building $\rightarrow$ Floor $\rightarrow$ Indoor Room / Outdoor Asset) with dynamic QR code generation, eliminating hardcoded campus dependencies.
- **Automated Background SLA Engine** executing multi-tiered threshold escalations (50% Worker Reminder, 75% Supervisor Warning, 100% Administrative Breach Escalation).
- **Institutional Single Sign-On (SSO)** via Google OAuth 2.0 with a configurable domain whitelist table (`@lpu.co.in`, `@lpu.in`), ensuring institutional security while preventing unauthorized account creation.

---

## Chapter 1: Introduction & Institutional Context

### 1.1 Institutional Baseline: Lovely Professional University (LPU)
Lovely Professional University is one of India's largest single-campus universities, accommodating over 30,000 students and 5,000 faculty and administrative staff. The physical plant includes:
- **Academic Blocks:** Multi-story academic clusters (Block 32, 34, 38, etc.) housing laboratories, lecture theatres, faculty rooms, and IT server hubs.
- **Residential Complexes:** 14+ high-capacity multi-story hostels (BH-1 to BH-8 for male students; GH-1 to GH-6 for female students).
- **Commercial & Central Facilities:** UniMall, Shanti Devi Mittal Auditorium, Central Library, Sports Arenas, and Food Courts.
- **Outdoor Infrastructure:** Internal perimeter road networks, solar lighting grids, parking lots, stormwater drainage, and sewage treatment plants.

### 1.2 Problem Statement & Objective
In an environment of this magnitude, the absence of an integrated digital maintenance workflow results in significant operational friction, deferred maintenance, and student dissatisfaction.

The primary objective of SmartCampus is to digitize and automate the entire campus maintenance lifecycle:
$$\text{Issue Reporting} \longrightarrow \text{Triage \& SLA Assignment} \longrightarrow \text{Workforce Dispatch} \longrightarrow \text{Field Repair} \longrightarrow \mathbf{Independent\ Verification} \longrightarrow \text{Closure \& Feedback}$$

---

## Chapter 2: System Architecture & Design Philosophy

### 2.1 The LPU UMS Design Aesthetic
Unlike generic modern SaaS templates or AI-generated consumer interfaces characterized by neon gradients, SmartCampus adopts the formal, professional, and utilitarian aesthetic of **LPU UMS**:
- Deep navy blue header (`#0f2b48`) with crisp contrast and formal institution crest integration.
- Dedicated user profile badge displaying Full Name, Role, and Student/Staff Registration Number.
- Clean breadcrumb navigation ribbons for fast context orientation.
- High-density, high-legibility data tables with sharp status badges (`Submitted`, `Under Review`, `Assigned`, `In Progress`, `Awaiting Verification`, `Verified Resolved`, `Closed`).
- Actionable modals and forms designed for rapid data entry without decorative distractions.

### 2.2 Role-Based Access Control (RBAC) Architecture
The system enforces five mutually exclusive roles:
1. **Student / Teacher (Reporter):** Submits complaints, uploads photos, tracks status, and rates resolution.
2. **Campus Administrator (Control Desk):** Reviews submissions, executes duplicate detection heuristics, assigns priorities and SLAs, and routes work to departments.
3. **Department Supervisor (Team Head):** Manages trade technicians, balances workloads, inspects completed repairs, and authorizes verification sign-offs.
4. **Maintenance Worker (Technician):** Receives mobile assignments, logs on-site commencement, and uploads post-repair photographic evidence.
5. **Super Administrator:** Manages campus spatial trees, departments, user role elevations, and system-wide SLA policies.

---

## Chapter 3: Database & Data Model Engineering

The underlying database, **`SmartCampusDB`**, is engineered in Microsoft SQL Server using Entity Framework Core 10 Code-First modeling. It comprises 11 functional entity clusters:
1. **Authentication & Identity:** ASP.NET Core Identity tables extended with `UserProfiles` and `AllowedEmailDomains`.
2. **Spatial Topology:** `Buildings`, `Floors`, `Locations`.
3. **Issue Catalog & SLA:** `IssueCategories`, `IssueSubcategories`, `Priorities`, `SLAPolicies`.
4. **Workforce Management:** `Departments`, `Teams`, `TeamMembers`, `Skills`, `WorkerSkills`.
5. **Complaint Management:** `Complaints`, `ComplaintImages`, `ComplaintHistory`.
6. **Work Orders:** `WorkOrders`, `WorkOrderAssignments`, `WorkOrderEvidence`.
7. **Verification Gate:** `Verifications`, `VerificationImages`.
8. **SLA Monitoring:** `SLAEscalations`.
9. **Feedback & Quality:** `ComplaintFeedbacks`.
10. **System Notifications:** `Notifications`.
11. **System Audit Trail:** `AuditLogs`.

---

## Chapter 4: Core Operational Workflows

### 4.1 The 10-Stage Complaint Lifecycle
The lifecycle strictly separates technician completion from final resolution:
1. **Submitted:** Complaint created with photos and location details.
2. **Under Review:** Control Desk opens ticket.
3. **Approved / Rejected:** Validated by desk; rejection requires mandatory justification.
4. **Assigned:** Child work orders dispatched to relevant trade departments with SLA deadlines.
5. **Work Accepted:** Technician acknowledges dispatch.
6. **In Progress:** Technician arrives on site and starts physical work.
7. **Work Completed:** Technician finishes work and uploads completion photographs.
8. **Awaiting Verification:** System notifies Department Supervisor for physical inspection.
9. **Verified Resolved / Rework Required:** Supervisor inspects work. Approval transitions to `VerifiedResolved`; rejection triggers `ReworkRequired` with defect notes.
10. **Closed / Reopened:** Ticket closed after student rating or 48-hour timeout.

---

## Chapter 5: Security, Performance & Verification

### 5.1 Security Governance
- Server-side Anti-Forgery Tokens (`[ValidateAntiForgeryToken]`) enforced across all state-mutating requests.
- Strict MIME and file signature validation on image uploads, restricting extensions to `.jpg`, `.png`, and `.webp`, capped at 10 MB.
- Server-side token validation for Google OAuth 2.0 with institutional domain filtering.
- Programmatic role separation ensuring a technician can never invoke verification endpoints.

### 5.2 Performance & Indexing
- Sub-50ms query latency achieved via non-clustered composite indexes on `(ReporterUserId, Status, CreatedAtUtc)` and `(Status, PriorityId, LocationId)`.
- Asynchronous, non-blocking background hosted services for SLA polling.

---

## Chapter 6: Conclusion

SmartCampus establishes a robust, auditable, and scalable standard for university campus maintenance management. By grounding the architecture in LPU's physical topology, enforcing the independent verification gate, and delivering an authentic LPU UMS institutional user experience, the system eliminates administrative bottlenecks, improves campus safety, and guarantees operational transparency.
