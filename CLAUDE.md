# Svastha

Training issue assessment app. Personal use, public repo.

## Stack — locked
- .NET Core API, Dapper, DbUp (raw SQL migrations)
- Postgres + pgvector
- Vite + React + TypeScript, static PWA
- Deploy: Fly.io (API), Cloudflare Pages (frontend)

## Hard rules
- NEVER use EF Core. Dapper only. No DbContext, no LINQ-to-SQL,
  no scaffolded entities.
- All SQL is hand-written, in code or in migration files.
- Migrations are numbered .sql files run by DbUp from a separate
  console app. Never run migrations on API startup.
- No ORM of any kind beyond Dapper's mapping.
- No Next.js. No React Native. No NoSQL.

## Conventions
- Namespaces: Svastha.Api, Svastha.Migrations, Svastha.Tests
- DB connection comes from env var SVASTHA_DB, with a localhost
  fallback for dev.
- Table and column names: snake_case.
- Domain names: Complaint, Assessment. Not "triage".

## Layout
- api/            .NET backend
  - Svastha.slnx  solution
  - src/Svastha.Api/         minimal API + Dockerfile
  - src/Svastha.Migrations/  DbUp console app
  - tests/Svastha.Tests/
- client/         Vite + React + TS frontend
- docker-compose.yml  db (pgvector/pg16) + api

## Commands
- Build: dotnet build api/Svastha.slnx
- Migrate: dotnet run --project api/src/Svastha.Migrations
- Frontend: cd client && npm run dev

## Ports
- API 5036, Vite 5173, Postgres 5432

## Working style
- Small steps. Do not scaffold ahead of what was asked.
- Do not add packages that were not requested.
