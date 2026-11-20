# SmartCampus: Project Presentation Deck & Academic Defense Guide

**Presentation Title:** SmartCampus: Intelligent Campus Issue Reporting, Workforce Allocation & Resolution Verification System  
**Institution:** Lovely Professional University (LPU), Phagwara, Punjab, India  
**Degree & Department:** B.Tech. in Computer Science & Engineering  
**Academic Session:** 2025 – 2026  
**Technology Stack:** ASP.NET Core MVC (.NET 10 LTS), EF Core 10, SQL Server, Bootstrap 5 (LPU UMS Theme)  
**Official GitHub Repository:** [https://github.com/DipanshuDhaka/smart_Campus](https://github.com/DipanshuDhaka/smart_Campus)

---

### Project Personnel & Evaluation Metadata

| Role | Details |
| :--- | :--- |
| **Submitted To (Project Evaluator):** | **Mr. Aryan Chauhan** (Teacher ID: 35200), Department of CSE, Lovely Professional University |
| **Submitted By (Group Project):** | **Dipanshu** (Reg No: 12323250) — *Backend, Database & Security*<br/>**Vishnu** (Reg No: 12307182) — *Control Desk Triage, SLA Engine & SignalR*<br/>**Harsh** (Reg No: 12306879) — *Campus GIS Map, QR Codes & UI* |

---

## Slide 1: Title & Academic Identity

- **Institution:** Lovely Professional University, Punjab
- **Project Title:** SmartCampus: Campus Issue Reporting & Resolution System
- **Context:** Capstone Engineering Project Defense | Department of Computer Science & Engineering
- **Submitted By (Students):**
  - **Dipanshu** (Reg No: 12323250) — *Role: Backend, Database & Security*
  - **Vishnu** (Reg No: 12307182) — *Role: Control Desk, SLA Engine & SignalR*
  - **Harsh** (Reg No: 12306879) — *Role: Campus GIS Map, QR Codes & UI*
- **Submitted To:** **Mr. Aryan Chauhan** (Teacher ID: 35200), Department of CSE, LPU
- **GitHub:** `https://github.com/DipanshuDhaka/smart_Campus`

> **Speaker Note:** "Respected evaluator Aryan Chauhan sir, and members of the examination committee. Welcome to our capstone presentation on SmartCampus, presented by Dipanshu, Vishnu, and Harsh. SmartCampus is an enterprise-grade campus maintenance and workforce governance platform engineered specifically for large-scale academic institutions like Lovely Professional University."

---

## Slide 2: The Real Campus Problem: Broken Maintenance Systems

1. **[01] Massive Campus Scale:** 600+ acres, 60+ academic blocks, and 25,000+ hostel residents generate dozens of daily breakdowns across electricity, plumbing, and network.
2. **[02] Lost & Informal Complaints:** Complaints reported via phone calls, WhatsApp messages, or paper registers get lost with zero ticket tracking and no audit history.
3. **[03] The False Resolution Trap:** Technicians frequently mark tickets as 'Completed' without doing the work. Students find the exact same issue broken minutes later.
4. **[04] Zero Deadlines or SLA Escalations:** Critical safety issues (like sparking MCBs or overflowing water) remain unattended for days with no automatic alerts or supervisor escalation.

> **Speaker Note:** "In a campus as massive as LPU, traditional facility maintenance breaks down because complaints get lost in WhatsApp chats, workers close tickets without fixing them, and there are no tracked deadlines."

---

## Slide 3: The SmartCampus Solution: Complete End-to-End Governance

1. **[01] One Centralized Web Portal:** A single online system where students report issues, control desk dispatches work orders, and technicians manage their tasks.
2. **[02] Automated Smart Dispatch:** The system analyzes keywords in the issue description to recommend the right department and auto-assigns the worker with the lowest workload.
3. **[03] Independent Quality Verification Gate:** Technicians cannot close tickets. A department supervisor must physically inspect the repair and upload sign-off approval.
4. **[04] Automated 24/7 SLA Engine:** A background monitoring daemon tracks resolution deadlines every 60 seconds and automatically escalates overdue tickets to management.

> **Speaker Note:** "SmartCampus replaces informal reporting with structured tracking, smart worker dispatching, and independent supervisory verification."

---

## Slide 4: System Architecture & Technology Stack

1. **[01] ASP.NET Core (.NET 10 LTS):** High-performance MVC web application organized into 4 modular areas (Admin, Supervisor, Worker, and Student Portal).
2. **[02] SQL Server & EF Core 10:** Relational database schema with 11 core modules, Code-First migrations, and strict foreign-key integrity constraints.
3. **[03] Real-Time SignalR WebSockets:** Pushes instant live alert banners and notification popups to active user screens without needing any page reload.
4. **[04] Leaflet GIS & Vector QR Engine:** Interactive campus map showing all 60+ blocks, paired with dynamic room doorplate QR codes for instant mobile reporting.

> **Speaker Note:** "We built SmartCampus using .NET 10 LTS MVC, SQL Server, SignalR for live push alerts, and Leaflet GIS for campus mapping."

---

## Slide 5: Unit 1 — Enterprise MVC Architecture & Area Routing

1. **[01] Syllabus Topic: MVC Design Pattern:** Separates application into Models (data rules), Views (HTML presentation), and Controllers (request mediation and logic).
2. **[02] SmartCampus Use Case: 4 Discrete Areas:** Structured the app into 4 isolated areas: Admin (Control Desk), Supervisor (Squads & Verification), Worker (Field Tasks), and Reporter (Students).
3. **[03] How It Works: Clean Request Pipeline:** Incoming HTTP requests route directly to specific Area controllers, executing verified business logic and rendering strongly-typed Razor views.
4. **[04] Real Project Benefit:** Prevents code clutter, enforces strict role separation, and allows independent development of student and worker portals.

> **Speaker Note:** "In Unit 1, we implemented the MVC pattern by organizing our project into 4 dedicated ASP.NET Core Areas."

---

## Slide 6: Unit 2 — Relational Data Modeling & EF Core ORM

1. **[01] Syllabus Topic: Code-First ORM & Migrations:** Object-Relational Mapping (ORM) maps C# domain classes to database tables, preventing SQL injection and type mismatches.
2. **[02] SmartCampus Use Case: Campus Spatial Hierarchy:** Models the physical campus hierarchy: Building -> Floor -> Room. A room cannot exist without an assigned floor and building.
3. **[03] How It Works: Fluent API & Cascade Rules:** Configures composite keys, unique indexes, and DeleteBehavior.Restrict to prevent accidental deletion of locations with active history.
4. **[04] Real Project Benefit:** Ensures 100% database integrity across 33 relational tables covering complaints, work orders, assignments, and audit trails.

> **Speaker Note:** "In Unit 2, we designed an EF Core Code-First schema modeling the entire campus hierarchy with strict foreign-key protection."

---

## Slide 7: Unit 3 — Identity Management, OAuth 2.0 & Role Security

1. **[01] Syllabus Topic: Claims-Based Security & RBAC:** Authentication verifies user identity, while Role-Based Access Control (RBAC) ensures users only access authorized actions.
2. **[02] SmartCampus Use Case: 5-Tier User Governance:** Strict permissions across 5 roles: Student/Teacher, Campus Admin (Control Desk), Department Supervisor, Maintenance Worker, and Super Admin.
3. **[03] How It Works: Google OAuth & Domain Whitelist:** Restricts sign-in exclusively to verified university email domains (@lpu.in, @lpu.co.in) and validates anti-forgery CSRF tokens on all forms.
4. **[04] Real Project Benefit: Single-Assignment Lock:** Enforces backend validation so a work order can only be assigned once, completely preventing duplicate technician allocations.

> **Speaker Note:** "Unit 3 covers web security. We implemented role-based authorization, Google domain whitelist, and a single-assignment lock on work orders."

---

## Slide 8: Unit 4 — Background Hosted Services & Real-Time SignalR

1. **[01] Syllabus Topic: IHostedService & Background Loops:** Allows the web server to run background tasks asynchronously on a recurring timer without blocking active user web requests.
2. **[02] SmartCampus Use Case: Automated 60-Second SLA Daemon:** The SLAEngineHostedService runs every 60 seconds in the background, comparing open tickets against configured SLA response targets.
3. **[03] How It Works: 3-Stage Auto-Escalation:** Automatically triggers a 50% Reminder, a 75% Warning to the supervisor, and a 100% Critical Breach Escalation to the Control Desk.
4. **[04] Real Project Benefit: Real-Time SignalR Push:** Instantly pushes notification badges and live popups to connected browser screens via WebSockets without requiring page refreshes.

> **Speaker Note:** "In Unit 4, we built an automated 60-second background daemon that tracks SLA deadlines and pushes real-time SignalR alerts."

---

## Slide 9: Unit 5 — Geospatial GIS Mapping & Dynamic QR Automation

1. **[01] Syllabus Topic: Spatial GIS Models & 2D Barcodes:** Combines geographic coordinates (WGS84 lat/long) with matrix barcodes (QR codes) for location-aware mobile applications.
2. **[02] SmartCampus Use Case: Dynamic Room Doorplate QRs:** Every classroom, lab, and hostel room generates a unique vector QR code. Scanning it pre-fills the exact building, floor, and room.
3. **[03] How It Works: Interactive Leaflet Campus Map:** Interactive map plotted at LPU coordinates (31.25°N, 75.70°E) displaying 60+ campus blocks, hostel wings, and incident hotspots.
4. **[04] Real Project Benefit: 10-Second Issue Filing:** Reduces complaint filing time from minutes to 10 seconds while eliminating wrong room numbers and vague location errors.

> **Speaker Note:** "Unit 5 covers GIS and QR codes. Room doorplates can be scanned on mobile to instantly report issues at that exact spot."

---

## Slide 10: Unit 6 — Software Verification Gates & Quality Engineering

1. **[01] Syllabus Topic: Quality Assurance & Separation of Duties:** Core software engineering rule: The person who performs the work must not be the one who signs off on its correctness.
2. **[02] SmartCampus Use Case: Work Completed != Resolved:** When a technician finishes a repair, the ticket only moves to 'Awaiting Verification'. The technician is strictly blocked from closing it.
3. **[03] How It Works: Mandatory Supervisory Physical Inspection:** The supervisor must physically inspect the location, verify before/after photo evidence, and submit a 1-to-5 star quality rating.
4. **[04] Real Project Benefit: Rejection & Rework Loop:** If a repair is shoddy or incomplete, the supervisor rejects the ticket with feedback, sending it straight back for rework.

> **Speaker Note:** "Unit 6 addresses quality gates. Our core architectural rule is 'Work Completed != Verified Resolved'. Independent verification eliminates fake closures."

---

## Slide 11: Workflow Step 1 & 2: Issue Reporting & Control Desk Triage

- **Live Screenshot:** Student Complaint Form & Control Desk Queue
1. **[01] Student Submits Ticket:** Student logs in and logs an issue for sparking MCB at Room 32-201 (Robotics Lab). Ticket SC-2026-00005 generated.
2. **[02] Automated Duplicate Check:** Control desk checks ticket history: verifies zero duplicate complaints logged for this room in the past 48 hours.
3. **[03] Smart Trade Recommendation:** System scores description keywords (#sparking, #mcb) and automatically recommends the Electrical Department.
4. **[04] Critical Priority & SLA:** Dispatches Work Order WO-2026-00004 with Critical Priority and an automated 1-hour resolution deadline.

---

## Slide 12: Workflow Step 3 & 4: Squad Allocation & Field Execution

- **Live Screenshot:** Technician Mobile Dashboard & Proof Upload Form
1. **[01] Workload Auto-Balancing:** Supervisor selects Senior Electrician Rajesh Verma based on lowest active task count in the electrical roster.
2. **[02] Single-Assignment Lock:** Allocation button locks immediately into 'Squad Allocated', preventing duplicate assignments.
3. **[03] Mobile Field Repair:** Electrician accepts task on mobile portal, starts on-site work, replaces faulty 32A MCB, and tests 230V stable.
4. **[04] Mandatory Photo Evidence:** Uploads after-repair photograph and repair notes. Status changes to 'Awaiting Verification' for supervisor audit.

---

## Slide 13: Workflow Step 5: Physical Inspection & Verified Resolution

- **Live Screenshot:** Verified Resolution & Complete Audit Timeline
1. **[01] On-Site Physical Audit:** Supervisor conducts physical inspection of Room 32-201: checks wiring, tests thermal scan (34°C normal).
2. **[02] 5-Star Quality Sign-Off:** Supervisor awards a 5/5 star rating and submits formal digital inspection sign-off.
3. **[03] Met 1-Hour SLA Target:** Work order marked VerifiedResolved in 24 minutes, well within the 1-hour Critical SLA deadline.
4. **[04] Complete Audit History:** Student views verified resolution with full 10-stage timeline showing every actor, timestamp, and remark.

---

## Slide 14: Student Team Contributions & Individual Ownership

- **[01] Dipanshu (Reg No: 12323250) — Role: Backend, Database & Security**
  - **Core Architecture:** .NET 10 LTS MVC architecture partitioned into 4 Areas.
  - **Data Modeling:** Designed 11 relational modules with EF Core 10 Fluent API.
  - **User Security:** Google OAuth 2.0 institutional email whitelist verification.
  - **Integrity Gate:** Single-assignment lock preventing duplicate allocations.

- **[02] Vishnu (Reg No: 12307182) — Role: Control Desk & Operations Engine**
  - **Triage Desk:** Control Desk triage queue with 48-hour duplicate checker.
  - **SLA Daemon:** 60-second automated recurring background SLA monitor.
  - **Real-Time SignalR:** WebSocket live push alerts for 50%, 75%, and 100% breaches.
  - **Analytics Dashboard:** Executive analytics reporting MTTR and trade hotspots.

- **[03] Harsh (Reg No: 12306879) — Role: Campus GIS, QR Engine & UI/UX**
  - **Campus GIS Map:** Interactive Leaflet.js map plotting 60+ LPU campus blocks.
  - **Vector QR Doorplates:** QRCoder dynamic SVG/PNG doorplates for instant reporting.
  - **Mobile Worker Portal:** Task execution interface with photo evidence upload.
  - **LPU UMS Aesthetic:** Authentic institutional visual styling with Bootstrap 5.

---

## Slide 15: Project Summary & Defense Conclusion

- **✔ Production-Ready Architecture:** Built and tested end-to-end on .NET 10 LTS and SQL Server with 0 errors.
- **✔ Eliminates False Closures:** Mandatory supervisory physical inspection guarantees issues are genuinely fixed.
- **✔ Automated Intelligence:** Smart keyword trade suggestions, auto-balanced assignments, and 60s SLA alerts.
- **✔ Source Code on GitHub:** All 187 project files, models, controllers, reports, and tests are pushed and synchronized.
- **Official GitHub Repository:** `https://github.com/DipanshuDhaka/smart_Campus`
- **Thank you, Aryan Chauhan Sir & Examination Committee! We are open for Technical Questions.**
