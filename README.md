# JobForge API

An API that lets an app hand off slow or unreliable tasks, like sending an email, generating a report, or calling another service, so they run in the background instead of making the user wait. If a task fails, it's automatically retried a few times before being marked as failed. Multiple teams ("organizations") can use the same API at once, each only ever seeing their own data.

## Quick Start

Requires Docker and Docker Compose.

```bash
git clone <repo-url>
cd JobForgeAPI
```

Copy the `.env.example` file into an actual `.env` file, open it and fill in real values (any local values work; see comments in `.env.example` for guidance).

```bash
docker compose up --build
```

The database schema is applied automatically the first time the app starts. The API is now available at `http://localhost:8080`. A few places worth visiting:

- `http://localhost:8080/scalar`: interactive API documentation, with every endpoint listed and a built-in way to try requests directly in the browser
- `http://localhost:8080/health`: confirms the API and its database connection are both working
- `http://localhost:8080/hangfire`: a live dashboard showing background jobs as they're queued, processed, retried, or completed

To try the full flow: register an account (this also creates your organization), log in to get an access token, then use that token to submit a job and watch it get picked up and processed in the Hangfire dashboard.

## Tech Stack

- **Runtime**: .NET 10, ASP.NET Core
- **Database**: PostgreSQL, via Entity Framework Core
- **Background processing**: Hangfire, backed by the same Postgres database
- **Auth**: JWT bearer tokens, with passwords hashed via BCrypt
- **Logging**: Serilog, structured console output
- **API docs**: Scalar (OpenAPI-based interactive documentation)
- **Testing**: xUnit, with integration tests running against a real, isolated test database
- **Containerization**: Docker, Docker Compose

## Key Features

- **Multi-tenancy**: every job and user belongs to an organization. A valid login only ever grants access to that organization's own jobs, enforced on every request, not just assumed by the client.
- **Background job processing**: submitting a job returns immediately; the actual work happens separately, tracked through a clear lifecycle (Pending -> Processing -> Succeeded or Failed).
- **Automatic retries with backoff**: a failed job is retried a few times with increasing delays between attempts before it's given up on and marked as failed.
- **Idempotency keys**: a client can attach a unique key to a job submission. Submitting the same key twice (for example, after a network retry) returns the original job instead of creating a duplicate.
- **JWT authentication**: registration and login issue and validate signed tokens; protected endpoints reject requests with no token, an invalid token, or an expired one.

## Architecture

One app, one database. Postgres stores two kinds of things: the application's own data (users, organizations, jobs) through Entity Framework Core, and Hangfire's own internal job-queue state, in a separate schema it manages automatically. The app and database run as separate containers on one Docker Compose network, with the app reaching Postgres by its service name rather than `localhost`.

## Testing

```bash
dotnet test
```

Tests run as real HTTP requests against a fully running copy of the app (via `WebApplicationFactory`), pointed at a separate `job_forge_test` database so they never touch real data. That test database is created automatically the first time the Postgres container starts.

Running the tests requires the .NET 10 SDK installed locally (the containerized app only ships the runtime, which is sufficient for running the API but not for building and running the test project).

## Environment Variables

See `.env.example` for the full list of variables the app expects, with placeholder values and guidance for each.