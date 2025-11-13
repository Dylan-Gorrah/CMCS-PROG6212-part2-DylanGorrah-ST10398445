# DG CMS System

**Tag:** `dev`

This is a cosmic-themed ASP.NET Core MVC app that helps lecturers, coordinators, and managers move claims through a simple workflow. Lecturers submit claims with documents, coordinators review them, and managers give the final sign-off. All of that rides on a SQLite database and ASP.NET Core Identity, so you get role-based access control out of the box.

## Run It Locally (Quick Start)

### What you need

- .NET 8.0 SDK or newer — download from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)  
  Make sure `dotnet --version` shows `8.0.x` or higher.
- Visual Studio 2022 (optional, but convenient) with the **ASP.NET and web development** workload.

### Option 1 — Visual Studio (the easy way)

1. Unzip the project somewhere on your machine.
2. Open Visual Studio and choose **File → Open → Project/Solution**.
3. Pick `CMS ASSIGNMENT.sln`.
4. Hit **F5** to run. The SQLite database appears and seeds itself automatically.

### Option 2 — Command line (nice and direct)

```bash
cd "CMS ASSIGNMENT"
dotnet restore
dotnet run
```

Then browse to the URL that shows up in the console (usually `https://localhost:5001`).

---

## Feature Highlights

- 🔐 **Role-based access** — Lecturer, Coordinator, Manager each see their own dashboards.
- 📋 **Claim workflow** — Submit, review, approve/reject with status tracking.
- 📎 **File uploads** — PDFs, images, and Office docs up to 5 MB.
- 💾 **SQLite storage** — Lightweight file-based database that spins up automatically.
- 🌌 **Cosmic UI** — Gradient backgrounds, particles, custom cursor, and glide-y animations.

---

## Default Login Accounts

| Role        | Email                  | Password          |
| ----------- | ---------------------- | ----------------- |
| Lecturer    | `lecturer@test.com`    | `Lecturer123!`    |
| Coordinator | `coordinator@test.com` | `Coordinator123!` |
| Manager     | `manager@test.com`     | `Manager123!`     |

---

## How the data fits together

**ApplicationUser**

- Inherits from ASP.NET Core Identity.
- Adds `FirstName`, `LastName`, and a `Role` enum (Lecturer, Coordinator, Manager).
- Tracks submitted claims and coordinated claims.

**Claim**

- Stores hours, rate, total amount, notes, and document metadata.
- Status moves from Pending → ApprovedByCoordinator → ApprovedByManager, with reject paths at each review step.
- Links back to the lecturer and coordinator (plus whoever approved or rejected it).

```
Pending → ApprovedByCoordinator → ApprovedByManager
       ↘ RejectedByCoordinator
                      ↘ RejectedByManager
```

---

## Project layout

```
CMS ASSIGNMENT/
├── Controllers/      // MVC controllers for each role
├── Data/             // DbContext + seeding
├── Models/           // Entity models
├── Views/            // Razor views (cosmic theme lives here)
├── Services/         // Business logic
├── Repositories/     // Data access layer
├── Interfaces/       // Abstractions
└── wwwroot/          // CSS, JS, uploads
```

---

## Tech stack

- ASP.NET Core 8.0 MVC
- Entity Framework Core 8.0
- ASP.NET Core Identity
- SQLite
- Bootstrap 5 + a dash of jQuery

---

## Helpful notes

- The database is rebuilt each time the app starts (great for demos).
- Uploaded files land in `wwwroot/uploads/`.
- Password rules: min 6 chars, one uppercase, one lowercase, one digit.
- Everything stores timestamps in UTC.

---

## Troubleshooting tips

- **NuGet packages missing?**  
  Run `dotnet restore` or right-click the solution in Visual Studio and choose **Restore NuGet Packages**.

- **Database acting up?**  
  Delete `ClaimsManagement.db` and run the app again. It’ll regenerate.

- **Port already taken?**  
  Visual Studio will pick a new one automatically. Just check the console output.

---

### Developed by Dylan Gorrah

Built with cosmic inspiration ✨
