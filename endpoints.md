# RaceDay API - Endpoint Plan

This plan covers all functional areas required by the brief: Authentication
(register/login), User Profile, Events, Categories, Event Enrolments, and
Results. It is completed before any Part 2 API code is written, and the
implemented API must closely match it.

| Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Register a new user | Public | `{ fullName, email, password, roleName }` | `201 { userId, token }` |
| POST | /api/auth/login | Authenticate and issue a JWT | Public | `{ email, password }` | `200 { token, role }` |
| GET | /api/users/me | Get logged-in user's profile | Authenticated | — | `200 { userId, fullName, email, role }` |
| PUT | /api/users/me | Update own profile | Authenticated | `{ fullName, email }` | `200 { updated profile }` |
| GET | /api/events | List all events | Public | — | `200 [ events ]` |
| GET | /api/events/{id} | Get a single event's detail | Public | — | `200 { event }` |
| POST | /api/events | Create a new event | Admin | `{ name, eventDate, location, description }` | `201 { eventId }` |
| PUT | /api/events/{id} | Update an event | Admin | `{ name, eventDate, location, description, status }` | `200 { event }` |
| DELETE | /api/events/{id} | Delete an event | Admin | — | `204 No Content` |
| GET | /api/categories | List all categories | Public | — | `200 [ categories ]` |
| POST | /api/categories | Create a category | Admin | `{ name, description }` | `201 { categoryId }` |
| GET | /api/events/{id}/categories | List categories linked to an event | Public | — | `200 [ categories ]` |
| POST | /api/events/{id}/categories | Link a category to an event | Admin | `{ categoryId }` | `201 { eventCategoryId }` |
| POST | /api/enrolments | Enrol the logged-in user into an event category | Athlete | `{ eventCategoryId }` | `201 { enrolmentId, status }` |
| GET | /api/enrolments/me | List logged-in user's own enrolments | Authenticated | — | `200 [ enrolments ]` |
| DELETE | /api/enrolments/{id} | Cancel own enrolment | Authenticated | — | `204 No Content` |
| GET | /api/events/{id}/results | Get results for an event | Public | — | `200 [ results ]` |
| POST | /api/results | Capture a result for an enrolment | Admin | `{ enrolmentId, finishTime, position }` | `201 { resultId }` |
| PUT | /api/results/{id} | Update a result | Admin | `{ finishTime, position, status }` | `200 { result }` |

## Notes on design decisions

- **Two roles**: `Admin` (manages events, categories, and results) and
  `Athlete` (browses events, manages their own profile, enrols in
  categories, views results). This matches the `RoleName` check
  constraint on the `Users` table in the SQL script.
- **Nested routes** (`/api/events/{id}/categories`,
  `/api/events/{id}/results`) are used where a resource is naturally
  scoped to a parent event, matching the `EventCategories` junction
  table that resolves the Event–Category many-to-many relationship in
  the ERD.
- Endpoints beyond the six required areas (category creation, event
  update/delete) were added because they are necessary for an Admin to
  actually manage the system end-to-end.
