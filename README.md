# RaceDay: Part 3 (MVC Web Front End)

ASP.NET Core 8 MVC application for the RaceDay platform. It consumes the Part 2 REST API for **all** data operations.
It contains no database access and no duplicated business logic, and it never talks to Azure directly.
File uploads (event banners, profile pictures) are sent to the API, which stores them in Azure Blob Storage.

## Features
- **Auth & roles:** register / log in via the API; session holds token + role; protected pages redirect to login; logout clears the session.
- **Organiser:** dashboard (events, enrolments per event, upcoming dates), event CRUD with banner upload, categories (age / distance), enrolments list, results entry.
- **Participant:** upcoming events with filters (search, type, max distance), event details, enrol with category, My Enrolments (colour-coded status), My Results ("47th out of 312 finishers"), profile + picture upload.
- **UI:** RaceDay theme (Bootstrap 5 + custom CSS), consistent layout, friendly error messages (no raw errors shown).

## Configuration
The API address is set with `Api:BaseUrl` (see `RaceDay.Mvc/appsettings.json`). Routes the MVC app calls are listed in one place:
`RaceDay.Mvc/Services/ApiService.cs` → `Routes` class.

## Run locally
```bash
cd RaceDay.Mvc
dotnet run
```
(Make sure the Part 2 API is running and `Api:BaseUrl` points at it.)

## Run with Docker (single command)
Build:
```bash
docker build -t raceday-mvc ./RaceDay.Mvc
```
Run (replace the API URL with your deployed or local API):
```bash
docker run -p 8080:8080 -e Api__BaseUrl=https://YOUR-API-URL/ raceday-mvc
```
Then open http://localhost:8080

If your API runs on your own machine (e.g. port 5000), use `-e Api__BaseUrl=http://host.docker.internal:5000/`.

## Azure Blob Storage (in the API)
See `ApiChanges/`. The API needs the `Azure.Storage.Blobs` package, a `BlobStorage:ConnectionString` setting and the upload endpoints:
- `POST api/events/{id}/banner`
- `POST api/profile/picture`

## CI
`.github/workflows/ci.yml` builds every project in the repo (API + MVC) and the MVC Docker image on each push.

## Demo video
YouTube (unlisted): _add link here_
