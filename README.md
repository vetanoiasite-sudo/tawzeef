# Tawzeef (توظيف)

Multi-tenant employment platform for church employment offices — mediating between job seekers
and employers rather than an open job board. See [docs/README.md](docs/README.md) for the full
planning documentation (business requirements, RBAC, database, API, workflows, testing, UI/UX).

## Status

Foundation scaffold only — no business features yet. Backend solution and frontend app are wired
up and build cleanly; Phase 2 Foundation (auth, RBAC, tenant resolution, office management, audit
logging, secure file storage, shared UI) is the next step.

## Repository layout

```
backend/    .NET 9 solution — Api / Application / Domain / Infrastructure (+ tests)
frontend/   React + Vite + TypeScript app
assets/     Brand assets (logo SVGs, see assets/brand/README.md)
docs/       Planning documentation — read docs/README.md first
```

## Backend

```bash
cd backend
dotnet build
dotnet test
dotnet run --project src/Tawzeef.Api
```

Conventions: [docs/BACKEND_GUIDELINES.md](docs/BACKEND_GUIDELINES.md). `Api → Application →
Domain`, `Infrastructure → Domain`; `Application` never references `Infrastructure` or EF Core.

## Frontend

```bash
cd frontend
npm install
npm run dev
```

| Script | Purpose |
|---|---|
| `npm run dev` | Dev server |
| `npm run build` | Typecheck + production build |
| `npm run typecheck` | TypeScript only |
| `npm run lint` | ESLint (zero warnings) |
| `npm run format` / `format:check` | Prettier |

Conventions: [docs/PROJECT_ARCHITECTURE_FRONTEND.md](docs/PROJECT_ARCHITECTURE_FRONTEND.md).
`app → features → shared`; path alias `@/` → `src/`.

**Known tooling constraint:** `eslint-plugin-jsx-a11y` and `typescript-eslint` have not yet caught
up to the latest major versions of ESLint (10) and TypeScript (7 / "tsgo") at time of scaffolding.
`frontend/package.json` pins `eslint@^9` and `typescript@^5.7` so the full type-checked + a11y +
boundaries lint setup works. Revisit this pin once those plugins publish compatible releases.
