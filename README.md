# SmartCampus — Intelligent Campus Issue Reporting, Workforce Allocation & Resolution Verification System

[![Framework](https://img.shields.io/badge/Framework-.NET%2010%20LTS-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-CC292B?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Styling](https://img.shields.io/badge/Design-Bootstrap%205%20%7C%20LPU%20UMS%20Theme-0f2b48?logo=bootstrap)](https://getbootstrap.com/)
[![Status](https://img.shields.io/badge/Architecture-Enterprise%20MVC-0d9488)]()

> **Reference Campus:** Lovely Professional University (LPU), Phagwara, Punjab, India  
> **Campus Scope:** 600+ Acres, 60+ Academic & Residential Blocks, 30,000+ Students & Staff  
> **Core Principle:** Independent Verification Gate — **`Work Completed != Resolved`**

---

## 1. Project Overview

**SmartCampus** is a centralized university facilities and infrastructure maintenance portal inspired by the aesthetic, security, and administrative rigor of university management systems like **LPU UMS (University Management System)**.

Across massive institutions like LPU (comprising Academic Blocks like 32, 34, 38, Hostel Complexes BH-1 through BH-8 and GH-1 through GH-6, UniMall, and miles of utility grids), facility issues traditionally suffer from lost complaints, lack of worker accountability, and "false resolutions" where tickets are closed without verification.

SmartCampus resolves this by introducing:
- **LPU UMS Design Aesthetic:** A clean, formal, professional institutional interface with navy blue (`#0f2b48` / `#1B365D`), crisp university white, slate grey tables, role-based navigation ribbons, and structured notification boards.
- **5-Tier Role Governance:** Student/Teacher, Campus Control Desk (Admin), Department Supervisor (Team Head), Maintenance Worker (Technician), and Super Administrator.
- **Dynamic 3-Tier Campus Hierarchy:** Building $\rightarrow$ Floor $\rightarrow$ Indoor Room / Outdoor Zone, linked with dynamic QR codes.
- **Independent Quality Verification Gate:** A worker completing a task moves it to `AwaitingVerification`. A supervisor must conduct a physical inspection and upload verification evidence before it reaches `VerifiedResolved`. A worker can **never** verify their own work.
- **Automated Multi-Tier SLA Escalation:** 50% Worker Reminder, 75% Supervisor Warning, and 100% Admin Escalation.

---

## 2. Technology Stack & Architectural Decisions

| Layer | Component | Specification | Justification |
| :--- | :--- | :--- | :--- |
| **Backend Framework** | ASP.NET Core MVC | .NET 10 LTS (`net10.0`) | High-performance, enterprise-grade, strongly typed C# 13/14 |
| **ORM & Data Layer** | Entity Framework Core 10 | Code-First Migrations, SQL Server Driver | Transactional consistency, relational integrity, migrations |
| **Database Engine** | Microsoft SQL Server | `SmartCampusDB` | Enterprise relational database with indexed queries |
| **Authentication & AuthZ**| ASP.NET Core Identity + Google OAuth 2.0 | Server-side token validation + domain whitelist | Institutional domain eligibility (e.g. `@lpu.co.in`, `@lpu.in`) |
| **Frontend UI** | HTML5, Bootstrap 5.3, Vanilla JS | LPU UMS Portal Theme (Navy Blue `#0f2b48`) | Formal, crisp institutional look without AI-generated visual clutter |
| **Mapping & GIS** | Leaflet.js + OpenStreetMap | Interactive LPU Bounds (`31.2536° N, 75.7037° E`) | Open-source, zero-cost high precision campus map |
| **Visual Analytics** | Chart.js 4.4.x | Clean operational charts | MTTR, SLA compliance, department load & hotspot heatmaps |
| **QR Code Engine** | QRCoder (.NET library) | Dynamic in-memory SVG/PNG generation | Direct scan-to-report URL pre-filling |

---

## 3. Project Directory Structure

```text
SmartCampus/
├── docs/                                  # Comprehensive Documentation Suite
│   ├── SRS.md                             # IEEE Software Requirements Specification
│   ├── USE_CASE_SPECIFICATION.md          # 10 Detailed Use Cases & Mermaid Diagrams
│   ├── DATABASE_ERD.md                    # Database ERD & 11-Module Data Dictionary
│   ├── PROJECT_REPORT.md                  # Comprehensive University Project Report
│   ├── PRESENTATION.md                    # Slide-by-Slide Defense Presentation
│   └── BUILD_STEP_BY_STEP.md              # Granular Execution Log for Every Module
├── PRD.md                                 # Product Requirements Document & Checklist
├── STATE.md                               # GSD State & Architecture Decision Records
├── CONTEXT.md                             # Technical Environment Specifications
├── progress.txt                           # Ralph Loop Iteration History
├── README.md                              # This Document
├── SmartCampus.sln                        # Visual Studio Solution File
└── src/
    └── SmartCampus.Web/                   # Main ASP.NET Core MVC Application
        ├── Areas/                         # Role-Specific Portals (LPU UMS Layouts)
        │   ├── Admin/                     # Control Desk & Triage Operations
        │   ├── Supervisor/                # Department Teams & Verification Queue
        │   ├── Worker/                    # Technician "My Tasks" & Photo Submission
        │   └── Reporter/                  # Student & Faculty Complaint Portal
        ├── Controllers/                   # Public & Account Controllers
        ├── Data/                          # DbContext, Configurations & LPU Campus Seeder
        ├── Models/                        # Entities & ViewModels
        ├── Services/                      # SLA Engine, QR Generator, Storage, Notifications
        ├── Views/                         # Shared Views, Navigation & Error Pages
        └── wwwroot/                       # Static Assets (Bootstrap, CSS, JS, Uploads)
```

---

## 4. Complete 10-Stage Complaint Lifecycle

```
[1. Submitted] ───────► [2. Under Review] ───────► [3. Approved / Rejected]
                              │
                              ▼
                      [4. Assigned (WorkOrder & SLA)]
                              │
                              ▼
                      [5. Work Accepted]
                              │
                              ▼
                      [6. In Progress]
                              │
                              ▼
                      [7. Work Completed (Photos Uploaded)]
                              │
                              ▼
                  [8. Awaiting Verification]
                     │                    │
        (Supervisor Approved)      (Supervisor Rejected)
                     │                    │
                     ▼                    ▼
             [9a. Verified Resolved]   [9b. Rework Required] ──► (Back to In Progress)
                     │
                     ▼
             [10. Closed / Feedback] ──► (Dispute within 48h ──► Reopened)
```

---

## 5. Getting Started & Installation

### Prerequisites
- **Operating System:** Windows 10 / 11 / Windows Server
- **SDK:** [.NET 10 LTS SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Verified `10.0.302`)
- **IDE:** Visual Studio 2022 (v17.12+) or Visual Studio Code
- **Database:** Microsoft SQL Server (LocalDB, SQL Express, or Developer Edition)

### Setup Instructions
1. **Clone the repository:**
   ```powershell
   git clone https://github.com/YourOrg/SmartCampus.git
   cd SmartCampus
   ```

2. **Restore dependencies:**
   ```powershell
   dotnet restore
   ```

3. **Configure Database Connection:**
   Update `src/SmartCampus.Web/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartCampusDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```

4. **Apply Migrations and Seed LPU Campus Data:**
   ```powershell
   cd src/SmartCampus.Web
   dotnet ef database update
   ```

5. **Run the Application:**
   ```powershell
   dotnet run
   ```
   Navigate to `https://localhost:5001` or `http://localhost:5000` in your web browser.

---

## 6. Default Test Accounts & Roles

| Role | Username / Email | Default Password | Primary Dashboard |
| :--- | :--- | :--- | :--- |
| **Super Administrator** | `superadmin@smartcampus.lpu.in` | `Admin@12345` | `/Admin/Dashboard` |
| **Control Desk Admin** | `controldesk@smartcampus.lpu.in` | `Admin@12345` | `/Admin/Triage` |
| **Electrical Supervisor**| `elec.head@smartcampus.lpu.in` | `Super@12345` | `/Supervisor/Verification` |
| **Maintenance Worker** | `technician.rajesh@smartcampus.lpu.in` | `Worker@12345` | `/Worker/MyTasks` |
| **Student / Teacher** | `student.test@lpu.in` | `Student@12345`| `/Reporter/Dashboard` |

---

## 7. Documentation Suite References

- [Software Requirements Specification (SRS)](file:///c:/Users/hp/Desktop/SmartCampus/docs/SRS.md)
- [Use Case Specifications & Workflows](file:///c:/Users/hp/Desktop/SmartCampus/docs/USE_CASE_SPECIFICATION.md)
- [Database ERD & Data Dictionary](file:///c:/Users/hp/Desktop/SmartCampus/docs/DATABASE_ERD.md)
- [Comprehensive Project Report](file:///c:/Users/hp/Desktop/SmartCampus/docs/PROJECT_REPORT.md)
- [Project Presentation Deck](file:///c:/Users/hp/Desktop/SmartCampus/docs/PRESENTATION.md)
- [Granular Step-by-Step Build Log](file:///c:/Users/hp/Desktop/SmartCampus/docs/BUILD_STEP_BY_STEP.md)

---
*SmartCampus is an academic and enterprise engineering project engineered for university infrastructure management.*
