# React Project Architecture

> **Purpose.** A reusable blueprint for the **structure and architecture** of a React single-page application: layers, folders, file responsibilities, dependency rules, conventions and quality gates.
>
> **Scope.** This document is domain-agnostic. It contains no business rules, no domain entities and no implementation logic. Feature names such as `<feature>` or `example-feature` are placeholders.
>
> **Audience.** Developers and AI coding agents creating a new React project or extending one that follows this blueprint.

---

## Table of Contents

1. [Architectural Principles](#1-architectural-principles)
2. [Technology Stack](#2-technology-stack)
3. [Repository Structure](#3-repository-structure)
4. [Source Structure (`src/`)](#4-source-structure-src)
5. [Feature Module Structure](#5-feature-module-structure)
6. [Layer Responsibilities](#6-layer-responsibilities)
7. [Dependency Rules](#7-dependency-rules)
8. [Naming Conventions](#8-naming-conventions)
9. [Coding Standards](#9-coding-standards)
10. [Component Architecture](#10-component-architecture)
11. [State Architecture](#11-state-architecture)
12. [Data Layer Architecture](#12-data-layer-architecture)
13. [Routing Architecture](#13-routing-architecture)
14. [Forms Architecture](#14-forms-architecture)
15. [Styling and Theming Architecture](#15-styling-and-theming-architecture)
16. [Internationalization Architecture](#16-internationalization-architecture)
17. [Performance Guidelines](#17-performance-guidelines)
18. [Accessibility and Security Standards](#18-accessibility-and-security-standards)
19. [Testing Architecture and Quality Gates](#19-testing-architecture-and-quality-gates)
20. [Build, Configuration and Deployment](#20-build-configuration-and-deployment)
21. [Anti-Patterns to Avoid](#21-anti-patterns-to-avoid)
22. [New Project Setup Checklist](#22-new-project-setup-checklist)
23. [Instructions for AI Coding Agents](#23-instructions-for-ai-coding-agents)

---

## 1. Architectural Principles

These principles decide every ambiguous case.

1. **Colocate by feature, share by necessity.** Code lives in the feature that uses it. It moves to `shared/` only when a second feature needs it and it carries no feature-specific knowledge.
2. **One source of truth per fact.** Each route, permission, design token, environment value, translation key and cache key is defined once and derived everywhere else.
3. **State lives where it belongs.** Server data → query cache. Shareable view state → URL. Form drafts → form library. Ephemeral UI → component state. Truly global client state → a small store.
4. **Dependencies point one way.** `app → features → shared`. Never the reverse. Enforced by lint.
5. **Load only what is needed.** Routes are lazy by default; heavy libraries load on demand.
6. **Types describe reality.** Strict TypeScript, schema-validated boundaries, no `any`.
7. **Fail loudly in development, gracefully in production.** Validate configuration at startup; error boundaries per route.
8. **Accessibility and localization are built into shared building blocks**, so features inherit them.
9. **Every dependency has a stated reason.**
10. **Delete, don't comment out.** Version control is the archive.

---

## 2. Technology Stack

Defaults suit a client-rendered application (dashboards, back-offices, internal tools). Change a row only with a written reason.

| Concern | Default | Alternative / when |
|---|---|---|
| Language | TypeScript, `strict` | — |
| Build tool | Vite | Next.js or React Router framework mode only when SSR/SEO is required |
| React plugin | `@vitejs/plugin-react` (enables React Compiler) | `@vitejs/plugin-react-swc` for faster builds without the compiler |
| Routing | React Router (data mode) | TanStack Router for fully typed params/search params |
| Server state | TanStack Query | RTK Query when Redux is already required |
| Client state | React state / Context; Zustand when a global store is justified | Redux Toolkit for large, event-heavy client state |
| HTTP | Native `fetch` behind one client module | `ky` for built-in retries/hooks |
| Forms | React Hook Form | — |
| Validation | Zod | Valibot when bundle size is critical |
| UI kit | One kit per app (e.g. Ant Design for data-dense apps; shadcn/ui + Radix for custom-branded apps) | — |
| Styling | Tailwind CSS + UI-kit theme tokens | CSS Modules |
| Icons | One icon set | — |
| Notifications | UI kit's built-in system | A single dedicated library if the kit has none |
| i18n | i18next + react-i18next | Lingui |
| Dates | One library (dayjs or date-fns) | — |
| Unit / component tests | Vitest + React Testing Library + user-event | — |
| API mocking | MSW | — |
| E2E tests | Playwright | Cypress |
| Lint | ESLint flat config + typescript-eslint (type-checked) + react-hooks + jsx-a11y + boundaries | Biome (fewer plugins) |
| Format | Prettier | Biome |
| Git hooks | lefthook or husky + lint-staged | — |

**Dependency rule:** add a library only when the platform or an existing dependency cannot reasonably do the job, it is maintained, its bundle cost is acceptable for the route using it, and the reason is recorded in the pull request.

---

## 3. Repository Structure

```
project-root/
├─ .github/workflows/        # CI pipeline (quality gates, section 19)
├─ .vscode/                  # recommended extensions and workspace settings
├─ e2e/                      # end-to-end specs and fixtures
├─ public/                   # files copied verbatim to the build root (favicon, robots.txt, host config)
├─ scripts/                  # repository scripts (i18n key check, size budget, codegen)
├─ src/                      # application source (section 4)
├─ .env.example              # every variable documented, no secrets
├─ .env.development          # committed, non-secret defaults per mode
├─ .env.staging
├─ .env.production
├─ .gitignore                # *.local, dist, coverage, test reports
├─ .prettierrc
├─ eslint.config.js
├─ index.html
├─ package.json
├─ playwright.config.ts
├─ PROJECT_ARCHITECTURE.md   # this document
├─ README.md                 # how to install, run, test, build, deploy
├─ tsconfig.json             # references app + node configs
├─ tsconfig.app.json
├─ tsconfig.node.json
└─ vite.config.ts
```

---

## 4. Source Structure (`src/`)

```
src/
├─ main.tsx                      # entry: validate config, init i18n, render <App/>
│
├─ app/                          # COMPOSITION LAYER
│  ├─ App.tsx                    # providers + router
│  ├─ providers/                 # one file per provider + AppProviders.tsx that nests them
│  ├─ router/
│  │  ├─ router.tsx              # creates the router from the route registry
│  │  ├─ routeRegistry.ts        # single list of routes (path, lazy page, access rule, nav metadata)
│  │  ├─ guards/                 # authentication / authorization / guest-only route wrappers
│  │  └─ RouteErrorBoundary.tsx  # errorElement for route segments
│  ├─ layouts/                   # application shells (main layout, auth layout)
│  └─ styles/
│     ├─ globals.css             # CSS entry, layers, base styles
│     └─ theme.ts                # design tokens (single source for UI kit + CSS)
│
├─ features/                     # FEATURE LAYER — one folder per capability (section 5)
│  ├─ <feature-a>/
│  └─ <feature-b>/
│
├─ shared/                       # FOUNDATION LAYER — feature-agnostic
│  ├─ api/                       # HTTP client, error type, query client config, generic response types
│  ├─ auth/                      # session store, access-control primitives (<Can>, useAccess)
│  ├─ config/                    # validated environment configuration
│  ├─ i18n/                      # i18n setup and common translations
│  ├─ ui/                        # generic presentational components (one folder per component)
│  ├─ hooks/                     # generic hooks (debounce, URL state, disclosure)
│  ├─ lib/                       # pure framework-free utilities (format, object, file, storage)
│  └─ types/                     # global declarations and generic utility types
│
├─ assets/                       # fonts, images, icons imported by code
│
└─ test/                         # test infrastructure only
   ├─ setup.ts                   # global test setup
   ├─ msw/                       # mock server + handlers (one file per feature)
   └─ renderWithProviders.tsx    # render helper with app providers
```

---

## 5. Feature Module Structure

Every feature has the same shape. Create only the folders a feature needs.

```
features/<feature>/
├─ api/
│  ├─ <feature>.api.ts           # request functions (no React)
│  ├─ <feature>.mapper.ts        # transport DTO ⇄ UI model conversion
│  ├─ <feature>.keys.ts          # cache-key factory
│  └─ <feature>.queries.ts       # query and mutation hooks
├─ components/                   # components used only by this feature
│  └─ <LargeComponent>/          # a folder when a component has sub-parts
├─ hooks/                        # feature-only hooks
├─ pages/                        # route components (lazy-loaded, leaves of the import graph)
├─ schemas/                      # validation schemas
├─ types/                        # feature types (inferred from schemas where possible)
├─ utils/                        # feature-only pure helpers
├─ i18n/                         # optional feature translation namespace
├─ constants.ts
├─ <feature>.routes.ts           # route objects contributed to the route registry
└─ index.ts                      # PUBLIC API — the only file other layers may import
```

**Public API (`index.ts`)** exports only what other parts of the app need: route objects, selected components, selected hooks, public types. Keep it small and free of heavy imports.

---

## 6. Layer Responsibilities

| Layer | Contains | Must not contain |
|---|---|---|
| `app/` | Providers, router, guards, layouts, global styles, startup wiring | Feature logic, request definitions, reusable UI primitives |
| `features/` | Everything specific to one capability: pages, components, data access, schemas, types, routes, translations | Imports of another feature's internals; generic utilities |
| `shared/` | Feature-agnostic building blocks | Any knowledge of a specific feature |
| `assets/` | Static files | Code |
| `test/` | Test infrastructure | Test cases (those sit next to the code they test) |

### Responsibilities inside a feature

| File type | Responsibility | Never does |
|---|---|---|
| `*.api.ts` | Build requests, call the HTTP client, map DTOs | Import React, show UI feedback |
| `*.mapper.ts` | Convert transport shapes ⇄ UI shapes | Perform requests |
| `*.keys.ts` | Define cache keys | Anything else |
| `*.queries.ts` | Bind request functions to the query cache; invalidation | Render UI |
| `pages/*` | Read route/URL state, call hooks, compose components | Contain reusable sub-components |
| `components/*` | Render feature UI | Call `fetch` directly |
| `schemas/*` | Validation rules | Side effects |

---

## 7. Dependency Rules

```
   app  ──►  features  ──►  shared  ──►  third-party
```

1. **No upward imports.** `shared` never imports `features` or `app`; `features` never import `app`.
2. **Public API only.** Outside a feature, import from `@/features/<feature>` — never from its internal folders.
3. **No circular feature dependencies.** Extract the common part to `shared/` or compose at the page/app level.
4. **Pages are leaves.** Only route definitions import pages.
5. **Data-access files are React-free.** `*.api.ts` never imports React.
6. **One network entry point.** Only `shared/api` calls `fetch`.
7. **One session entry point.** Only `shared/auth` reads or writes session storage.
8. **Cross-feature composition order:** (a) a page composes public components of two features; (b) a feature uses another feature's exported hook; (c) promote to `shared/` if three or more features need it and it is feature-agnostic.

### Enforcement

- `eslint-plugin-boundaries` (element types `app`, `feature`, `shared`) or `import/no-restricted-paths` zones.
- `no-restricted-imports` banning `@/features/*/*` from outside a feature.

### Path alias

One alias, `@/` → `src/`, configured in both `tsconfig.app.json` (`paths`) and `vite.config.ts` (`resolve.alias`). Relative imports only within the same folder subtree; no `../../../`.

---

## 8. Naming Conventions

| Thing | Convention | Example |
|---|---|---|
| Feature and multi-word folders | `kebab-case` | `features/example-feature/` |
| Component folders | `PascalCase` | `shared/ui/DataTable/` |
| Component files | `PascalCase.tsx`, one exported component, name = file name | `ExampleCard.tsx` |
| Hooks | `useCamelCase.ts` | `useDebouncedValue.ts` |
| Other modules | `camelCase.ts` with role suffix | `example.api.ts`, `example.keys.ts`, `example.schema.ts` |
| Types | `PascalCase`, no `I` prefix | `ExampleItem`, `ExampleFormValues` |
| Props types | `<Component>Props` | `ExampleCardProps` |
| Constants | `UPPER_SNAKE_CASE`; `as const` objects for enumerations | `PAGE_SIZE_OPTIONS` |
| Booleans | `is / has / can / should` prefix | `isOpen`, `canEdit` |
| Handlers | `handleX` internally, `onX` as props | `onSubmit={handleSubmit}` |
| Translation keys | Namespaced, `UPPER_SNAKE_CASE` leaves | `common:ACTIONS.SAVE` |
| URLs | `kebab-case`, plural resources, ids as path params | `/items/:itemId/edit` |
| Tests | `*.test.ts(x)` beside the subject; `e2e/*.spec.ts` | `format.test.ts` |

Run a spell checker (e.g. `cspell`) on identifiers and file names in CI.

---

## 9. Coding Standards

### 9.1 TypeScript

- `strict`, `noUncheckedIndexedAccess`, `noImplicitOverride`, `verbatimModuleSyntax`; `exactOptionalPropertyTypes` recommended.
- No `any`; use `unknown` and narrow. Every `as` cast or `!` assertion carries a comment.
- Derive types from one source (schema inference, `as const` objects, `ReturnType`).
- Discriminated unions instead of parallel booleans.
- Transport types and UI types are separate; convert in mappers.
- Explicit return types on exported `shared/` functions.

### 9.2 Imports

Order: third-party → `@/app` / `@/features` / `@/shared` → relative → styles/assets. Use `import type` for type-only imports. Enforce with a sorter.

### 9.3 Size limits (review triggers)

| Item | Soft | Hard |
|---|---|---|
| Component file | 200 lines | 300 lines |
| Function / hook | 40 lines | 80 lines |
| Props per component | 7 | 10 |
| JSX nesting depth | 4 | 6 |

### 9.4 Error handling architecture

- One normalized error type produced by the HTTP client for every failed request.
- Queries render inline error states with retry; global notifications only for background failures.
- Mutations report success/failure through one shared notification hook.
- Each route segment has an `errorElement`; independent widgets get local error boundaries.
- Session expiry is handled by the session store + router redirect — no full page reload.
- Never swallow errors silently. No `console.log` in committed code.

### 9.5 Comments and dead code

- Comment *why*, not *what*.
- No commented-out code.
- `TODO` comments reference a ticket.

### 9.6 Duplication vs abstraction

- Rule of three: abstract on the third repetition.
- Abstract behaviour (hooks, helpers) before markup.
- Repeated configuration becomes data in one registry.
- Wrap a library component only to add app-wide behaviour (accessibility, theming, error display).

---

## 10. Component Architecture

### 10.1 Component roles

| Role | Location | Data access |
|---|---|---|
| Page | `features/*/pages` | Yes, via feature hooks |
| Feature component | `features/*/components` | Via feature hooks when it simplifies the page; otherwise props |
| Shared UI component | `shared/ui` | Never |
| Layout | `app/layouts` | Session only |

### 10.2 Rules

1. Function components with named exports.
2. Minimal, typed props; prefer `children` and slot props over boolean flags.
3. Hooks return data and callbacks — never JSX or render functions.
4. Call `useTranslation()` where text renders; do not pass `t` through props.
5. One modal/drawer instance per page, driven by selected-item state — not one per list row.
6. Derive values during render; `useEffect` only for synchronizing with external systems.
7. Mount forms after their data is ready (`defaultValues` + `key`), instead of resetting in effects.
8. Stable ids as list keys.
9. Split large components into a container plus presentational sections.
10. No inline `<style>` tags; `style` only for truly dynamic values.
11. Every async view has loading, empty, error and success states.

### 10.3 Shared UI kit (minimum)

`PageHeader`, `DataTable`, `FormField`, `IconButton` (accessible label required), `ConfirmAction`, `SearchInput`, `StatusTag`, `EmptyState`, `ErrorState`, `LoadingState`, `Can` (access-gated rendering).

### 10.4 Standard screen patterns

| Pattern | Composition |
|---|---|
| List | Header with primary action → filter bar bound to URL → table with server pagination → row actions → one shared modal |
| Details | Header with back link and actions → sections → related content in lazy tabs |
| Create / Edit | One component for both; mode from route; schema validation; unsaved-changes prompt |
| Modal form | Small forms only; larger forms become pages |
| Dashboard | Independent widgets, each with its own query, skeleton and error boundary; heavy widgets lazy |

---

## 11. State Architecture

| State kind | Lives in | Tool |
|---|---|---|
| Server state | Query cache | TanStack Query (or RTK Query) |
| URL state (pagination, filters, sorting, tabs, ids) | Path + search params | Router + a typed `useUrlState` hook |
| Form state | Form instance | React Hook Form |
| Local UI state | Component | `useState` / `useReducer` |
| Global client state (session, theme, layout prefs) | Small store | External store module, Context, or Zustand |
| Derived state | Render | Plain expressions |
| Persisted preferences | Storage via `shared/lib/storage` | Validated on read |

Rules:

- Never copy server data into local or global state.
- Each fact has exactly one owner (e.g. current language belongs to the i18n library only).
- No global store "just in case".
- Validate anything read from persistent storage.

---

## 12. Data Layer Architecture

### 12.1 Request flow

```
Page / Component
   ▼  use<Feature>Query(params)          features/<feature>/api/<feature>.queries.ts
Query hook  — cache key, staleness, invalidation
   ▼  <feature>Api.method(params)        features/<feature>/api/<feature>.api.ts
Request function — endpoint, method, DTO mapping
   ▼  http.get / post / …                shared/api/httpClient.ts
HTTP client — base URL, auth header, response envelope, error normalization, expiry signal
   ▼
fetch
```

### 12.2 HTTP client responsibilities (`shared/api`)

1. Prefix the validated base URL (no hard-coded fallback).
2. Attach auth and standard headers.
3. Serialize bodies; pass `FormData` untouched.
4. Build query strings with `URLSearchParams`.
5. Forward `AbortSignal`.
6. Unwrap the backend response envelope, if any.
7. Normalize every error into one error type.
8. Signal session expiry on `401`.
9. Never show UI feedback itself.

### 12.3 Backend adaptation

Backend-specific conventions (envelope shapes, pagination format, id placement, field casing) are absorbed in `shared/api` (generic) or `*.api.ts` / `*.mapper.ts` (endpoint-specific). Components never see transport shapes.

### 12.4 Cache conventions

- A key factory per feature (`all → lists → list(params) → details → detail(id)`); no inline keys.
- Central defaults in `shared/api/queryClient.ts`: sensible `staleTime`, retries only for network/5xx errors.
- Keep previous data while paginating.
- Dependent queries use `enabled`.
- Mutations invalidate the narrowest affected keys.
- Optimistic updates only for low-risk changes, always with rollback.

---

## 13. Routing Architecture

### 13.1 Route registry

All routes are declared once (each feature contributes its `<feature>.routes.ts`) with: path, lazy page, access rule, and optional navigation metadata. From the registry the app derives the router tree, the navigation menu and the default landing route.

### 13.2 Route tree

```
/login                 GuestOnly → AuthLayout → login page
/                      RequireAuth → AppLayout   (errorElement on every segment)
├─ index               → redirect to first accessible route
├─ <registry routes>   RequireAccess (checks every matched route)
├─ /forbidden          → 403 page
└─ *                   → 404 page
```

### 13.3 Rules

- Every route is lazy-loaded.
- Ids in path params; filters/pagination/sorting/tabs in search params.
- Flat sibling routes for list/new/details/edit unless a true master–detail layout is needed.
- Access guards evaluate the access rule of **every** matched route, so a child can never be less protected than its parent.
- Guards only redirect — no side effects during render.
- Authentication guard preserves the target URL (`?redirect=`), validated as a same-origin relative path.
- Element-level access uses `<Can>` / `useAccess()` with permissions — never raw role-name comparisons in components.
- Frontend access control is a UX layer; the backend enforces authorization.

---

## 14. Forms Architecture

- React Hook Form + schema resolver; one schema per form in `features/*/schemas`; form type inferred from the schema.
- Validation messages are translation keys; `FormField` renders label, control, hint and error with correct accessibility wiring.
- Reusable field rules live in `shared/lib/validation.ts`.
- Server field errors map onto fields; general errors show as a form-level alert.
- Unsaved-changes prompt on page-level forms.
- Large forms: section components receiving `control`, field arrays for repeatable groups, DTO ⇄ form mapping in mappers, stepper for very long forms.

---

## 15. Styling and Theming Architecture

- **Design tokens** in `app/styles/theme.ts` feed both the UI kit theme and CSS variables. Semantic names (`primary`, `danger`, `surface`, `text-muted`).
- No raw color values in components; no `!important`; no inline `<style>` blocks.
- **Responsibilities:** utility CSS for layout, spacing, typography and responsiveness; UI-kit tokens for component internals.
- Use CSS cascade layers to order UI-kit styles and utilities predictably.
- Repeated class patterns become components, not long arbitrary-selector strings.
- Conditional classes via `clsx` / `cn`.
- Logical properties (`ms`, `me`, `ps`, `pe`, `start`, `end`, `text-start`) for direction safety.
- Responsive: mobile-first breakpoints; navigation collapses to a drawer on small screens; tables scroll horizontally or become card lists; touch targets ≥ 44 px.
- Fonts self-hosted (WOFF2), only used weights, `font-display: swap`.

---

## 16. Internationalization Architecture

- i18n initialized before first render; languages lazy-loaded.
- Namespaces: `common`, `validation`, `nav`, plus one per feature.
- One place syncs `<html lang>` and `<html dir>` at startup and on language change.
- UI kit receives direction and locale through its root provider; date library locale synchronized.
- Formatting through `Intl` helpers in `shared/lib`.
- Typed translation keys; CI script checks key parity across languages.
- Direction-dependent icons flip in RTL.

---

## 17. Performance Guidelines

| Area | Rule |
|---|---|
| Code splitting | Route-level lazy loading for every page |
| Heavy libraries | Dynamic `import()` at the route or on user action (charts, editors, PDF, calendars) |
| Bundle budget | Size check in CI; inspect with a bundle visualizer |
| Single choice | One UI kit, one icon set, one date library, one notification system |
| Barrels | Feature `index.ts` exports must not pull heavy modules into hot paths |
| Assets | Optimized SVGs, modern image formats, lazy images with explicit dimensions |
| Caching | Hashed assets `immutable`; `index.html` `no-cache` |
| Rendering | Measure before memoizing; keep fast-changing state low in the tree; virtualize long lists |
| Data | Server-side pagination/filter/sort for unbounded data; cancel stale requests |
| Monitoring | Track Core Web Vitals (LCP < 2.5 s, INP < 200 ms, CLS < 0.1) |

---

## 18. Accessibility and Security Standards

### 18.1 Accessibility (WCAG 2.2 AA)

- Every control has an accessible name (`IconButton` requires a label; `FormField` links label and input).
- Correct `lang` and `dir` on `<html>`.
- Full keyboard operability with visible focus; native interactive elements only.
- Focus trapped in modals and restored on close; moved to the page heading on navigation.
- Contrast ≥ 4.5:1 for text; information not conveyed by color alone.
- Meaningful `alt` text; `alt=""` for decorative images.
- Errors linked via `aria-describedby` and `aria-invalid`.
- Respect `prefers-reduced-motion`.
- Tooling: `eslint-plugin-jsx-a11y`, axe checks in E2E.

### 18.2 Security

- No `dangerouslySetInnerHTML` except through one sanitizing shared component.
- Prefer HttpOnly cookies for credentials; if tokens must be stored client-side, isolate access in `shared/auth` and use a strict CSP.
- Content-Security-Policy and standard security headers at the host.
- Only relative, same-origin redirect targets.
- No secrets in client environment variables — everything in the bundle is public.
- No sensitive data in URLs, logs or analytics.
- Clear caches on sign-out.
- Dependency auditing and automated updates in CI.

---

## 19. Testing Architecture and Quality Gates

### 19.1 Test layers

| Layer | Tool | Scope |
|---|---|---|
| Static | TypeScript, ESLint, Prettier, spell check | Types, boundaries, accessibility lint, formatting |
| Unit | Vitest | `shared/lib`, mappers, schemas, key factories, access rules |
| Component / integration | Vitest + Testing Library + MSW | Features as users experience them, network mocked |
| End-to-end | Playwright (+ axe) | Smoke paths, access per role, both text directions |
| Visual (optional) | Screenshots / Storybook | Shared UI kit |

### 19.2 Rules

- Test behaviour, not implementation; query by role, label or text.
- Mock the network, not hooks.
- Fresh query client per test, retries off.
- Every bug fix includes a regression test.
- A generated test asserts access for every role × route in the registry.

### 19.3 Quality gates (required to merge)

1. Install from lockfile
2. Typecheck
3. Lint with zero warnings
4. Format check
5. Tests with coverage thresholds
6. Build for each target mode
7. Bundle-size budget
8. Translation key parity
9. E2E smoke
10. Dependency audit

### 19.4 Definition of Done

- [ ] Layer and dependency rules respected
- [ ] No unjustified new dependency
- [ ] Loading, empty, error, success states present
- [ ] Works in every supported language and direction, on narrow and wide screens
- [ ] Keyboard-operable, labelled controls
- [ ] Access rules on routes and actions
- [ ] Tests added or updated; CI green
- [ ] No commented-out code, `console.log`, hard-coded colors or user-facing strings
- [ ] Bundle budget respected

---

## 20. Build, Configuration and Deployment

- **Scripts:** `dev`, `build`, `build:<mode>`, `preview`, `typecheck`, `lint`, `format`, `format:check`, `test`, `test:watch`, `e2e`, `e2e:smoke`, `i18n:check`, `analyze`.
- **Modes:** `development`, `staging`, `production` via `.env.<mode>`; local overrides in git-ignored `.env.<mode>.local`.
- **Configuration validated at startup** in `shared/config` and exposed as one typed object; app refuses to start with invalid config.
- **Environment badge** visible in non-production builds.
- **Build once per environment** (values inlined), or load a runtime config file if one artifact must be promoted across environments.
- **TypeScript:** bundler resolution, strict flags (section 9.1), path alias.
- **ESLint:** type-checked presets, `react-hooks` as errors, `jsx-a11y`, boundaries, `no-console`, floating-promise and exhaustive-switch rules.
- **Hosting (static SPA):** fallback of unknown paths to `index.html`; `index.html` not cached; hashed assets cached long-term; security headers.
- **CI/CD:** artifacts built in CI; documented branch → environment mapping; keep previous build for rollback; publish assets before `index.html`; reload once on stale-chunk load errors.

---

## 21. Anti-Patterns to Avoid

| Anti-pattern | Do instead |
|---|---|
| Eagerly importing every page | Lazy routes |
| Folders by file type (`pages/`, `apis/`, `types/` at top level) | Feature folders |
| One global types file | Types inside their feature |
| Guard checking only the deepest matched route | Check every match |
| Same route list in router, menu and helpers | One route registry |
| Direct storage access across components | Session module |
| Full page reload on session expiry | Store event + router redirect |
| Side effects during render | Effects or event handlers |
| Global toast for every failed read | Inline error states with retry |
| Ad-hoc parsing of error payloads | One normalized error type |
| Passing `t` as a prop | `useTranslation()` locally |
| Hooks returning JSX or render functions | Components + data hooks |
| A modal per list row | One modal per page |
| Effects copying fetched data into forms | Mount form with `defaultValues` |
| Choosing UI by `pathname.includes()` | Real routes |
| Resource ids in query strings | Path params |
| Role-name checks in components | Permissions + `<Can>` |
| Duplicate create/edit components | One component, mode from route |
| `!important` utilities fighting the UI kit | Cascade layers |
| Long arbitrary-selector class strings | Shared components |
| Hard-coded colors | Tokens |
| Multiple icon sets / notification systems | One of each |
| Duplicate state owners | One owner per fact |
| Unused dependencies or plugins | Remove |
| Hard-coded fallback API host | Validated config |
| Commented-out code | Delete |
| Premature generic "config-driven" pages | Compose small shared parts |
| Context for high-frequency state | Local state or selector-based store |
| Array index as key in dynamic lists | Stable ids |

---

## 22. New Project Setup Checklist

### Foundation
- [ ] Scaffold with Vite React + TypeScript template
- [ ] Commit lockfile; pin Node version (`.nvmrc`, `engines`)
- [ ] Configure tsconfig strict flags and `@/` alias (TS + Vite)
- [ ] ESLint (type-checked, hooks, a11y, boundaries, no-console), Prettier, spell check
- [ ] Pre-commit hook; CI workflow with all quality gates
- [ ] Project README

### Structure
- [ ] Create `app/`, `features/`, `shared/`, `assets/`, `test/` (section 4)
- [ ] Add this document to the repository root

### Core modules
- [ ] `shared/config` validated environment + `.env.example`
- [ ] `shared/api` HTTP client, error type, query client, generic types
- [ ] `shared/auth` session store, access primitives, `<Can>`
- [ ] `shared/i18n` setup, lazy languages, `lang`/`dir` sync, key parity script
- [ ] `app/styles` tokens wired to UI kit and CSS; cascade layers; self-hosted fonts
- [ ] `app/providers` composed in one `AppProviders`
- [ ] `app/router` registry, guards (all-match access check), error boundary, 403/404
- [ ] `app/layouts` shell with navigation generated from the registry

### Shared UI
- [ ] Minimum kit from section 10.3
- [ ] Generic hooks: debounce, URL state, disclosure, notifications

### Reference feature
- [ ] Authentication feature (login, redirect handling)
- [ ] One complete feature following section 5, passing section 19.4 — the template for all others

### Operations
- [ ] Host config: SPA fallback, cache and security headers
- [ ] Error monitoring and Web Vitals (recommended)
- [ ] Bundle budget
- [ ] Automated dependency updates

---

## 23. Instructions for AI Coding Agents

These rules are mandatory for any agent creating or modifying a project that follows this document.

### Before changing code
1. Read this document before the first change in a session; re-read the relevant section before working in a new area.
2. Read the project README and any agent instruction files; explicit project overrides win.
3. Search `shared/` and the target feature for an existing component, hook, helper, schema, cache key or translation key before creating one.
4. Mirror the structure of the most complete existing feature.
5. State the plan (files and their layer) before large changes.

### While changing code
6. Respect layer and dependency rules (sections 6–7).
7. Place and name files per sections 4, 5 and 8.
8. Do not add dependencies without a stated, necessary reason; load heavy ones lazily.
9. Register new routes in the route registry, lazy-loaded, with an access rule.
10. Put every user-facing string in i18n for all languages; keep layouts direction-safe.
11. Implement loading, empty, error and success states.
12. Use the forms architecture (section 14).
13. Keep server data in the query cache and view state in the URL.
14. Use design tokens only.
15. Meet the accessibility and security standards (section 18).
16. Keep changes focused; no unrelated refactors or reformatting.
17. No commented-out code, `console.log`, `any`, unexplained casts, or disabled lint rules without justification.
18. Prefer simple code; abstract on the third repetition.

### Before declaring done
19. Run typecheck, lint (zero warnings), tests, build and i18n check; report any that could not run.
20. Add or update tests; add a regression test for each bug fix.
21. For UI changes, verify in the running app: all languages and directions, narrow and wide viewports, keyboard use, and a role that must be denied.
22. Check bundle impact when adding a dependency or route.
23. Report what changed, what was verified, and what remains.

### When this document seems wrong
24. Do not silently deviate. Explain the conflict, propose the change, and update this document in the same pull request once agreed.

---

*Keep this document current: any agreed change to architecture, structure or conventions updates this file in the same pull request.*
