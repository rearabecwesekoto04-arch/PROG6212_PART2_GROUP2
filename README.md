# RaceDay API

**Module:** PROG6212 – Programming 2B
**Part 2:** RESTful API Development
**Student name:** `<Tlhologelo Rearabecwe Sekoto>`
**Student number:** `<ST10438915>`

RaceDay is an ASP.NET Core Web API for managing race events (run, walk or cycle). Organisers create events and categories and capture results. Participants register, enrol in events and view their results. Access is controlled by role.

## Video Presentation

`<>`

## CI Build Status
![CI](https://github.com/<username>/<repo>/actions/workflows/ci.yml/badge.svg)

Screenshot of a successful (green) GitHub Actions run:
![CI screenshot](docs/ci-screenshot.png)

## Tech Stack
- ASP.NET Core Web API (C#)
- Entity Framework Core (Code-First) with the InMemory provider
- JWT Bearer authentication with role-based authorisation
- BCrypt password hashing (BCrypt.Net-Next)
- Swagger / OpenAPI (Swashbuckle)
- xUnit with `WebApplicationFactory` for integration-style tests
- GitHub Actions for CI

## Project Structure
RaceDay/
├── .github/workflows/ci.yml     # CI pipeline
├── RaceDay.Api/
│   ├── Controllers/             # Auth, Users, Events, Categories, Enrolments, Results
│   ├── Data/                    # AppDbContext and DbSeeder
│   ├── Dtos/                    # Request DTOs
│   ├── Models/                  # Role, User, Event, Category, Enrolment, Result
│   ├── appsettings.json         # JWT settings
│   └── Program.cs
├── RaceDay.Tests/               # xUnit tests
└── README.md
```

## Setup and Run
### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (version matching the project's target framework)
- Visual Studio 2022 or later, or the .NET CLI

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/<username>/<repo>.git
   cd <repo>
   ```
2. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   ```
3. Run the API:
   ```bash
   dotnet run --project RaceDay.Api
   ```
4. Open Swagger in your browser (use the port shown in the console):
   ```
   https://localhost:<port>/swagger
   ```
The database is in-memory, so it is recreated and re-seeded every time the application starts.

### Configuration
JWT settings are in `RaceDay.Api/appsettings.json`:

```json
"Jwt": {
  "Key": "<secret key, at least 32 characters>",
  "Issuer": "RaceDay",
  "Audience": "RaceDayUsers"
}
```

## Authentication and Roles
There are two roles: **Organiser** and **Participant**.
- **Organisers are preseeded** at startup. They cannot be created through the API, so nobody can register themselves as an Organiser.
- **Participants register** through `POST /api/auth/register`. The role is always set to Participant on the server.
- Both roles log in through `POST /api/auth/login` and receive a JWT containing their user ID and role.
- Passwords are hashed with BCrypt and the plain text is never stored.
- Non-public endpoints require a valid token (401 without one). Using the wrong role returns 403.
- Organisers can only modify categories, enrolments and results that belong to their own events.

### Seeded accounts
All seeded accounts use the password `Password123!` (demo only).
| Role | Name | Email |
|---|---|---|
| Organiser | Tlhologelo Sekoto | tr.otokes23@raceday.co.za |
| Organiser | Thandi Mathe | sebotse.mm21@raceday.co.za |
| Participant | Shonisani Thavhiwa | shoni.sp12@raceday.co.za |
| Participant | Rearabecwe Tlatsi | tlatsi.otokes04@raceday.co.za |

### Using the token in Swagger
1. Call `POST /api/auth/login` and copy the `token` value.
2. Click **Authorize** at the top of Swagger UI.
3. Paste the token (Swagger adds `Bearer` automatically) and click **Authorize**.

## Endpoints
| Method | Route | Access | Purpose |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register a Participant account |
| POST | `/api/auth/login` | Public | Log in and receive a JWT |
| GET | `/api/users/me` | Any logged-in user | View own profile |
| PUT | `/api/users/me` | Any logged-in user | Update own profile |
| GET | `/api/events` | Any logged-in user | List events |
| GET | `/api/events/{id}` | Any logged-in user | Get one event |
| POST | `/api/events` | Organiser | Create an event |
| PUT | `/api/events/{id}` | Organiser (owner) | Update an event |
| DELETE | `/api/events/{id}` | Organiser (owner) | Delete an event |
| GET | `/api/categories?eventId=` | Any logged-in user | List categories |
| GET | `/api/categories/{id}` | Any logged-in user | Get one category |
| POST | `/api/categories` | Organiser (owner of event) | Create a category |
| PUT | `/api/categories/{id}` | Organiser (owner of event) | Update a category |
| DELETE | `/api/categories/{id}` | Organiser (owner of event) | Delete a category |
| POST | `/api/enrolments` | Participant | Enrol in an event via a category |
| GET | `/api/enrolments/mine` | Participant | View own enrolments |
| GET | `/api/enrolments/event/{eventId}` | Organiser (owner) | View all enrolments for an event |
| POST | `/api/results` | Organiser (owner) | Capture a result |
| PUT | `/api/results/{id}` | Organiser (owner) | Update a result |
| GET | `/api/results/event/{eventId}` | Organiser (owner) | View results for an event |
| GET | `/api/results/mine` | Participant | View own results |

### Business rules
- A participant can enrol in one category per event.
- Enrolment is blocked when a category reaches its `MaxParticipants`.
- A category cannot be deleted while it has enrolments.
- Each enrolment can have only one result.

## Database
The schema follows the Part 1 ERD and SQL script:
`ROLES` → `USERS` → `EVENTS` → `CATEGORIES` → `ENROLMENTS` → `RESULTS`

EF Core Code-First models mirror these tables, including the foreign keys and the unique constraints on `Users.Email` and `Results.EnrolmentId`.

## Swagger
Swagger UI is available at `/swagger`. Every endpoint is listed and testable from the browser, with a description of its purpose, the data it expects and the responses it returns.

## Unit Tests
Tests are in `RaceDay.Tests` and use `WebApplicationFactory<Program>`, so requests run through the real authentication and authorisation pipeline.

Run them with:
dotnet test

Covered scenarios include:
- Registration and login (success and failure)
- Event CRUD as an Organiser
- 401 for unauthenticated requests
- 403 for the wrong role and for a different organiser
- 404 for missing resources
- Participant enrolment and verification that it is recorded

## Continuous Integration
The workflow in `.github/workflows/ci.yml` runs on every push and pull request to `main`. It restores, builds, runs the tests and publishes the build as an artifact.

## Design Decisions and Limitations
- **In-memory database:** Used for development and testing. Data resets when the app restarts.
- **JWT authentication:** The token carries the user ID and role. Authorisation is enforced with `[Authorize(Roles = "...")]` plus ownership checks in the controllers.
- **Seeded passwords:** The demo password is for development only. A production system would use unique secrets and keep the JWT key outside source control.
