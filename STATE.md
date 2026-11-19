# SmartCampus Project State (GSD State Tracker)

**Current Milestone:** All 15 Phases Complete — Production Ready & Shipped to GitHub  
**Phase Status:** SHIPPED (Source Code & Deliverables Synced to GitHub)  
**Last Updated:** 2026-11-19  
**GitHub Repository:** [https://github.com/DipanshuDhaka/smart_Campus](https://github.com/DipanshuDhaka/smart_Campus)  
**Active Framework:** Get Shit Done (GSD) + Ralph Loop Task Processor  

---

## 1. Milestone Tracking

| Phase | Description | Status | Evidence / Deliverables |
| :--- | :--- | :--- | :--- |
| **Phase 1** | **Planning & Requirements** | **COMPLETE** | `PRD.md`, `docs/SRS.md`, `docs/USE_CASE_SPECIFICATION.md`, `docs/DATABASE_ERD.md`, `docs/PROJECT_REPORT.md`, `docs/PRESENTATION.md` |
| **Phase 2** | **Environment & Project Setup** | **COMPLETE** | `SmartCampus.sln`, `SmartCampus.Web` (.NET 10 LTS), EF Core 10, Identity, `.gitignore` |
| **Phase 3** | **Database Development & Migrations**| **COMPLETE** | 11 relational modules, `ApplicationDbContext`, Fluent API indexes, `DbInitializer` with LPU topology & pilot complaints |
| **Phase 4** | **Google Auth & RBAC** | **COMPLETE** | Google OAuth 2.0, `AllowedEmailDomains`, LPU UMS layout & 4 role dashboards |
| **Phase 5** | **Campus & Location Management** | **COMPLETE** | 60+ Buildings CRUD, Floors, Rooms, QRCoder SVG/PNG engine, Leaflet GIS map |
| **Phase 6** | **Student/Teacher Complaint Portal** | **COMPLETE** | Dynamic cascading & QR auto-fill, multi-photo upload, 10-stage tracker, rating & dispute |
| **Phase 7** | **Campus Control Desk** | **COMPLETE** | Triage queue, duplicate detection heuristic, SLA deadline calculation, rejection workflow |
| **Phase 8** | **Department & Workforce Management**| **COMPLETE** | 7 Maintenance departments, trade teams, technician rosters, availability toggles |
| **Phase 9** | **Work Orders & Manpower Allocation** | **COMPLETE** | Multi-worker dispatch, mobile task portal, acceptance, and on-site repair tracking |
| **Phase 10**| **Independent Verification Gate** | **COMPLETE** | Side-by-side photo comparison, supervisor inspection sign-off, rework loop, self-approval prevention |
| **Phase 11**| **SLA Monitoring & Escalation Engine** | **COMPLETE** | `SLAEngineHostedService`, 50%/75%/100% breach notifications, SLA monitor dashboard |
| **Phase 12**| **Interactive Campus Map & QR Scanner**| **COMPLETE** | Leaflet.js LPU map, building markers, QRCoder dynamic doorplates, Web Camera QR Scanner |
| **Phase 13** | **Operations Analytics & Notifications** | **COMPLETE** | Chart.js visual dashboards (Categories, Workload, Top 5 Hotspots), MTTR, SLA compliance %, Notification center |
| **Phase 14** | **Advanced Optimization & Real-Time** | **COMPLETE** | Smart Department Recommender, Workload Auto-Balancing algorithm, SignalR live hub & toast alerts |
| **Phase 15** | **Hardening, Pilot Data & Final Delivery** | **COMPLETE** | Seeded pilot lifecycle data, Release build 0 errors, security audit (CSRF, upload sandbox, SQLi), project reports |

---

## 2. Architectural Decision Records (ADRs)

- **ADR-001 (Monolithic .NET 10 MVC with Modular Areas):** Rather than fragmenting into distributed microservices or separate client/server SPAs that complicate local deployment, SmartCampus uses ASP.NET Core MVC (.NET 10 LTS) with MVC Areas (`Admin`, `Supervisor`, `Worker`, `Reporter`) and Web API endpoints for dynamic map/charts.
- **ADR-002 (Relational Core in Microsoft SQL Server via EF Core 10):** Strict transactional integrity is non-negotiable for complaint lifecycle state transitions, work orders, and independent verifications. EF Core Code-First with explicit foreign key constraints and audit tables guarantees consistency.
- **ADR-003 (Independent Verification Gate):** A worker's transition of a task to `WorkCompleted` does not resolve the complaint. The state machine transitions to `AwaitingVerification`, requiring a Team Head to inspect physical evidence and either grant `VerifiedResolved` or mandate `ReworkRequired`. Workers cannot verify their own work.
- **ADR-004 (Google OAuth 2.0 with Dynamic Domain Filter):** Authentication relies on Google OAuth with server-side identity verification. Permitted domains are stored in `AllowedEmailDomains` table, allowing university admins to toggle between open testing and institutional `@lpu.co.in` restrictions without code changes.
- **ADR-005 (File Storage Strategy):** High-resolution complaint and verification photos are stored in dedicated hashed directory structures under `wwwroot/uploads/` with strict MIME validation, randomized GUID filenames, and file size caps. Database tables store secure relative URI metadata, not binary BLOBs.
- **ADR-006 (Intelligent Department & Workforce Recommender):** Diagnostic issue keywords in title and description are tokenized and scored against trade domain vocabularies (ELEC, PLUMB, CIVIL, HVAC, ITNET, HOUSE, HORT) to provide confidence scores and crew recommendations without external API dependencies.
- **ADR-007 (Real-Time Communication via ASP.NET Core SignalR):** Real-time notifications and SLA escalations are pushed instantly to active role groups and targeted user connections via `/hubs/notifications`.

---

## 3. Project Status Summary

The SmartCampus enterprise platform for Lovely Professional University is fully built, hardened, and verified with 0 compilation errors across both Debug and Release configurations. All 15 development phases outlined in the engineering specifications are complete and documented.

---

## 4. Export Deliverables (PPTX, DOCX & PDF)

| Format | File Path | Description |
| :--- | :--- | :--- |
| **PowerPoint (.pptx)** | [`docs/SmartCampus_Presentation.pptx`](file:///c:/Users/hp/Desktop/SmartCampus/docs/SmartCampus_Presentation.pptx) | 15-slide human-made capstone presentation deck formatted in LPU UMS navy & gold with student group details (Dipanshu, Vishnu, Harsh), mentor Aryan Chauhan (ID: 35200), syllabus units 1–6 with use cases & code artifacts, challenges tackled, and live workflow demo screenshots. |
| **Word Document (.docx)** | [`docs/SmartCampus_Project_Report.docx`](file:///c:/Users/hp/Desktop/SmartCampus/docs/SmartCampus_Project_Report.docx) | Comprehensive 30+ page university capstone project report formatted in official LPU academic style with 3-student slots, 13 screenshots, and 7 chapters. |
| **PDF Project Report (.pdf)** | [`docs/SmartCampus_Project_Report.pdf`](file:///c:/Users/hp/Desktop/SmartCampus/docs/SmartCampus_Project_Report.pdf) | Exact 32-page publication-grade A4 capstone engineering report with running headers, footers, dynamic page numbers, 13 screenshots, and data tables. |
| **PDF Presentation Deck (.pdf)** | [`docs/SmartCampus_Presentation.pdf`](file:///c:/Users/hp/Desktop/SmartCampus/docs/SmartCampus_Presentation.pdf) | Print- and share-ready 16:9 landscape presentation slide deck in PDF format matching the 15-slide PPTX. |

