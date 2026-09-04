# RaceDay System

## System Description

RaceDay is a race event management system that allows athletes to browse
upcoming running/sporting events, view the categories offered within each
event, and enrol in a category of their choice. Event organisers (Admins)
create and manage events and categories, and capture results once an
event has taken place. The system is built around six core entities:
Users, Events, Categories, EventCategories (resolving the many-to-many
relationship between events and categories), Enrolments, and Results.

Full planning documentation — the ERD, the API endpoint plan, and the SQL
schema creation script — lives in the `/docs` folder and was completed
before any application code was written, as required.

## Roles

| Role | Description |
|---|---|
| **Admin** | Creates and manages events and categories, records and updates race results, and has full oversight of the system. |
| **Athlete** | Registers and manages their own profile, browses events and categories, enrols in event categories, and views published results. |

## Planning Documents (`/docs`)

- `erd.png` (or `erd.svg`) — Entity Relationship Diagram for the full data model
- `endpoints.md` — Complete API endpoint plan (method, route, description, role, request body, expected response)
- `script.sql` — SQL Server script that creates and populates the schema, matching the ERD exactly

## CI/CD

A GitHub Actions workflow (`.github/workflows/validate-structure.yml`)
validates on every push/PR that:
- the `/docs` folder exists
- `/docs` contains the ERD, endpoint plan, and SQL script
- `README.md` exists at the repository root

**Successful build screenshot:**

_(Insert screenshot of a green GitHub Actions run here before submission.)_

## Video Walkthrough

**Unlisted YouTube link:** _(insert link here)_

The video walks through the planning documents, explains the ERD design
decisions (including why `EventCategories` was introduced to resolve the
Event–Category many-to-many relationship), and covers the reasoning
behind the endpoint plan.

## Running the SQL Script

1. Open SSMS and connect to a clean SQL Server instance.
2. Open `docs/script.sql`.
3. Execute the full script — it creates `RaceDayDB`, all six tables with
   primary/foreign keys, and inserts sample data.
