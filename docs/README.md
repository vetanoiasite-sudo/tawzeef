# Documentation Index — Church Employment Office Platform

Planning-stage documentation, prepared before any code exists in this repository. Read in this
order:

| # | Document | Priority (as requested) | Status |
|---|---|---|---|
| 0a | [BUSINESS_REQUIREMENTS.md](./BUSINESS_REQUIREMENTS.md) | — (source) | Product owner's PRD, copied in verbatim. The single source of truth for business rules. |
| 0b | [PROJECT_ARCHITECTURE_FRONTEND.md](./PROJECT_ARCHITECTURE_FRONTEND.md) | — (source) | React/Vite frontend architecture baseline, copied in verbatim. |
| 0c | [BACKEND_GUIDELINES.md](./BACKEND_GUIDELINES.md) | — (source) | .NET backend architecture baseline, copied in verbatim. |
| 1 | [RBAC_PERMISSIONS.md](./RBAC_PERMISSIONS.md) | ضروري (required) | Reviewed 2026-11-18 — role/permission matrix, tenant & payment-gating scoping rules. |
| 2 | [DATABASE_DOCUMENTATION.md](./DATABASE_DOCUMENTATION.md) | ضروري (required) | Reviewed 2026-11-18 — ERD, table-by-table schema, integrity rules. |
| 3 | [API_DOCUMENTATION.md](./API_DOCUMENTATION.md) | ضروري (required) | Reviewed 2026-11-18 — endpoint inventory, request/response shapes, error codes. |
| 4 | [WORKFLOWS_AND_STATE_MACHINES.md](./WORKFLOWS_AND_STATE_MACHINES.md) | مهم جداً (very important) | Reviewed 2026-11-18 — state machines per entity + the 10 end-to-end process diagrams required by BUSINESS_REQUIREMENTS.md §22. |
| 5 | [TESTING_DOCUMENTATION.md](./TESTING_DOCUMENTATION.md) | مهم جداً (very important) | Reviewed 2026-11-18 — test layers, acceptance test catalogue, E2E smoke suite, Definition of Done. |
| 6 | *(removed)* `UI_UX_GUIDELINES.md` | لاحقاً (later) | Design system is now the Vitamin (Decathlon) design-system package, re-themed to the Tawzeef indigo brand `#1A23C3`. Status color mapping lives in `frontend/src/shared/ui/statusTags.ts`; palette tokens in `frontend/src/app/styles/globals.css`. |
| 7 | `DEPLOYMENT_DOCUMENTATION.md` | لاحقاً (later) | **Not drafted yet** — deliberately deferred per the requested priority order. Ask for it once the other six are confirmed. |

## Decision review (2026-11-18)

BUSINESS_REQUIREMENTS.md §24 originally listed ~60 open product decisions (`[قرار مطلوب]`). These
were walked through with the product owner question-by-question and the confirmed answers are
recorded in **RBAC_PERMISSIONS.md §0** — the single source of truth for what was decided and why.
Every other document's inline notes ("Confirmed, §0 item N") point back to that table rather than
repeating the reasoning. Decisions covered: tenancy (one office per account), Super Admin's privacy
boundary, mobile-number login + OTP, auto-re-review on editing approved data, the 15-day
advertisement lifecycle (approve-then-pay, manual EGP payment, no refunds, publish-time anchor,
pause/edit/renewal rules), direct Employer-to-applicant contact post-payment, withdraw/re-apply,
who writes each interview/hiring report, the 3-strike complaint-enforcement rule (valid-only,
cumulative, `OfficeAdmin`-only decisions, suspension = hidden-from-employers not login-block),
in-app-only notifications, Arabic-only/RTL, and permanent data retention.

**What's still genuinely open** (narrower than before — see DATABASE_DOCUMENTATION.md §8 for the
authoritative list): exact mandatory field lists on a few forms, exact field lists on the two
interview/follow-up reports, and whether a suspended Job Seeker or blocked company gets a formal
appeal path. Treat anything in a document still marked "open" or "TBD" as a real question, not an
oversight.

## Cross-reference map

```
BUSINESS_REQUIREMENTS.md  ──requirements──▶  RBAC_PERMISSIONS.md
                           ──requirements──▶  DATABASE_DOCUMENTATION.md
                           ──requirements──▶  WORKFLOWS_AND_STATE_MACHINES.md

RBAC_PERMISSIONS.md       ──who can call it──▶  API_DOCUMENTATION.md
DATABASE_DOCUMENTATION.md ──shapes the DTOs──▶  API_DOCUMENTATION.md
WORKFLOWS_AND_STATE_MACHINES.md ──drives the status fields──▶  API_DOCUMENTATION.md, statusTags.ts (frontend)

API_DOCUMENTATION.md + RBAC_PERMISSIONS.md + WORKFLOWS_AND_STATE_MACHINES.md
                           ──acceptance criteria──▶  TESTING_DOCUMENTATION.md

PROJECT_ARCHITECTURE_FRONTEND.md ──component/state conventions──▶  Vitamin (@vtmn/css) + globals.css tokens
BACKEND_GUIDELINES.md            ──service/controller conventions──▶  API_DOCUMENTATION.md, DATABASE_DOCUMENTATION.md
```
