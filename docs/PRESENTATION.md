# SmartCampus: Project Presentation Deck & Speaker Defense Script

**Presentation Title:** SmartCampus: Intelligent Campus Issue Reporting, Workforce Allocation & Resolution Verification System  
**Reference Institution:** Lovely Professional University (LPU), Punjab, India  
**Target Audience:** University Leadership, Directorate of Facilities & IT Governance Committee  
**Technology Stack:** ASP.NET Core MVC (.NET 10 LTS), EF Core 10, SQL Server, Bootstrap 5 (LPU UMS Theme)  

---

## Slide 1: Title & Project Identity

```
╔════════════════════════════════════════════════════════════════════════════════╗
║                                 SMARTCAMPUS                                    ║
║  Intelligent Campus Issue Reporting, Workforce Allocation & Resolution System  ║
║                                                                                ║
║                  Reference Campus: Lovely Professional University               ║
║                        Enterprise .NET 10 LTS Architecture                      ║
╚════════════════════════════════════════════════════════════════════════════════╝
```

### Speaker Notes
> "Respected committee members, welcome to our presentation on **SmartCampus**. SmartCampus is an enterprise-grade campus maintenance and workforce governance platform engineered specifically for large-scale academic institutions, modeled around the reference campus of Lovely Professional University. Today, we will demonstrate how this system transforms campus facility management from a fragmented, manual process into an auditable, automated, and quality-verified digital workflow."

---

## Slide 2: The Institutional Challenge (The LPU Context)

### Key Metrics
- **Campus Scale:** 600+ contiguous acres in Phagwara, Punjab.
- **Physical Plant:** 60+ multi-story academic blocks (Block 32, 34, 38), 14+ residential hostels (BH-1 to 8, GH-1 to 6), central library, UniMall, and outdoor utilities.
- **Campus Population:** 30,000+ students and over 5,000 staff members.

### The Operational Pain Points
1. **Lost & Untracked Complaints:** Issues reported via WhatsApp, phone calls, or physical registers lack centralized tracking.
2. **The "False Resolution" Trap:** Workers mark complaints as "fixed" without physical inspection, leading to recurring breakdowns.
3. **Uneven Workforce Allocation:** Certain maintenance technicians are overloaded while others remain idle.
4. **SLA Neglect:** Critical safety hazards (e.g., exposed wiring, water leakages in labs) linger without automated escalation.

### Speaker Notes
> "Operating an estate the size of LPU is equivalent to managing a small smart city. When a street lamp fails near Block 32 or an AC fails in a server lab, traditional reporting channels break down. The single largest issue faced by campus control desks is the 'False Resolution Trap'—where a technician marks an issue complete, but the student discovers it is still broken. SmartCampus was created to solve this fundamental accountability problem."

---

## Slide 3: Core Architectural Differentiator

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           THE CARDINAL PRINCIPLE                            │
│                                                                             │
│                   WORK COMPLETED  ≠  VERIFIED RESOLVED                      │
│                                                                             │
│   Worker finishes task ──► "Awaiting Verification" ──► Independent Head     │
│                                                          inspects & signs off│
└─────────────────────────────────────────────────────────────────────────────┘
```

### Key Pillars
- **Strict Role Separation:** A technician can **never** verify their own work.
- **Evidence-Based Handover:** Mandatory side-by-side photographic inspection (Original Problem Photo vs Technician Completion Photo vs Supervisor Sign-off).
- **Rework Loop:** Flawed repairs are formally rejected with defect notes and returned to the field technician.

### Speaker Notes
> "Our core architectural breakthrough is simple yet transformative: In SmartCampus, 'Work Completed' does not mean 'Resolved'. When an electrician finishes a repair, the ticket enters 'Awaiting Verification'. An independent Department Supervisor must physically inspect the site, upload photographic proof, and formally sign off. If the work is substandard, it is rejected back to the technician for rework."

---

## Slide 4: Authentic LPU UMS Portal Aesthetic

### Design Standards
- **Zero "AI-Template" Clutter:** No purple gradients, floating neon bubbles, or generic SaaS marketing elements.
- **LPU UMS Color Palette:** Classic deep navy blue (`#0f2b48` / `#1B365D`), crisp university white (`#ffffff`), and slate gray structured borders (`#e2e8f0`).
- **Institutional Context:**
  - University crest and formal portal header.
  - Institutional profile badge displaying Full Name, Role, and Student/Staff Registration Number.
  - High-density, readable tabular records with crisp Bootstrap 5 status badges.
  - Breadcrumb navigation ribbons ensuring clear operational context.

### Speaker Notes
> "We intentionally rejected generic or AI-styled templates. Instead, we designed SmartCampus to seamlessly reflect the formal, authoritative look and feel of LPU UMS. From the navy blue top bar to the registration number badges and structured tables, any student, professor, or maintenance supervisor will feel instantly at home."

---

## Slide 5: The 5-Tier Role Governance Matrix

| Role | Operational Scope | Key Functionality |
| :--- | :--- | :--- |
| **1. Student / Teacher** | Personal Submissions | File issue with photos, track timeline, rate service |
| **2. Campus Control Desk** | Campus-Wide Queue | Triage tickets, detect duplicates, assign priority & SLA |
| **3. Department Supervisor** | Department Scope | Allocate workers, inspect completed jobs, approve/reject |
| **4. Maintenance Worker** | Assigned Tasks Only | Accept assignments, update progress, upload after-photos |
| **5. Super Administrator** | Global Governance | Dynamic buildings, SLA policies, user approvals, analytics |

### Speaker Notes
> "Security and governance are maintained through five tightly scoped roles. Self-registration grants only the Student/Teacher role. Elevating accounts to technicians, supervisors, or administrators requires explicit administrative authorization. Strict data boundaries ensure workers see only their own tasks, while supervisors oversee their trade departments."

---

## Slide 6: Google OAuth 2.0 & Institutional Whitelist

```
[User clicks "Continue with Google"]
               │
               ▼
   [Server validates ID Token & email_verified == true]
               │
               ▼
   [Server checks domain against AllowedEmailDomains table]
     ├── Domain NOT whitelisted ──► Terminate with warning
     └── Domain permitted (@lpu.co.in / @lpu.in)
               │
               ▼
   [Establish ASP.NET Identity Session ──► Role Dashboard]
```

### Speaker Notes
> "SmartCampus eliminates insecure password storage by leveraging Google OAuth 2.0 Single Sign-On. Crucially, the backend verifies tokens on the server and checks the email against our dynamic `AllowedEmailDomains` table. This allows university administrators to restrict access to `@lpu.co.in` or `@lpu.in` domains, while permitting configurable test domains during pilot stages."

---

## Slide 7: Dynamic Spatial Hierarchy & Dynamic QR Codes

```
LPU Campus (600 Acres)
    ├── Academic Blocks (Block 32, 34, 38...)
    │       └── Floors (Basement, Ground, 1st..8th)
    │               └── Rooms (Classroom 32-401, AI Lab 3, Restrooms) ──► QR Code
    ├── Residential Hostels (BH-1..8, GH-1..6)
    └── Outdoor Sectors (Perimeter Roads, Green Zones, Parking) ──► Pole QR Code
```

### Key Technical Feature
- **Zero Hardcoded Buildings:** All buildings, floors, and rooms are database-driven, allowing portability to any other campus.
- **Dynamic QR Code Engine:** Uses `QRCoder` to encode unique location GUIDs. A student scanning a doorplate QR code is taken directly to an auto-filled report form.

### Speaker Notes
> "With over 60 buildings, hardcoding campus geography is impossible. SmartCampus models the campus as a 3-tier dynamic tree: Building to Floor to Room or Outdoor Zone. Furthermore, every room or utility pole has a unique cryptographic QR code. Scanning the QR code instantly locks in the building and room, eliminating user reporting errors."

---

## Slide 8: The 10-Stage Complaint Lifecycle

```
1. Submitted ────────► 2. Under Review ────────► 3. Approved / Rejected
                            │
                            ▼
                    4. Assigned (WorkOrder & SLA)
                            │
                            ▼
                    5. Work Accepted
                            │
                            ▼
                    6. In Progress
                            │
                            ▼
                    7. Work Completed (Photos Uploaded)
                            │
                            ▼
                8. Awaiting Verification
                   ├── Supervisor Approves ──► 9a. Verified Resolved ──► 10. Closed
                   └── Supervisor Rejects  ──► 9b. Rework Required ──► Back to In Progress
```

### Speaker Notes
> "Here you see the complete 10-stage lifecycle. Notice that after Stage 7 (Work Completed), the ticket does not close. Instead, it enters Stage 8: Awaiting Verification. Only when the Department Supervisor conducts a physical inspection and approves it does it reach Verified Resolved. If rejected, it enters the Rework Required loop with defect notes."

---

## Slide 9: Workforce Allocation & Workload Balancing

### Intelligent Allocation Features
- **Trade Matching:** Complaint category (Electrical, Plumbing, HVAC, Civil, IT) automatically filters the target department.
- **Multi-Trade Work Orders:** Complex incidents automatically spawn child work orders across multiple departments.
- **Real-Time Workload Meters:**
  - `Available` (0–1 active tasks) — Green
  - `Busy` (2–3 active tasks) — Yellow
  - `Overloaded` (>3 active tasks) — Red
  - `On Leave` — Muted

### Speaker Notes
> "Supervisors no longer assign work blindly. When an approved ticket arrives, the system displays the technician roster with their verified skills and live workload meters. This prevents overloading senior technicians while junior workers sit idle, optimizing campus response times."

---

## Slide 10: Multi-Tier SLA & Automated Escalation Engine

| Priority | Target Resolution | Tier 1 (50% Elapsed) | Tier 2 (75% Elapsed) | Tier 3 (100% Breached) |
| :--- | :--- | :--- | :--- | :--- |
| **Critical** | 1 Hour | 30 min Technician Reminder | 45 min Supervisor Warning | 60 min Admin Escalation |
| **High** | 6 Hours | 3 hour Reminder | 4.5 hour Warning | 6 hour Escalation |
| **Medium** | 24 Hours | 12 hour Reminder | 18 hour Warning | 24 hour Escalation |
| **Low** | 72 Hours | 36 hour Reminder | 54 hour Warning | 72 hour Escalation |

### Engine Mechanics
- Autonomous background service (`SLAEngineHostedService`) checks active tickets every 60 seconds.
- Automatically sets `IsOverdue = true` and logs auditable breach records in `SLAEscalations`.

### Speaker Notes
> "To guarantee that maintenance requests are never forgotten, SmartCampus incorporates an automated SLA engine. Running as a continuous background service, it calculates elapsed time against resolution targets. At 50%, the worker is reminded; at 75%, the supervisor receives a warning; at 100%, the ticket breaches, alerting the central campus control desk."

---

## Slide 11: Enterprise Database Architecture (`SmartCampusDB`)

### 11 Relational Modules
1. **Identity & Auth:** `AspNetUsers`, `UserProfiles`, `AllowedEmailDomains`
2. **Campus Topology:** `Buildings`, `Floors`, `Locations`
3. **Issue Catalog & SLA:** `IssueCategories`, `Priorities`, `SLAPolicies`
4. **Workforce Structure:** `Departments`, `Teams`, `TeamMembers`, `Skills`
5. **Complaints:** `Complaints`, `ComplaintImages`
6. **Work Orders:** `WorkOrders`, `WorkOrderAssignments`, `WorkOrderEvidence`
7. **Verification Gate:** `Verifications`, `VerificationImages`
8. **SLA Monitoring:** `SLAEscalations`
9. **Feedback & Quality:** `ComplaintFeedbacks`
10. **System Notifications:** `Notifications`
11. **System Audit Trail:** `ComplaintHistory`, `AuditLogs`

### Speaker Notes
> "Our database architecture in SQL Server is partitioned into 11 distinct relational modules. Every status change, worker assignment, and verification inspection is backed by immutable audit history and composite database indexes, ensuring sub-50ms performance even under heavy university concurrency."

---

## Slide 12: GIS Mapping & Operational Analytics

### Geospatial Visualization (Leaflet.js)
- Interactive campus map centered on LPU coordinates (`31.2536° N, 75.7037° E`).
- Color-coded urgency pins (Red = Critical, Orange = High, Yellow = Medium, Green = Resolved).
- Building-level issue aggregation and hotspot heatmaps.

### Executive Dashboards (Chart.js)
- Mean Time to Resolve (MTTR) trends.
- Department-wise SLA compliance percentages.
- Verification rejection/rework rates per trade team.

### Speaker Notes
> "Campus leadership requires high-level operational intelligence. Our Leaflet map provides an instant spatial overview of where faults are clustered across LPU's 600 acres. Simultaneously, our executive Chart.js dashboards monitor department response velocity, SLA compliance rates, and technician performance."

---

## Slide 13: Staged Campus Deployment Strategy

```
Stage 1: Prototype (Weeks 1–4)
  ├── 3–5 Core Buildings (e.g. Block 32, Block 34, Central Library)
  └── Test 5 roles, work orders & independent verification gate

Stage 2: Limited Campus Pilot (Weeks 5–8)
  ├── 10–15 Blocks (Academic Blocks + BH-1, GH-1 Hostels)
  └── Real student reporting, live maintenance dispatch & feedback

Stage 3: Full University Rollout (Weeks 9–12)
  ├── 60+ Blocks across all 600 acres
  └── Full integration with LPU estate management divisions
```

### Speaker Notes
> "We advocate a disciplined, phased rollout. We begin with a focused 3-to-5 building prototype covering Block 32 and Central Library, advance to a 15-building pilot including hostels, and finally transition to a campus-wide rollout across all 60+ blocks."

---

## Slide 14: Conclusion & Project Summary

### Summary of Accomplishments
- Delivered an enterprise-grade ASP.NET Core MVC (.NET 10 LTS) platform.
- Implemented the authentic **LPU UMS** institutional design language.
- Enforced the **Independent Verification Gate** (`Work Completed != Resolved`).
- Automated workforce allocation, SLA tracking, and dynamic QR location mapping.
- Supported by complete documentation: SRS, Use Cases, ERD, Report, and Build Logs.

### Speaker Notes
> "In conclusion, SmartCampus bridges the gap between infrastructure maintenance and accountability. By replacing informal, unverifiable reporting with an intelligent, role-governed platform, SmartCampus ensures that Lovely Professional University operates with the highest standards of safety, transparency, and operational excellence. Thank you, and we welcome your questions."
