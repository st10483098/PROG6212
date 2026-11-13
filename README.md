# RaceDay API (PROG6212 POE Part 2)

ASP.NET Core 8 Web API using Entity Framework Core (Code-First, SQL Server), session-based role authentication, Swagger, xUnit tests and GitHub Actions CI/CD.

## Setup and run
1. Install the .NET 8 SDK and SQL Server / LocalDB.
2. Check `ConnectionStrings:DefaultConnection` in `RaceDay.Api/appsettings.json`.
3. Create the database (Package Manager Console, default project `RaceDay.Api`):
   ```
   Add-Migration InitialCreate
   Update-Database
   ```
   or from a terminal: `dotnet ef migrations add InitialCreate --project RaceDay.Api` then `dotnet ef database update --project RaceDay.Api`
4. Run: `dotnet run --project RaceDay.Api` and open `/swagger`.
5. Run tests: `dotnet test`

## The two roles
| Role | Can do |
|------|--------|
| **Participant** | Register/login, view/update own profile, view events and categories, enrol in an event with a category, view own results |
| **Organiser** | Own profile, create/update/delete own events, define categories, view enrolments for own events, capture finish times and positions |

Organiser registration requires an invite code (`Organiser:SignupCode` in appsettings, default `RACEDAY-ORG-2025`) so users cannot self-promote. Passwords are hashed with ASP.NET Core `PasswordHasher`. Sessions store `UserId` and `Role`; the `[SessionAuthorize]` filter returns 401 (not logged in) or 403 (wrong role).

## Endpoints
| Method | Route | Access |
|---|---|---|
| POST | /api/auth/register, /login, /logout | Public |
| GET, PUT | /api/profile | Any logged in |
| GET | /api/events, /api/events/{id} | Any logged in |
| POST, PUT, DELETE | /api/events[/{id}] | Organiser |
| GET | /api/events/{id}/categories | Any logged in |
| POST | /api/categories | Organiser |
| POST | /api/enrolments | Participant |
| GET | /api/enrolments/mine | Participant |
| GET | /api/events/{id}/enrolments | Organiser |
| POST | /api/results | Organiser |
| GET | /api/results/mine | Participant |

## CI/CD
GitHub Actions builds and runs all tests on every push (`.github/workflows/ci.yml`).

![Green build](docs/ci-green.png)  <!-- add your screenshot here -->

## Video
YouTube (unlisted): <ADD LINK>
