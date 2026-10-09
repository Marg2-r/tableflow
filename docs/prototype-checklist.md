# TableFlow v0.1 prototype checklist

This checklist defines the minimum acceptance criteria for the first working
prototype. Run the checks with test data.

## Startup

- [ ] `srcTableFlow.Api/.env` was created from `.env.example`.
- [ ] `docker compose up --build` starts PostgreSQL and the API without errors.
- [ ] `GET http://localhost:8080/health` returns a successful response.
- [ ] The frontend opens at `http://localhost:5173`.
- [ ] Created data remains available after restarting the containers.

## Public booking

- [ ] Available times load without authentication.
- [ ] The floor plan shows only suitable active tables.
- [ ] A guest can create a reservation.
- [ ] A new reservation appears in the management section.
- [ ] An overlapping reservation cannot be created for the same table.
- [ ] An inactive or undersized table cannot be booked.

## Manager authentication

- [ ] An incorrect password does not grant access.
- [ ] Five failed attempts temporarily lock the account.
- [ ] Correct credentials open the Management section.
- [ ] Refreshing the page preserves the active session.
- [ ] Logout ends the session.
- [ ] Management endpoints return `401` without the manager cookie.
- [ ] Requests for another restaurant return `403`.

## Management

- [ ] A manager can create and update a table.
- [ ] A manager can update table positions on the floor plan.
- [ ] A deactivated table is no longer offered to guests.
- [ ] Reservation settings load and save successfully.
- [ ] Reservations can be filtered by date, status, and search text.

## Reservation statuses

- [ ] `Confirmed → Seated` works.
- [ ] `Seated → Completed` works.
- [ ] `Confirmed → NoShow` works.
- [ ] `Confirmed → Cancelled` works.
- [ ] Terminal reservations do not display available actions.
- [ ] An invalid transition through the API returns `409 Conflict`.

## Automated checks

From the project root:

```powershell
dotnet build .\TableFlow.slnx --configuration Release
dotnet test .\tests\TableFlow.Api.Tests\TableFlow.Api.Tests.csproj --configuration Release
```

From `tableflow-web`:

```powershell
npm run lint
npm run build
```

- [ ] The backend solution builds without errors or warnings.
- [ ] All backend tests pass.
- [ ] Frontend lint passes.
- [ ] The frontend production build succeeds.

## Prototype checkpoint

- [ ] Documentation matches the current commands and ports.
- [ ] Staged files do not include `.env`, passwords, `node_modules`, or `dist`.
- [ ] Changes are merged into `main`.
- [ ] The `v0.1.0-prototype` tag exists.

## Not blocking v0.1

The following items are required before a public release, but they do not
block the local prototype:

- CSRF protection;
- rate limiting;
- production HTTPS and domain configuration;
- PostgreSQL backups;
- monitoring and centralized logging;
- email notifications;
- guest cancellation through a secure token;
- privacy policy and other legal pages;
- final visual and mobile polish.
