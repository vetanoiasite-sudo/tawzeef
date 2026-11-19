# Tawzeef (توظيف)

Multi-tenant employment platform for church employment offices — mediating between job seekers
and employers rather than an open job board. See [docs/README.md](docs/README.md) for the full
planning documentation (business requirements, RBAC, database, API, workflows, testing, UI/UX).

## Status

Auth, Offices, Job Seeker/Employer profile + office review, office staff accounts, Complaints,
Job Advertisements + Applications + public job search, Interviews + reports, Suggestions, and
Notifications are built end-to-end (backend + frontend), verified via curl and in the browser.
Still missing: Dashboards, AuditLog viewing, PlatformSettings, and a way for office staff to
actually view/download a Job Seeker's or Employer's uploaded document (only approve/reject by id
exists today — a real gap, not a deferred decision).

## Repository layout

```
backend/    .NET 9 solution — Api / Application / Domain / Infrastructure (+ tests)
frontend/   React + Vite + TypeScript app
assets/     Brand assets (logo SVGs, see assets/brand/README.md)
docs/       Planning documentation — read docs/README.md first
```

## Backend

### First-time setup (every machine, not just the first one)

Three things are **deliberately not committed to git** (BACKEND_GUIDELINES.md §14: no secrets in
`appsettings.json`, ever) and must be set up once per machine before `dotnet run` works:

1. **SQL Server LocalDB** — install it (ships with Visual Studio, or standalone via the
   [SQL Server Express LocalDB installer](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb)).
   Windows/LocalDB only; swap `ConnectionStrings:DefaultConnection` in
   `appsettings.Development.json` for a real SQL Server/Docker instance on macOS/Linux.
2. **JWT signing key**, via .NET user-secrets (never appsettings, not even
   `appsettings.Development.json` — it's still committed to git):
   ```bash
   cd backend/src/Tawzeef.Api
   dotnet user-secrets init   # no-op if UserSecretsId is already in the .csproj
   dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 48)"
   ```
   (No `openssl`? Any random string of 32+ bytes works — `dotnet user-secrets set "Jwt:Key" "paste-a-long-random-string-here"`.)
   Without this, the app refuses to start with `Jwt:Key is not configured` — that guard is
   intentional (BACKEND_GUIDELINES.md §14), not a bug; it means "you haven't done step 2 yet."
3. **Create/update the database:**
   ```bash
   cd backend/src/Tawzeef.Api
   dotnet ef database update --project ../Tawzeef.Infrastructure --startup-project .
   ```
4. **Seed the first Super Admin** (the database starts empty — no office, no users; the test
   accounts used during development live only in the original dev machine's LocalDB file, never
   committed to git):
   ```bash
   dotnet run -- seed-superadmin 01000000000 "SomeStrongPassword123!" "Your Name"
   ```
   From there: log in as that Super Admin → create an office (`POST /api/offices`) → invite
   yourself as that office's `OfficeAdmin` (`POST /api/office-users/invite`) → accept the invite
   via OTP (logged to the console by the dev-only `DevLogOtpSender` — search the terminal output
   for `[DEV OTP]`) to get a real OfficeAdmin login.

Then the usual:
```bash
cd backend
dotnet build
dotnet test
dotnet run --project src/Tawzeef.Api
```

**Uploaded documents** (CVs, IDs, legal docs) are written to disk at `App_Data/secure-files`
relative to `Tawzeef.Api`'s own project folder by default — never inside `wwwroot` (that's served
publicly by static-file middleware, which this app doesn't even enable; see
`BUSINESS_REQUIREMENTS.md §17.3` on why these must never be publicly reachable), and never inside
`bin/`, which `dotnet clean` wipes. Override the location for a real deployment with
`FileStorage:LocalPath` in configuration (an absolute path outside the repo/deploy tree — a mounted
volume, a network share).

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
`eslint-plugin-boundaries` also prints (non-failing) deprecation warnings for its `rules`/
`element-types` options on every lint run — it still enforces the `app → feature → shared` rule
correctly on the old API, but its v7 config shape isn't verified here yet; migrate
`eslint.config.js` to `policies`/`dependencies` once that's confirmed rather than guessing at it.
