# Contract Monthly Claim System

> A modern ASP.NET Core solution that streamlines monthly teaching claim approvals for Lecturers, Coordinators, Managers, and HR.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-5C2D91?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![SQLite](https://img.shields.io/badge/Database-SQLite-003B57?logo=sqlite&logoColor=white)](https://www.sqlite.org/index.html)

---

## 📌 Table of Contents

- [Quick Snapshot](#-quick-snapshot)
- [How It Works](#-how-it-works)
- [File Responsibilities](#-file-responsibilities)
- [Tech Stack](#-tech-stack)
- [Run It in Visual Studio](#-run-it-in-visual-studio)
- [Project Structure](#-project-structure)
- [Credits](#-credits)

## 🛰️ Quick Snapshot

Contract Monthly Claim System is an ASP.NET Core MVC app that streamlines monthly teaching claim approvals from lecturers through to HR. It keeps roles focused, enforces policy rules, and delivers ready-to-export finance summaries.

**Key capabilities:**

- End-to-end claim submission and approvals across Lecturer → Coordinator → Manager → HR.
- Automatic rule checks and flags to highlight unusual hours, rates, or missing documents.
- Dashboard notifications, status tracking, and downloadable HR reports.

> [!TIP]
> Use the sections below to see how the workflow fits together, what each file does, and how to launch the project fast.

## 🔄 How It Works

1. **Lecturer** logs in, records hourly work, attaches proof, and submits the claim.
2. **Coordinator** reviews automated alerts, approves, or returns the claim with notes.
3. **Manager** performs the final approval check before payment prep.
4. **HR** updates lecturer records and exports invoice-style summaries for finance.

Each step is surfaced through role-specific dashboards backed by ASP.NET Core Identity, so everyone only sees the tasks relevant to them.

## 🧭 File Responsibilities

| Folder/File | Purpose |
| --- | --- |
| `Controllers/` | MVC controllers for Lecturer, Coordinator, Manager, Home, and more role flows. |
| `Services/ClaimService.cs` | Core business rules, totals, and document handling. |
| `Services/ReportService.cs` | HR-friendly report builders and exports. |
| `Interfaces/` | Contracts for repositories and services (e.g., `IClaimService`, `IClaimRepository`). |
| `Repositories/ClaimRepository.cs` | Entity Framework Core data access for claims. |
| `Models/` | Domain entities (`Claim`, `ApplicationUser`) plus enums for statuses and roles. |
| `ViewModels/` | Shapes used by Razor views for forms and dashboards. |
| `Views/` | Razor pages grouped per role with shared layouts and partials. |
| `Areas/HR/Pages/` | Razor Pages for HR CRUD screens and reporting tools. |
| `Data/ApplicationDbContext.cs` | Entity Framework Core context configuration. |
| `Data/SeedData.cs` | Seeds demo users, roles, and sample claims. |
| `wwwroot/` | Static assets (CSS, JS) and uploaded support documents. |
| `appsettings.json` | Connection string, upload limits, and app configuration. |
| `Program.cs` | Dependency injection, Identity, database, and middleware setup. |

## 🧰 Tech Stack

- **ASP.NET Core 8.0 MVC** for the main web application.
- **Razor Pages** to power the HR area.
- **Entity Framework Core 8.0 + SQLite** for data persistence with minimal setup.
- **ASP.NET Core Identity** for authentication and role management.
- **Bootstrap 5 & jQuery** for responsive UI and light interactivity.

## 🚀 Run It in Visual Studio

| Visual Studio (UI) | Command Line |
| --- | --- |
| 1. Install the **.NET 8 SDK** and Visual Studio 2022 with the *ASP.NET and web development* workload.<br>2. Open **File → Open → Project/Solution** and select `CMS ASSIGNMENT.sln`.<br>3. Press **F5** (or the green *Start* button) and Visual Studio builds, runs, and seeds demo data automatically. | Prefer the terminal? From the project root run:<br><br>```bash
dotnet restore
dotnet run
```<br>The console prints the local URL (usually `https://localhost:5001`). |

## �️ Project Structure

<details>
<summary><strong>Expand to view the full structure</strong></summary>

```text
CMS ASSIGNMENT/
├── CMS ASSIGNMENT.sln           # Start here when opening the solution
├── Controllers/
│   ├── LecturerController.cs    # Lecturer dashboard and submissions
│   ├── CoordinatorController.cs # Pending review workflow
│   ├── ManagerController.cs     # Final approval actions
│   └── HomeController.cs        # Landing and role redirects
├── Services/
│   ├── ClaimService.cs          # Core business logic and document handling
│   └── ReportService.cs         # HR report helpers and exports
├── Interfaces/
│   ├── IClaimRepository.cs      # Data access contracts for claims
│   └── IClaimService.cs         # Abstraction for claim operations
├── Repositories/
│   └── ClaimRepository.cs       # EF Core implementation of repositories
├── Models/
│   ├── Claim.cs                 # Claim entity with flags and metadata
│   └── ApplicationUser.cs       # Identity user plus role info
├── ViewModels/
│   ├── ClaimViewModel.cs        # Form binding for claim submissions
│   └── ClaimListViewModel.cs    # List shape for dashboards
├── Views/
│   ├── Lecturer/                # Razor views for lecturers
│   ├── Coordinator/             # Coordinator review screens
│   ├── Manager/                 # Manager approvals and reports
│   └── Shared/                  # Layouts, partials, validation scripts
├── Areas/
│   └── HR/
│       └── Pages/
│           ├── Index.cshtml     # HR landing page
│           ├── Lecturers/       # CRUD Razor Pages for lecturers
│           └── Reports/         # Invoice and summary exports
├── Data/
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   └── SeedData.cs              # Seeds roles, users, and sample claims
├── wwwroot/
│   ├── css/                     # Global styles
│   ├── js/                      # Client-side scripts
│   └── uploads/                 # Stored lecturer documents
└── appsettings.json             # Connection strings and configuration
```

</details>

## 👤 Credits

Built by **Dylan Gorrah** — feel free to adapt the system to fit your automation goals.
