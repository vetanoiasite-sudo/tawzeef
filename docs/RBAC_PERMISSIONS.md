# RBAC & Permissions — Church Employment Office Platform

**Status:** Draft for product-owner review
**Depends on:** [BUSINESS_REQUIREMENTS.md](./BUSINESS_REQUIREMENTS.md) §4, §12, §17, §24.A
**Consumed by:** [DATABASE_DOCUMENTATION.md](./DATABASE_DOCUMENTATION.md), [API_DOCUMENTATION.md](./API_DOCUMENTATION.md), [BACKEND_GUIDELINES.md](./BACKEND_GUIDELINES.md) §12

> This document defines **who can do what**. Every rule here must be enforced server-side
> (`PermissionAuthorizationFilter` + default-deny fallback policy, per BACKEND_GUIDELINES §12) and
> mirrored on the frontend only as a UX convenience (`<Can>` / `useAccess()`, per
> PROJECT_ARCHITECTURE_FRONTEND §13.3). The frontend check is never the source of truth.

---

## 0. Decisions confirmed with the product owner (2026-11-18)

The business requirements document originally marked these as **\[قرار مطلوب\]** (decision
required). They have now been reviewed and confirmed directly with the product owner, so the rest
of this document treats them as settled — not as an assumption pending review.

| # | Question (BUSINESS_REQUIREMENTS.md ref) | **Confirmed answer** |
|---|---|---|
| 1 | §4.2 — one office-admin role, or several staff roles? | **Confirmed:** one `OfficeAdmin` role per office, plus an optional `OfficeStaff` role with a **data-driven** permission subset (assignable per office) — kept as the flexible default since the product owner did not ask for named sub-roles (reviewer/finance/etc.) yet. |
| 2 | §24.A.4 — can an office have multiple admins/staff? | **Confirmed: yes.** `UserOfficeMembership` carries a `role` + optional permission overrides. |
| 3 | §24.A.5 — can Super Admin access private documents (CVs, IDs, legal docs, complaint/report text)? | **Confirmed: no.** Super Admin sees aggregate metadata (counts, statuses, timestamps) only — never document contents, complaint text, or report free-text. |
| 4 | §24.A.1/A.2/A.3 — one office per account, or multiple? How is the office chosen? | **Confirmed:** exactly **one office per Job Seeker / Employer account**. The office is chosen from a list shown at registration. |
| 5 | §11.3/§11.4/§24.G.46 — does a complaint count toward the 3-strike threshold only once the office marks it "valid"? | **Confirmed: yes** — only **office-confirmed-valid** complaints count. A `Pending` or `Invalid` complaint never counts. |
| 6 | §24.G.48/G.51 — does the 3-strike count reset after a time window? | **Confirmed: no.** It is a running, cumulative count with no expiry window, for both Job Seekers and companies. |
| 7 | §24.G.50 — what exactly happens when a Job Seeker is suspended? | **Confirmed:** their profile is hidden from Employers (cannot be found/shown as an applicant on new activity) but they can still log in and see their own account/status. Suspension is **not** a login block. |
| 8 | §24.G.54 — who may approve a suspension / cooperation-stop decision? | **Confirmed: `OfficeAdmin` only.** Not grantable to `OfficeStaff` under any permission grant (see footnote 5 in §3). |
| 9 | §24.E.33 — does the office mediate every contact, or can the Employer contact the applicant directly after paid access? | **Confirmed:** the Employer contacts the applicant **directly** once that advertisement's payment is confirmed. The office does not relay every message. |
| 10 | §24.B.6/B.7 — login identifier & OTP | **Confirmed:** login by **mobile number**; registration requires **OTP verification** of that number. |
| 11 | §24.B.10/B.11 — does editing approved data trigger re-review? | **Confirmed: yes, automatically.** Any edit to an approved Job Seeker profile or an approved Employer's company/documents flips the record back to `UnderReview`. |
| 12 | §24.B.12 — resubmission limit after rejection? | **Confirmed: no limit.** |
| 13 | §24.E.30 — can a Job Seeker apply while review is pending? | **Confirmed: no** — `ReviewStatus` must already be `Approved`. |
| 14 | §24.E.31/E.32 — withdrawal and re-application | **Confirmed:** a Job Seeker may withdraw an application and may re-apply to the same advertisement afterward. |
| 15 | §24.C.15/C.16 — pay before/after review; 15-day anchor | **Confirmed:** office reviews and approves content **first**, Employer pays **after** approval; the 15-day clock starts at **publish time**, not payment time. |
| 16 | §24.C.17/C.21 — pause; editing a published ad | **Confirmed: yes to both.** The office can pause a published ad, and an Employer editing a published ad must go back through office approval. |
| 17 | §24.C.18/C.19 — renewing an expired ad | **Confirmed:** renewal creates a **new** advertisement with a **new** payment — never an extension of the expired row. |
| 18 | §24.D.22–25 — fee currency & payment method | **Confirmed:** currency is **EGP**. Payment is **confirmed manually by office staff** for v1 — no online gateway yet, though the schema/API stay gateway-ready for later. |
| 19 | §24.D.26/D.27 — unlock scope/duration | **Confirmed:** payment unlocks applicant data for the **entire 15-day life of that advertisement** (one fee, no separate "data access" fee). |
| 20 | §24.D.28 — refund policy | **Confirmed: no refunds.** Made explicit to the Employer before they pay. |
| 21 | §24.F.38–40/F.42/F.44/F.45 — interviews & reports | **Confirmed:** the Employer schedules the interview and writes the first (`InterviewReport`) report. The one-month `HiringFollowUpReport` is completed by the **office**, who follows up with both sides. It is **mandatory** for every hire; an unfiled report past its due date becomes `Overdue` and notifies the responsible staff — there is no harder enforcement than that notification. |
| 22 | §24.H.55–57/H.61 | **Confirmed:** in-app notifications only for v1 (no SMS/email channel yet); UI is **Arabic-only**, RTL; data is **retained permanently**, never hard-deleted — soft `IsActive` only. |
| 23 | §24.H.58 | **Confirmed:** responsive web for v1; a **separate native mobile app is planned for later** — the API must stay client-agnostic (already true, it's a plain REST/JSON API) so a future app can consume it without backend changes. |

Everything in §1–§9 below reflects these confirmed answers. Remaining open items (exact mandatory
field lists, exact report field lists, appeal process for a suspension) are still genuinely open
and are called out inline where relevant — not resolved by this round of review.

---

## 1. Roles

| Role code | Scope | Held by | Notes |
|---|---|---|---|
| `SuperAdmin` | Platform-wide, all tenants | Platform operator staff | Not tied to any `office_id`. Bypasses the permission filter per BACKEND_GUIDELINES §12, but **not** the privacy restriction in §0.3. |
| `OfficeAdmin` | Single tenant (`office_id`) | Employment office management | Full permission set within its own office. Can manage `OfficeStaff` accounts for that office. |
| `OfficeStaff` | Single tenant (`office_id`) | Office employees (reviewers, complaint handlers, etc.) | Permission subset assigned by the `OfficeAdmin`, seeded as data (§0 item 1). Cannot manage other staff accounts or office-level settings unless explicitly granted. |
| `Employer` | Single tenant (`office_id`), single company | Company representative | One `User` ↔ one `Employer` company record at launch. |
| `JobSeeker` | Single tenant (`office_id`) | Individual applicant | One `User` ↔ one `JobSeekerProfile` at launch. |
| *(implicit)* `Guest` | Public | Unauthenticated visitor | Not a stored role — the absence of a token. Can browse/search public job data only. |

A `User` has exactly one role at launch (§0 item 4, confirmed). `UserOfficeMembership` (BUSINESS_REQUIREMENTS.md §15.1) is the seam left open for a future user-belongs-to-multiple-offices or
multiple-roles-per-office scenario — the table exists from day one even though v1 always writes
exactly one active membership row per user, so that relaxing the one-office-per-user rule later is
a data change, not a schema migration.

---

## 2. Permission catalogue

Permissions follow the `module.action` convention from BACKEND_GUIDELINES §12, so the
`PermissionAuthorizationFilter` can resolve most of them straight from the controller/action name.
Actions that are **not** plain CRUD (`approve`, `reject`, `suspend`, `pay`, `publish`, …) are custom
verbs that must be added to the permission catalogue explicitly — the naming convention
(`Get*`→view, `Create*`→create, `Update*`→edit, `Deactivate*`/`Delete*`→delete) does not cover them,
and an endpoint whose permission cannot be resolved must fail the startup audit rather than be
silently allowed (BACKEND_GUIDELINES §12, "endpoint cannot be classified → refused").

| Module | Actions | Notes |
|---|---|---|
| `offices` | `view`, `create`, `edit`, `deactivate` | Super Admin only. |
| `officeusers` | `view`, `invite`, `edit`, `deactivate` | Manage `OfficeAdmin`/`OfficeStaff` accounts within one office. |
| `jobseekers` | `view`, `viewdocuments`, `review`, `approve`, `reject`, `suspend`, `reinstate` | `review` = move Submitted → Under Review. |
| `employers` | `view`, `viewdocuments`, `review`, `approve`, `reject`, `suspend`, `reinstate` | |
| `jobadvertisements` | `view`, `create`, `edit`, `submit`, `review`, `approve`, `reject`, `publish`, `pause`, `close`, `renew` | `create`/`edit`/`submit`/`renew` are Employer actions; the rest are office actions. |
| `advertisementpayments` | `view`, `record`, `refund` | `record` = confirm a payment (manual or gateway webhook). |
| `jobapplications` | `view`, `viewown`, `create`, `withdraw`, `updateoutcome` | `viewown` = Job Seeker's own applications only. |
| `applicantdata` | `viewgated` | The CV/phone/contact unlock described in BUSINESS_REQUIREMENTS.md §6.6 — a **separate** permission from `jobapplications.view` so the payment gate is checkable independently of RBAC (see §5 below). |
| `interviews` | `view`, `schedule`, `edit` | |
| `interviewreports` | `view`, `create` | First report, BUSINESS_REQUIREMENTS.md §10.1. |
| `hiringfollowupreports` | `view`, `create` | Second report, BUSINESS_REQUIREMENTS.md §10.2. |
| `applicantevaluationreports` | `view`, `create` | Employer's per-applicant outcome, §10.3. |
| `complaints` | `view`, `viewown`, `create`, `triage`, `resolve`, `decide` | `decide` = the office decision to suspend/stop-cooperation, kept separate from `resolve` (closing the complaint record) so the two can be assigned to different staff. |
| `suggestions` | `view`, `viewown`, `create`, `respond` | |
| `officedecisions` | `view`, `create` | Append-only audit of suspension / cooperation-stop decisions (§11.5). |
| `notifications` | `viewown` | Every role only ever sees its own. |
| `auditlogs` | `view` | Office-scoped for `OfficeAdmin`; platform-scoped for `SuperAdmin` (metadata only, §0.3). |
| `dashboards` | `view` | Scoped automatically to the caller's role/office — see §4. |
| `platformsettings` | `view`, `edit` | Super Admin only. |

---

## 3. Role × Permission matrix

Legend: **✅ full** · **🟡 scoped** (own records / own office / payment-gated — see footnotes) ·
**—** not permitted.

| Permission | SuperAdmin | OfficeAdmin | OfficeStaff (data-driven grant) | Employer | JobSeeker |
|---|---|---|---|---|---|
| `offices.*` | ✅ | — | — | — | — |
| `officeusers.*` | ✅ (any office) | 🟡 own office | — ¹ | — | — |
| `jobseekers.view` / `.viewdocuments` | 🟡 metadata only (§0.3) | 🟡 own office | 🟡 if granted | — | 🟡 own profile only |
| `jobseekers.review/approve/reject/suspend/reinstate` | — | 🟡 own office | 🟡 if granted | — | — |
| `employers.view` / `.viewdocuments` | 🟡 metadata only (§0.3) | 🟡 own office | 🟡 if granted | 🟡 own company only | — |
| `employers.review/approve/reject/suspend/reinstate` | — | 🟡 own office | 🟡 if granted | — | — |
| `jobadvertisements.view` (published, public) | ✅ | ✅ | ✅ | ✅ | ✅ (incl. guests) |
| `jobadvertisements.create/edit/submit/renew` | — | — | — | 🟡 own company only | — |
| `jobadvertisements.review/approve/reject/pause/close` | — | 🟡 own office | 🟡 if granted | — | — |
| `jobadvertisements.publish` ² | — | 🟡 own office (system-triggered on payment confirm) | — | — | — |
| `advertisementpayments.view` | 🟡 metadata only | 🟡 own office | 🟡 if granted | 🟡 own payments only | — |
| `advertisementpayments.record/refund` | — | 🟡 own office | 🟡 if granted (finance subset) | — | — |
| `jobapplications.create/withdraw` | — | — | — | — | 🟡 own applications only |
| `jobapplications.viewown` | — | — | — | — | 🟡 own applications only |
| `jobapplications.view` (office-wide) | — | 🟡 own office | 🟡 if granted | — | — |
| `jobapplications.view` (per advertisement, pre-payment) ³ | — | 🟡 own office | 🟡 if granted | 🟡 own ads, **no CV/contact** | — |
| `applicantdata.viewgated` ³ | — | 🟡 own office | 🟡 if granted | 🟡 own ads **after payment only** | — |
| `jobapplications.updateoutcome` | — | — | — | 🟡 own ads only | — |
| `interviews.*` | — | 🟡 own office | 🟡 if granted | 🟡 own ads only — schedules & records it ⁴ | 🟡 read own interview only |
| `interviewreports.create/view` | — | 🟡 own office (read) | 🟡 if granted (read) | 🟡 own ads only (write) ⁴ | — |
| `hiringfollowupreports.create/view` | — | 🟡 own office (write) | 🟡 if granted (write) | 🟡 own ads only (read) ⁴ | — |
| `applicantevaluationreports.create/view` | — | 🟡 own office (read) | 🟡 if granted (read) | 🟡 own ads only (write) | — |
| `complaints.create` | — | — | — | ✅ | ✅ |
| `complaints.viewown` | — | — | — | 🟡 own complaints only | 🟡 own complaints only |
| `complaints.view/triage/resolve` | — | 🟡 own office | 🟡 if granted | — | — |
| `complaints.decide` | — | 🟡 own office (requires `OfficeAdmin` or explicitly granted) | 🟡 if granted | — | — |
| `suggestions.create` | — | — | — | ✅ | ✅ |
| `suggestions.viewown` | — | — | — | 🟡 own only | 🟡 own only |
| `suggestions.view/respond` | — | 🟡 own office | 🟡 if granted | — | — |
| `officedecisions.view` | 🟡 metadata only | 🟡 own office | 🟡 if granted | — | — |
| `officedecisions.create` | — | 🟡 own office | — ⁵ | — | — |
| `notifications.viewown` | ✅ own | ✅ own | ✅ own | ✅ own | ✅ own |
| `auditlogs.view` | 🟡 platform, metadata only | 🟡 own office | — | — | — |
| `dashboards.view` | ✅ platform dashboard | ✅ office dashboard | 🟡 subset per grant | ✅ employer dashboard | ✅ job-seeker dashboard |
| `platformsettings.*` | ✅ | — | — | — | — |

¹ `OfficeStaff` can never manage other staff accounts regardless of grants — that stays an
`OfficeAdmin`-only action, to avoid a staff member escalating their own permissions.
² `publish` is normally a system transition fired when the payment-confirmation event lands
(BUSINESS_REQUIREMENTS.md §6.3 step 6) — exposed as a manual office action only as a fallback for
manually-confirmed payments.
³ This is the **payment-gating rule** from BUSINESS_REQUIREMENTS.md §6.6 and is enforced
independently of RBAC — see §5.
⁴ Confirmed (§0 item 21): the Employer schedules the interview and writes the `InterviewReport`
(first report); the office writes the `HiringFollowUpReport` (one-month report), following up with
both the Employer and the Job Seeker.
⁵ Deliberately **not** grantable to `OfficeStaff` under the default catalogue — a suspension /
cooperation-stop decision requires `OfficeAdmin`-level authority per BUSINESS_REQUIREMENTS.md §11.3/§11.4
("requires Employment Office approval"). An office may choose to grant it; the default is closed.

---

## 4. Data-scoping rules (beyond plain permission checks)

These are not expressible as a static permission name — they must be enforced as a query/authorization
predicate on every request, per BACKEND_GUIDELINES §12 and BUSINESS_REQUIREMENTS.md §12.2 /
§17.4.

1. **Tenant isolation.** Every tenant-owned row carries `office_id`. Every query for an
   `OfficeAdmin`/`OfficeStaff` is implicitly filtered to `office_id == currentUser.OfficeId`. A
   request that supplies a different `office_id` (path, query, or body) must be rejected, not
   silently re-scoped — never trust a client-supplied tenant id (BUSINESS_REQUIREMENTS.md §12.2).
2. **Ownership scoping.**
   - `JobSeeker` → only their own `JobSeekerProfile`, `JobSeekerDocument`, `JobApplication`,
     `Complaint`, `Suggestion`, `Notification` rows.
   - `Employer` → only their own `Employer` company, `EmployerDocument`, `JobAdvertisement`,
     `AdvertisementPayment`, and `JobApplication`/report rows that reference *their own*
     advertisements.
3. **Payment gating** (see §5) is a third, independent scoping dimension layered on top of
   tenant + ownership scoping — all three must pass.
4. **Document/file access.** Every file download goes through an authorization check against the
   three rules above, even if the request supplies a seemingly-valid file reference
   (BUSINESS_REQUIREMENTS.md §17.3 — no predictable/public file paths, no bypass via direct URL).
5. **Status-gated actions.** An action is also gated by the record's current state — e.g. an
   `Employer` cannot `create` a `JobAdvertisement` while their own verification status is not
   `Approved`; a suspended `JobSeeker` cannot `create` a `JobApplication`. See
   WORKFLOWS_AND_STATE_MACHINES.md for the authoritative transition tables.

---

## 5. The payment-gating rule, precisely

This is the single most security-critical rule in the platform (BUSINESS_REQUIREMENTS.md §6.6,
§18.13) and it is **not** a role permission — it is a per-advertisement runtime flag checked on
every applicant-data request:

```
canViewApplicantData(employerUserId, jobAdvertisementId) :=
    employer owns jobAdvertisementId
    AND AdvertisementPayment for jobAdvertisementId has payment_status == Paid
    (confirmed, §0 item 19: unlock lasts the entire 15-day life of the advertisement — one fee,
     no separate "data access" fee)
```

Before this flag is true, the Employer's `jobapplications.view` response for that advertisement
must omit `JobSeeker.phone`, `JobSeeker.email`, `JobSeeker.address`, and **must not** return a
CV file reference at all — not a redacted one, not a 403-guarded one the client could still probe.
This must be enforced in the service/DTO-mapping layer (never a frontend-only hide), per
BACKEND_GUIDELINES §9 and BUSINESS_REQUIREMENTS.md §17.4.

**Confirmed (§0 item 9):** once this flag is true, the Employer contacts the applicant **directly**
with the now-revealed phone number — the office does not relay every message. This does not change
the rule above: the phone number only ever leaves the server once the flag is true, same as before.

---

## 6. Super Admin — explicit boundary (confirmed, §0 item 3)

Super Admin has:

- Full access to `EmploymentOffice`, office admin accounts, and platform-level aggregate
  counts/statistics.
- **No** access to `JobSeekerDocument`/`EmployerDocument` file contents, `Complaint.complaint_text`,
  `Suggestion.content`, or report free-text fields.
- Read access to status fields and timestamps everywhere (for support/troubleshooting), logged as
  an audit event.

If the product owner later grants broader access, implement it as an explicit, logged,
time-boxed "support access" action rather than a standing permission — never a silent exception to
tenant isolation.

---

## 7. Enforcement checklist (ties to BACKEND_GUIDELINES §12)

- [ ] `AuthorizationOptions.FallbackPolicy` = `RequireAuthenticatedUser()`.
- [ ] `PermissionAuthorizationFilter` registered globally; resolves `module.action` from
      controller module-map + verb-prefix convention; **custom verbs in §2 are registered
      explicitly**, not inferred.
- [ ] `PermissionEndpointAudit.AssertEveryEndpointIsClassified` runs at startup; the app refuses to
      boot on an unresolved endpoint.
- [ ] Role assignment seeded as data (`RolePermission`-style table), editable by `SuperAdmin`
      (offices) and `OfficeAdmin` (own office's `OfficeStaff` grants) — never a hard-coded
      `if (role == "...")` in a service.
- [ ] A convention test sweeps every controller and asserts a resolvable permission exists
      (BACKEND_GUIDELINES §15, "convention tests").
- [ ] A generated test asserts expected access for every **role × route** pair in the route
      registry (PROJECT_ARCHITECTURE_FRONTEND §19.2) — this table is that test's source of truth.

---

## 8. Frontend mirroring (PROJECT_ARCHITECTURE_FRONTEND §13.3)

- The route registry's `access rule` per route is derived from the "who can reach this page" column
  implied by §3 above (e.g. `/office/job-seekers/review` → `jobseekers.review`).
- Element-level gating inside a page uses `<Can permission="jobseekers.approve">` /
  `useAccess()` — never `user.role === "OfficeAdmin"` comparisons.
- The frontend never re-derives the payment-gating rule (§5) on its own: it renders what the API
  returned (a CV field that is simply absent from the payload pre-payment), so there is nothing
  sensitive to leak even if a check is bypassed in the browser.

---

*Update this document whenever a role, permission, or scoping rule changes — in the same change
that modifies the `PermissionAuthorizationFilter` catalogue or the route registry.*
