# SmartCampus — Technical Context & Conventions

**Project Directory:** `c:\Users\hp\Desktop\SmartCampus`  
**Host Framework:** .NET 10 LTS (`10.0.302 SDK`, Runtime `10.0.10`)  
**Target OS:** Windows 10 / 11  
**Target IDE:** Visual Studio 2022 / VS Code with C# Dev Kit  

---

## 1. Technological Environment

### 1.1 Verified Installed Tooling
- **.NET SDK:** `10.0.302` installed at `C:\Program Files\dotnet\sdk\10.0.302`
- **Shared Runtimes:**
  - `Microsoft.AspNetCore.App 10.0.10`
  - `Microsoft.NETCore.App 10.0.10`
  - `Microsoft.WindowsDesktop.App 10.0.10`
- **Database Engine:** Microsoft SQL Server (LocalDB / Express / Developer)
- **Target Database Name:** `SmartCampusDB`
- **ORM:** Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`)

### 1.2 Frontend Asset Stack
- **Styling Framework:** Bootstrap 5.3 + Bootstrap Icons
- **Mapping:** Leaflet.js 1.9.4 + OpenStreetMap tiles
- **Analytics:** Chart.js 4.4.x
- **Palette Scheme:**
  - Primary Dark / Brand: Navy Blue (`#0f2b48`)
  - Accent / Verification: Teal (`#0d9488`)
  - Background Neutral: Light Gray (`#f8fafc`)
  - Card Neutral: Pure White (`#ffffff`)
  - Status Warning / Alert: Amber (`#f59e0b`)
  - Status Danger / Escalation: Crimson (`#ef4444`)
  - Status Success / Resolved: Emerald (`#10b981`)

---

## 2. Directory Layout Architecture

Once scaffolded in Phase 2, the project layout will adhere to a clean ASP.NET Core structure:

```text
SmartCampus/
├── docs/                             # Architectural Specifications, SRS, ERD
│   ├── SRS.md                        # Software Requirements Specification
│   ├── USE_CASE_SPECIFICATION.md     # Use Case Details & Mermaid Diagrams
│   └── DATABASE_ERD.md               # Database ER Diagram & Table Specs
├── PRD.md                            # Product Requirements Document
├── STATE.md                          # GSD Framework State Tracker
├── CONTEXT.md                        # Technical Context & Guidelines
├── progress.txt                      # Ralph Loop Iteration History
├── SmartCampus.sln                   # Visual Studio Solution File
└── src/
    └── SmartCampus.Web/              # ASP.NET Core MVC Application (.NET 10)
        ├── Areas/                    # Role-specific MVC Areas
        │   ├── Admin/                # Control Desk & Super Admin Area
        │   ├── Supervisor/           # Department Supervisor Area
        │   ├── Worker/               # Maintenance Technician Area
        │   └── Reporter/             # Student & Teacher Area
        ├── Controllers/              # Shared Controllers (Home, Account, LocationApi)
        ├── Data/                     # ApplicationDbContext, Model Configurations & Seeders
        ├── Models/                   # Domain Entities & ViewModels
        │   ├── Entities/             # Campus, Complaint, WorkOrder, Verification, SLA
        │   └── ViewModels/           # Strongly-typed Views & DTOs
        ├── Services/                 # Business Logic, SLA Engine, Email, QR Services
        ├── Views/                    # Shared Views, Layouts, Navigation
        └── wwwroot/                  # Static assets (Bootstrap, Leaflet, Chart.js, Uploads)
```

---

## 3. Coding & Security Conventions

1. **Nullability & C# 13/14 Standards:** Enable `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.
2. **Entity Design:**
   - All primary keys: `int` or `Guid` with identity column generation.
   - Timestamps: `DateTime.UtcNow` stored in UTC; formatted to IST (`Asia/Kolkata` - UTC+5:30) in ViewModels.
   - Statuses: Strictly typed Enums (`ComplaintStatus`, `PriorityLevel`, `WorkOrderStatus`).
3. **Security Standards:**
   - Server-side Anti-Forgery Tokens (`[ValidateAntiForgeryToken]`) on all POST/PUT/DELETE forms.
   - MIME and file signature checking on all uploaded evidence (whitelist: `image/jpeg`, `image/png`, `image/webp`).
   - File uploads capped at 10 MB per image; stripped of malicious payloads.
   - Workers cannot approve their own tasks under any circumstances.
