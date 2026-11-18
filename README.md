# Tawzeef (توظيف)

Multi-tenant employment platform for church employment offices — mediating between job seekers
and employers rather than an open job board. See [docs/README.md](docs/README.md) for the full
planning documentation (business requirements, RBAC, database, API, workflows, testing, UI/UX).

## Status

No business features yet. Backend Phase 2 Foundation (auth, RBAC, tenant model, audit logging,
secure file storage) is done and verified end-to-end (migration applied, `/health` + Swagger
checked). Frontend Phase 2 Foundation (providers, router, theme, auth store) is next.

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

### Where the built code deviates from `docs/BACKEND_GUIDELINES.md`

That document is a reusable baseline (its own closing line says to update it "when the baseline
changes — not when a single project needs an exception"), so project-specific resolutions belong
here instead, not edited into it.

| The guide's literal sample | What's actually built | Why |
|---|---|---|
| `Application/Abstractions/ICurrentUser.cs` | `Domain/Contracts/ICurrentUser.cs` | `Infrastructure`'s `AuditStampingInterceptor` needs it, and the guide's own non-negotiable #1 ("`Infrastructure → Domain`", nothing points up) means `Infrastructure` can't reference `Application`. `Domain` is the one layer both may depend on. |
| `Application/Abstractions/IFileStorage.cs` | `Domain/Contracts/IFileStorage.cs` | Same reason — `Infrastructure`'s `LocalFileStorage` implements it. |
| `Application/Common/PagedResult.cs` | `Domain/Common/PagedResult.cs` | `IBaseRepository<T>.PageAsync` (in `Domain/Contracts`, per the guide's own tree) returns it, so it can't live one layer up. |
| `AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` | `AppDbContext : IdentityUserContext<ApplicationUser, Guid>` (no Identity role tables) | RBAC here is a custom, data-driven permission model (`UserOfficeMembership.Role` + `StaffPermissionGrant`), not ASP.NET Identity's Role/UserRole/RoleClaims tables — those would just sit empty. |

Everything else (controllers, `ServiceResult`, the permission filter + startup audit, the audit
interceptor, naming) follows the guide as written.

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
