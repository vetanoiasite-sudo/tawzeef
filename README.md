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

This repo is the **frontend only** — split from a combined repo on 2026-11-26 (full git history
for each side was preserved independently via `git subtree split`). The backend lives in a sibling
repo/folder, `Tawzeef-Api` (.NET 9), cloned next to this one — see
[its README](../Tawzeef-Api/README.md) for backend setup.

```
frontend/   React + Vite + TypeScript app
assets/     Brand assets (logo SVGs, see assets/brand/README.md)
docs/       Planning documentation — read docs/README.md first (covers both repos for now)
```

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

### Serving the built frontend from the backend (single process)

`npm run build` writes straight into the sibling `Tawzeef-Api` repo's `src/Tawzeef.Api/wwwroot`
(see `vite.config.ts` — `../../Tawzeef-Api/...`, so `Tawzeef-Api` must be cloned next to this repo,
both under the same parent folder), and `Program.cs` there serves it with `UseStaticFiles()` + a
`MapFallbackToFile("index.html")` SPA fallback (so a direct hit on e.g. `/jobs/123` still works, not
just in-app navigation). So instead of running two servers for a production-style check:

```bash
cd frontend && npm run build
cd ../../Tawzeef-Api && dotnet run --project src/Tawzeef.Api
```

then open `http://localhost:5280` — both the UI and the API come from that one process. The build
targets `VITE_API_BASE_URL=same-origin` (`.env.production`), which resolves to
`window.location.origin` at runtime instead of a fixed URL, so this works on whatever host/port the
app is actually deployed to. `wwwroot` is committed in the `Tawzeef-Api` repo on purpose (not
git-ignored there), so a fresh clone of it runs immediately without a frontend build step; local dev
with hot reload still uses the separate `npm run dev` Vite server on `:5173` against the backend on
`:5280` (CORS, `.env.development`), exactly as before the split.

**Known tooling constraint:** `eslint-plugin-jsx-a11y` and `typescript-eslint` have not yet caught
up to the latest major versions of ESLint (10) and TypeScript (7 / "tsgo") at time of scaffolding.
`frontend/package.json` pins `eslint@^9` and `typescript@^5.7` so the full type-checked + a11y +
boundaries lint setup works. Revisit this pin once those plugins publish compatible releases.
`eslint-plugin-boundaries` also prints (non-failing) deprecation warnings for its `rules`/
`element-types` options on every lint run — it still enforces the `app → feature → shared` rule
correctly on the old API, but its v7 config shape isn't verified here yet; migrate
`eslint.config.js` to `policies`/`dependencies` once that's confirmed rather than guessing at it.
