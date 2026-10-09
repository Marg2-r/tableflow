# TableFlow

TableFlow is a restaurant table reservation prototype. Guests can select a
date, time, and available table, while managers can manage tables, booking
settings, and the reservation lifecycle.

The current version targets one restaurant and local development.

## Prototype features

- search for available times and tables;
- guest reservations without customer accounts;
- protection against overlapping and concurrent bookings;
- manager authentication with an HttpOnly cookie;
- restaurant-scoped manager access;
- table and floor-plan management;
- configurable reservation duration, slot interval, and related rules;
- reservation search and filtering;
- `Confirmed`, `Seated`, `Completed`, `NoShow`, and `Cancelled` statuses;
- backend tests for status transitions and table availability.

## Technology stack

- ASP.NET Core 10;
- Entity Framework Core 10;
- PostgreSQL 18;
- ASP.NET Core Identity;
- React 19;
- Vite 8;
- Docker Compose;
- xUnit.

## Project structure

```text
TableFlow/
├── srcTableFlow.Api/          ASP.NET Core API and Docker Compose
├── tableflow-web/             React frontend
├── tests/TableFlow.Api.Tests/ Backend tests
├── docs/                      Documentation and checklists
└── TableFlow.slnx             .NET solution
```

## Requirements

The recommended setup requires:

- Docker Desktop with Docker Compose;
- Node.js 22 or newer;
- npm.

The .NET 10 SDK is also required when running or testing the backend outside
Docker.

## First-time setup

Run the commands below from the repository root.

### 1. Create the local `.env` file

```powershell
Copy-Item .\srcTableFlow.Api\.env.example .\srcTableFlow.Api\.env
```

Open `srcTableFlow.Api/.env` and replace the example passwords. The manager
password must contain at least 12 characters, including an uppercase letter,
a lowercase letter, a number, and a special character.

Never commit the real `.env` file.

### 2. Start PostgreSQL and the API

```powershell
docker compose -f .\srcTableFlow.Api\compose.yaml up --build
```

On the first startup, the API automatically:

- creates the database and applies EF Core migrations;
- adds the demo restaurant with `id = 1`;
- creates the `Manager` role;
- creates the manager configured through `BOOTSTRAP_ADMIN_*` variables.

Backend addresses:

- API: <http://localhost:8080>
- health check: <http://localhost:8080/health>
- Swagger UI: <http://localhost:8080/swagger>
- PostgreSQL from the host: `localhost:5433`

### 3. Start the frontend

Open a second terminal:

```powershell
cd .\tableflow-web
npm install
npm run dev
```

Open <http://localhost:5173>.

Use `BOOTSTRAP_ADMIN_EMAIL` and `BOOTSTRAP_ADMIN_PASSWORD` from the local
`.env` file to sign in to the `Management` section.

## Starting the project again

Backend and database:

```powershell
docker compose -f .\srcTableFlow.Api\compose.yaml up
```

Frontend:

```powershell
cd .\tableflow-web
npm run dev
```

## Stopping the project

```powershell
docker compose -f .\srcTableFlow.Api\compose.yaml down
```

A regular `down` command preserves the PostgreSQL volume and its data.

To completely remove the local database and start from scratch, run:

```powershell
docker compose -f .\srcTableFlow.Api\compose.yaml down -v
```

> Warning: `down -v` permanently deletes all local TableFlow data.

## Reservation lifecycle

```text
Confirmed ──→ Seated ──→ Completed
    │
    ├────────→ NoShow
    │
    └────────→ Cancelled
```

Allowed transitions:

- `Confirmed → Seated`;
- `Confirmed → NoShow`;
- `Confirmed → Cancelled`;
- `Seated → Completed`.

`Completed`, `NoShow`, and `Cancelled` are terminal statuses. The API returns
`409 Conflict` when a transition is not allowed.

## Main API endpoints

Routes do not use an `/api` prefix.

### Public endpoints

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/health` | API health check |
| `GET` | `/restaurants/{restaurantId}/tables/available` | Available tables |
| `GET` | `/restaurants/{restaurantId}/tables/available-times` | Available-time overview |
| `GET` | `/restaurants/{restaurantId}/tables/{tableId}/available-times` | Times for one table |
| `POST` | `/restaurants/{restaurantId}/reservations` | Create a reservation |
| `POST` | `/auth/login` | Manager login |

### Endpoints requiring the manager cookie

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/auth/me` | Get the current manager |
| `POST` | `/auth/logout` | End the current session |
| `GET/POST` | `/restaurants/{restaurantId}/tables` | List and create tables |
| `PUT/DELETE` | `/restaurants/{restaurantId}/tables/{id}` | Update or deactivate a table |
| `GET` | `/restaurants/{restaurantId}/reservations` | List reservations |
| `GET` | `/restaurants/{restaurantId}/reservations/{id}` | Reservation details |
| `PATCH` | `/restaurants/{restaurantId}/reservations/{id}/status` | Change reservation status |
| `GET/PUT` | `/restaurants/{restaurantId}/management/settings` | Reservation settings |

A manager can access only the `restaurantId` stored in their cookie identity.
The API returns `403 Forbidden` for a different restaurant.

## Project checks

### Backend

```powershell
dotnet build .\TableFlow.slnx --configuration Release
dotnet test .\tests\TableFlow.Api.Tests\TableFlow.Api.Tests.csproj --configuration Release
```

### Frontend

```powershell
cd .\tableflow-web
npm run lint
npm run build
```

## Environment variables

| Variable | Purpose |
|---|---|
| `POSTGRES_PASSWORD` | Local PostgreSQL password |
| `BOOTSTRAP_ADMIN_EMAIL` | Initial manager email |
| `BOOTSTRAP_ADMIN_PASSWORD` | Initial manager password |
| `BOOTSTRAP_ADMIN_RESTAURANT_ID` | Initial manager restaurant; use `1` for the prototype |
| `FRONTEND_ORIGIN` | Allowed frontend CORS origin |

If the manager already exists, changing `BOOTSTRAP_ADMIN_PASSWORD` does not
automatically update the existing account password.

## Scope of v0.1

`v0.1.0-prototype` is intended for local demonstrations and validation of the
core user journey. Before a public release, the project still needs CSRF
protection, rate limiting, production deployment, HTTPS, backups, monitoring,
and legal pages.

Use [`docs/prototype-checklist.md`](docs/prototype-checklist.md) for the final
prototype verification.
