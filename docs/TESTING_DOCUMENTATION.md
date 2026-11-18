# Testing Documentation — Church Employment Office Platform

**Status:** Draft for product-owner / tech-lead review
**Depends on:** [BUSINESS_REQUIREMENTS.md](./BUSINESS_REQUIREMENTS.md) §20, §25
**Conventions from:** [BACKEND_GUIDELINES.md](./BACKEND_GUIDELINES.md) §15, [PROJECT_ARCHITECTURE_FRONTEND.md](./PROJECT_ARCHITECTURE_FRONTEND.md) §19
**Companion:** [RBAC_PERMISSIONS.md](./RBAC_PERMISSIONS.md), [WORKFLOWS_AND_STATE_MACHINES.md](./WORKFLOWS_AND_STATE_MACHINES.md), [API_DOCUMENTATION.md](./API_DOCUMENTATION.md)

> BUSINESS_REQUIREMENTS.md §25 ("Definition of Done"): a feature is not complete because its screen
> exists. It is complete when its business rules are enforced server-side, tenant isolation holds,
> sensitive data is protected, and its acceptance criteria below have a passing test. This document
> is the checklist that decision is made against.

---

## 1. Test layers

| Layer | Tool | Scope | Source |
|---|---|---|---|
| Domain calculation | xUnit, no DB | 15-day expiry date math, one-month follow-up due-date math, payment amount handling | BACKEND_GUIDELINES §15 priority 1 |
| Service / integration | xUnit + real `UnitOfWork` over EF InMemory | Business rules in §3 below — the real data-access path, not a hand-written fake | BACKEND_GUIDELINES §15 priority 2 |
| Convention / architecture | xUnit, reflection over the compiled assemblies | Permission resolvability, layer boundaries, `IEntity`→configuration coverage | BACKEND_GUIDELINES §15 priority 3, §12 |
| Precision / constraint | xUnit against real SQL Server (CI only) | `decimal(18,2)` precision, unique indexes — EF InMemory does not enforce either | BACKEND_GUIDELINES §15 priority 4 |
| Frontend unit | Vitest | `shared/lib`, mappers, schemas, cache-key factories, access rules | PROJECT_ARCHITECTURE_FRONTEND §19.1 |
| Frontend component/integration | Vitest + Testing Library + MSW | Each feature as a user experiences it, network mocked | PROJECT_ARCHITECTURE_FRONTEND §19.1 |
| E2E | Playwright (+ axe) | Smoke paths, access per role, RTL rendering | PROJECT_ARCHITECTURE_FRONTEND §19.1, §4 below |

Rules carried over unchanged: test behaviour not implementation (query by role/label/text); mock
the network, not hooks; fresh query client / fresh `DbContext` per test; one behaviour per test,
named as a sentence; every bug fix ships with a regression test.

---

## 2. What's confirmed vs. still open

Most of the business rules below were reviewed and confirmed with the product owner on
2026-11-18 (RBAC_PERMISSIONS.md §0) — the test cases in §3 are written against those confirmed
rules, not placeholder assumptions. A handful of genuinely open items remain (exact mandatory
field lists, exact report field lists, a suspended account's appeal process —
DATABASE_DOCUMENTATION.md §8); test rows touching those are marked **(field list open)** or
**(appeal process open)** rather than "(assumed rule)" — the authorship/mandatory/enforcement
behavior itself is settled, only the exact field names are not.

---

## 3. Acceptance test catalogue

IDs: `TC-<AREA>-<NN>`. `Layer` = where this is best automated (a case can appear at more than one
layer — e.g. a business rule as a service test, and its user-visible effect as an E2E smoke test).

### 3.0 Authentication & OTP (confirmed, RBAC_PERMISSIONS.md §0 item 10)

| ID | Given / When / Then | Layer | Priority |
|---|---|---|---|
| TC-AUTH-01 | `POST /api/auth/otp/request` for a mobile number → an `OtpVerification` row is created with a hashed code and a short `ExpiresAt`; the response never echoes the raw code | Service | Must |
| TC-AUTH-02 | `POST /api/auth/otp/verify` with the correct code before expiry → returns an `otpToken`; the code's `ConsumedAt` is stamped so it cannot be reused | Service | Must |
| TC-AUTH-03 | Registration without a valid, unconsumed `otpToken` → `400 Validation`, no account created | Service | Must |
| TC-AUTH-04 | Repeated wrong-code attempts past the configured `AttemptCount` limit → further attempts rejected (rate-limit), even with a fresh request | Service | Should |
| TC-AUTH-05 | `POST /api/auth/login` authenticates by `PhoneNumber`, not `Email` | Service | Must |

### 3.1 Job Seeker registration & review (BUSINESS_REQUIREMENTS.md §20.1)

| ID | Given / When / Then | Layer | Priority |
|---|---|---|---|
| TC-JS-01 | A visitor browses `/api/jobs` without a token → sees published jobs, no employer contact fields present in the payload at all | Service + E2E | Must |
| TC-JS-02 | An anonymous user calls `POST /api/job-seekers/me/applications` → `401`, no application created | Service | Must |
| TC-JS-03 | A Job Seeker submits a profile missing one of the 3 required documents → `Submit` is rejected (`400 Validation`), profile stays `Draft` | Service | Must |
| TC-JS-04 | A Job Seeker submits with all 3 documents present → `ReviewStatus` becomes `Submitted`, a `Notification` is created for the office queue | Service | Must |
| TC-JS-05 | Office staff with `jobseekers.review` approves all 3 documents individually, then approves the profile → `ReviewStatus = Approved`, `ApprovedBy`/`ApprovedAt` stamped | Service | Must |
| TC-JS-06 | Office staff approves the profile while one document is still `Rejected` → rejected (`409 Conflict`) | Service | Must |
| TC-JS-07 | Office rejects a profile without a reason → `400 Validation` (`reason` required) | Service | Must |
| TC-JS-08 | A rejected Job Seeker resubmits → `ReviewStatus` returns to `Submitted`, no limit on how many times (confirmed) | Service | Must |
| TC-JS-12 | A Job Seeker edits any field on an `Approved` profile → `ReviewStatus` automatically flips to `UnderReview` (confirmed) | Service | Must |
| TC-JS-13 | A suspended Job Seeker's profile is excluded from an Employer's applicant-search/list results, but the Job Seeker can still `GET /api/job-seekers/me` successfully (confirmed) | Service | Must |
| TC-JS-09 | A `Suspended` Job Seeker attempts `POST .../applications` → `403`/business rejection, existing applications remain visible in history | Service | Must |
| TC-JS-10 | Job Seeker A requests Job Seeker B's profile/documents by id → `404`, not `403` (existence not disclosed) | Service | Must |
| TC-JS-11 | Job Seeker views own `GET /api/job-seekers/me/applications` → sees only `JobSeekerFacingStatus`, never raw employer notes | E2E | Must |

### 3.2 Employer registration & review (§20.2)

| ID | Given / When / Then | Layer | Priority |
|---|---|---|---|
| TC-EMP-01 | Employer uploads all 3 legal documents, submits → `VerificationStatus = Submitted` | Service | Must |
| TC-EMP-02 | Office approves Employer → `VerificationStatus = Approved`; Employer can now `POST /api/employer/job-advertisements` | Service | Must |
| TC-EMP-03 | An `Approved` Employer whose `CooperationStatus = Stopped` attempts to create an advertisement → rejected, even though `VerificationStatus` is still `Approved` | Service | Must |
| TC-EMP-04 | Employer A requests Employer B's documents/applicants by id → `404` | Service | Must |

### 3.3 Job advertisement & payment gating (§20.2, §6.6, §18.13 — highest-risk area)

| ID | Given / When / Then | Layer | Priority |
|---|---|---|---|
| TC-AD-01 | Employer submits an advertisement containing an employer phone/email in free-text fields → office review UI/validation flags it (manual moderation at minimum; automated profanity/contact-pattern check optional) | Service + manual | Should |
| TC-AD-02 | Office approves an advertisement → `Status = ApprovedAwaitingPayment` | Service | Must |
| TC-AD-03 | Advertisement reaches `Published` → `ExpiresAt == PublishedAt + 15 days` exactly | Domain calc + Service | Must |
| TC-AD-04 | A background job run with `UtcNow > ExpiresAt` moves the advertisement to `Expired` and it disappears from `GET /api/jobs` within one job cycle | Service + E2E | Must |
| TC-AD-05 | An expired advertisement remains fetchable by `GET /api/office/job-advertisements/{id}` (authorized) with full application/report history intact | Service | Must |
| TC-AD-06 | Employer edits a `Published` advertisement → `Status` flips to `UnderReview`; `PublishedAt`/`ExpiresAt` are **unchanged**; the ad disappears from `GET /api/jobs` while under re-review (confirmed) | Service | Must |
| TC-AD-07 | Employer renews an `Expired` advertisement → a **new** `JobAdvertisement` row is created at `Draft` with a **new** `AdvertisementPayment`; the old row's `Status`/history are untouched (confirmed) | Service | Must |
| **TC-PAY-01** | **Before** `AdvertisementPayment.PaymentStatus == Paid`, `GET /api/employer/job-advertisements/{adId}/applicants` returns rows with no `phone`, no `email`, no `cvDownloadUrl` field at all | Service | **Must — security-critical** |
| **TC-PAY-02** | Same call **after** `PaymentStatus == Paid` → full applicant data returned, scoped to **that advertisement's** applicants only | Service | **Must — security-critical** |
| **TC-PAY-03** | Employer attempts to read a CV via a guessed/reused `cvDownloadUrl` for an advertisement that is **not** paid → `403`/`404`, signed URL rejected | Service | **Must — security-critical** |
| TC-PAY-04 | Employer A, after paying for their own advertisement, attempts to read Employer B's paid advertisement's applicants → `404` | Service | Must |
| TC-PAY-05 | `POST /api/office/job-advertisements/{id}/payment/confirm` called twice against an already-`Paid` payment is a no-op (idempotent), does not create a second publish event or duplicate `PaidAt` stamp (confirmed: manual office confirmation, no gateway webhook in v1) | Service | Must |
| TC-PAY-06 | A non-`OfficeAdmin`/non-granted staff member, or the Employer themself, calls `payment/confirm` → `403` (only office staff with `advertisementpayments.record` may confirm) | Service | Must |
| TC-PAY-07 | `AdvertisementPayment.Currency` defaults to/accepts `EGP`; `PaymentStatus` never transitions to `Refunded` through any exposed endpoint in v1 (confirmed: no refund policy) | Service | Should |

### 3.4 Applications, interviews & reports (§20.3)

| ID | Given / When / Then | Layer | Priority |
|---|---|---|---|
| TC-APP-01 | Job Seeker applies to a `Published` ad while `ReviewStatus != Approved` → rejected (confirmed) | Service | Must |
| TC-APP-02 | Job Seeker applies twice (without withdrawing) to the same advertisement → second call is `409 Conflict` against the filtered unique index (confirmed) | Service | Must |
| TC-APP-09 | Job Seeker withdraws an application (`WithdrawnAt` stamped), then applies again to the same advertisement → second call **succeeds**, creating a new row (confirmed) | Service | Must |
| TC-APP-10 | After `AdvertisementPayment.PaymentStatus == Paid`, the Employer's revealed phone number is their own responsibility to contact — the platform does not log or relay the conversation content (confirmed); the system still requires `EmployerFacingStatus` to be updated in-app | Service | Should |
| TC-APP-03 | Employer sets `EmployerFacingStatus = ContactedRefused` → `JobSeekerFacingStatus` recomputes to `ContactedNotSuitable` in the same transaction | Service | Must |
| TC-APP-04 | `ApplicationStatusHistory` gets one row per status change, correct `StatusDomain`, correct actor | Service | Must |
| TC-APP-05 | Interview marked `Completed` → `InterviewReport` can be created; marked `NoShow` → report's `Attendance = DidNotAttend` path works too | Service | Must |
| TC-APP-06 | Marking an application "hired" creates a `HiringFollowUpReport` with `DueAt == HiredAt + 1 month`, `Status = PendingDue` | Domain calc + Service | Must |
| TC-APP-07 | A background sweep flips an overdue, unfiled follow-up report to `Overdue` and notifies the responsible staff | Service | Should |
| TC-APP-08 | Employer submits an `ApplicantEvaluationReport` a second time for the same application → updates the existing row, does not create a duplicate | Service | Must |

### 3.5 Complaints, suggestions & 3-strike enforcement (§20.4 — acceptance-critical)

| ID | Given / When / Then | Layer | Priority |
|---|---|---|---|
| TC-CPL-01 | Job Seeker and Employer can each submit a complaint; `SenderType` recorded correctly | Service | Must |
| TC-CPL-02 | A complaint left at `ValidityDecision = Pending` does **not** count toward the 3-strike total | Service | **Must — acceptance-critical** |
| TC-CPL-03 | The 3rd `Valid` complaint against the same Job Seeker raises an alert/task — **and does not, by itself, change `ReviewStatus`** | Service | **Must — acceptance-critical** |
| TC-CPL-04 | Only a call to `POST /office/complaints/{id}/decide` by an **`OfficeAdmin`** (confirmed — not `OfficeStaff`, even with a grant) can flip `ReviewStatus` to `Suspended` or `CooperationStatus` to `Stopped`; the decision always creates an `OfficeDecision` row with a non-empty `Reason` | Service | **Must — acceptance-critical** |
| TC-CPL-05 | A suspended Job Seeker's account and documents are **not** deleted — profile row and files remain, only `ReviewStatus` changes | Service | Must |
| TC-CPL-06 | Suggestion submission records sender identity and type; office can list and respond | Service | Must |
| TC-CPL-07 | The valid-complaint count never resets — a 4th valid complaint one year after the 3rd still shows a count of 4, not 1 (confirmed: cumulative, no time window) | Service | Must |
| TC-CPL-08 | A suspended Job Seeker's profile is excluded from any Employer-facing applicant view, but still appears in the office's own view, flagged `Suspended` | Service | Must |
| TC-CPL-09 | *(appeal process open — DATABASE_DOCUMENTATION.md §8)* No endpoint currently lets a suspended Job Seeker or blocked company contest the decision beyond submitting a new `Complaint`/`Suggestion` — revisit once the product owner defines an appeal path. | — | Note only |

### 3.6 Multi-tenant isolation (§20.5 — run against every feature, not just once)

| ID | Given / When / Then | Layer | Priority |
|---|---|---|---|
| TC-TEN-01 | Office A staff token used against any Office-B-owned resource id (job seeker, employer, ad, application, complaint, suggestion, report, document) → `404` for every endpoint in API_DOCUMENTATION.md §3 | Service (parameterized across endpoints) | **Must — run for every endpoint** |
| TC-TEN-02 | A request body/query string that sets an `officeId` different from the caller's own is ignored/rejected, never honored | Service | Must |
| TC-TEN-03 | A file download request for a document belonging to another office/owner → `403`/`404`, never a redirect to a public path | Service | Must |
| TC-TEN-04 | Deactivating an `EmploymentOffice` does not delete any child row; all history remains queryable by a `SuperAdmin` | Service | Must |
| TC-TEN-05 | Search/filter endpoints (`/api/jobs`, office review queues) never return a row whose `OfficeId` differs from the caller's resolved tenant (public search excepted, which is cross-tenant by design but still excludes non-`Published` rows) | Service | Must |

### 3.7 RBAC / permission convention (ties to BACKEND_GUIDELINES §12, §15)

| ID | Given / When / Then | Layer | Priority |
|---|---|---|---|
| TC-RBAC-01 | A generated test sweeps every route in the registry × every role in RBAC_PERMISSIONS.md §1 and asserts the expected allow/deny outcome from §3 of that document | E2E (or integration against the running API) | **Must** |
| TC-RBAC-02 | Every controller action resolves to a `PermissionDefinition` row at startup; an unresolvable action fails the startup audit (test boots the app in a test host and asserts no exception = the audit passing, then asserts a deliberately-misnamed test action **does** throw) | Convention | Must |
| TC-RBAC-03 | `MyApp.Application` has zero compiled references to `Microsoft.EntityFrameworkCore` or any `Microsoft.Data.SqlClient` type (BACKEND_GUIDELINES §2) | Convention | Must |
| TC-RBAC-04 | Every type implementing `IEntity` has exactly one `IEntityTypeConfiguration<T>` (BACKEND_GUIDELINES §7) | Convention | Must |
| TC-RBAC-05 | `OfficeStaff` cannot be granted `officeusers.*` or `complaints.decide` through the staff-permission-assignment endpoint, even if requested (RBAC_PERMISSIONS.md footnotes 1, 5) | Service | Must |

---

## 4. End-to-end smoke suite (Playwright)

One spec file per role, run against every supported language/direction and at least one narrow +
one wide viewport (PROJECT_ARCHITECTURE_FRONTEND §19.4):

| Spec | Golden path | Also covers |
|---|---|---|
| `job-seeker.spec.ts` | Register → upload 3 docs → submit → (seed: office approves) → search jobs → apply → see status update | Denied role: attempt an office-only URL directly → redirected to `/forbidden` |
| `employer.spec.ts` | Register company → upload 3 docs → (seed: office approves) → create ad → submit → (seed: office approves + pay) → see applicants gated then ungated → update outcome | Denied role: attempt another employer's ad url → `404`/403 page |
| `office-staff.spec.ts` | Login → review queue → approve a job seeker and an employer → review and approve/publish an ad → triage a complaint → record an enforcement decision | Cross-tenant: cannot see another office's queue items at all |
| `super-admin.spec.ts` | Create office → invite first office admin → view platform dashboard (aggregate counts only) | Cannot open a private document/complaint-text view |
| `accessibility.spec.ts` | `axe` scan on: home/search, job details, login, job-seeker dashboard, employer create-ad form, office review screen | Keyboard-only run-through of the apply flow and the create-ad form |
| `rtl.spec.ts` | Arabic/RTL render of the job search filter sidebar and the job details page — logical properties hold, no mirrored icon regressions | — |

---

## 5. Test data strategy

- Backend: a shared `Build` helper per entity (BACKEND_GUIDELINES §15) — e.g. `Build.JobSeeker()`,
  `Build.PublishedAdvertisement(paid: true)` — so test intent reads from the test name, not from
  12 lines of entity setup.
- Frontend: MSW handlers per feature in `test/msw/`, mirroring the DTOs in
  API_DOCUMENTATION.md §3 exactly — a handler that drifts from the real contract is worse than no
  mock.
- E2E: a seed script creates one office, one approved job seeker, one approved+paid employer
  advertisement, and one pending-review set of each, so smoke specs don't each re-run the full
  approval chain through the UI.
- Never use production data or real personal documents in any test fixture — use synthetic
  names/ids and a 1x1 placeholder file for CV/document uploads.

---

## 6. Definition of Done (ties to BUSINESS_REQUIREMENTS.md §25)

A feature is not "done" until, in the same change:

- [ ] Its row(s) in §3 above exist and pass.
- [ ] Tenant isolation case (TC-TEN-01 pattern) is added for its new endpoints.
- [ ] If it touches the payment gate or the 3-strike enforcement, the relevant **Must —
      security-critical / acceptance-critical** cases are not skipped or stubbed.
- [ ] `dotnet build && dotnet test` pass; frontend `typecheck`, `lint`, `test`, `build`,
      `i18n:check` pass.
- [ ] If an acceptance criterion depends on an item still genuinely open (DATABASE_DOCUMENTATION.md
      §8 — field lists, the appeal process), the test is explicitly marked `(field list open)` /
      `(appeal process open)` here and in the test's own name/comment.

---

*When a remaining open decision is answered, update: the confirmed-decisions table in RBAC_PERMISSIONS.md §0 or
the relevant state machine in WORKFLOWS_AND_STATE_MACHINES.md, the affected test rows above, and
the test code itself — in one change, not three.*
