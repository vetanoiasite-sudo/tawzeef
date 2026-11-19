# API Documentation — Church Employment Office Platform

**Status:** Draft for product-owner / tech-lead review
**Depends on:** [DATABASE_DOCUMENTATION.md](./DATABASE_DOCUMENTATION.md),
[RBAC_PERMISSIONS.md](./RBAC_PERMISSIONS.md),
[WORKFLOWS_AND_STATE_MACHINES.md](./WORKFLOWS_AND_STATE_MACHINES.md)
**Conventions from:** [BACKEND_GUIDELINES.md](./BACKEND_GUIDELINES.md) §10–§11

> Endpoint and field names below are the planning baseline, not a frozen contract — confirm against
> whatever already exists in the repository before coding (BUSINESS_REQUIREMENTS.md §21, Phase 0).

---

## 1. Conventions

- **Base route:** `api/[controller]`, plural resource name, id in the route
  (`/api/job-advertisements/{id:guid}`), never in a header or query string.
- **Response envelope — every response body has the same shape:**

```json
{
  "isSuccess": true,
  "error": "None",
  "errorMessages": [],
  "data": { }
}
```

  `data` is omitted/`null` on failure. This is `ServiceResult<T>` serialized directly
  (BACKEND_GUIDELINES §10–§11) — the controller never reshapes it.
- **HTTP status mapping** (`ApiControllerBase.Respond`, BACKEND_GUIDELINES §11):

  | `error` | HTTP status | Meaning |
  |---|---|---|
  | `None` | `200 OK` | Success. |
  | `Validation` | `400 Bad Request` | Shape or business-rule validation failed; `errorMessages` lists every problem, not just the first. |
  | `NotFound` | `404 Not Found` | Record does not exist **or** exists in another tenant (never leak existence across tenants — see §2). |
  | `Conflict` | `409 Conflict` | State conflict: duplicate code, duplicate application, already-paid advertisement, concurrency conflict. |
  | `Forbidden` | `403 Forbidden` | Authenticated, but the permission/ownership/payment-gate check failed. |
  | *(unexpected exception)* | `500 Internal Server Error` | Generic message only; details go to the log, never the response (BACKEND_GUIDELINES §13). |
  | *(binding failure)* | `400 Bad Request` | Same envelope shape, via `InvalidModelStateResponseFactory` (BACKEND_GUIDELINES §11). |
  | *(no/expired token)* | `401 Unauthorized` | Default-deny fallback policy, or account re-check failed (BACKEND_GUIDELINES §12). |

- **Auth:** `Authorization: Bearer <JWT>` on every endpoint except the few `AllowAnonymous` ones
  listed explicitly in §3.1 and §3.9 (public job browsing, `/health`).
- **Pagination:** list endpoints take `pageIndex` (0-based) and `pageSize` as query params, and
  return an `X-Pagination` response header (`{ "totalCount", "pageIndex", "pageSize",
  "totalPages" }`) — never an unpaged array for anything that can grow unbounded
  (BACKEND_GUIDELINES §11, PROJECT_ARCHITECTURE_FRONTEND §17).
- **Filtering:** `[FromQuery]` filter DTOs (`{Entity}QueryDto`) for list endpoints — never
  `[FromBody]` for a `GET`.
- **Cancellation:** every action accepts and forwards a `CancellationToken`; not shown per-endpoint
  below to avoid repetition.
- **Dates/money:** ISO-8601 UTC strings over the wire; money as a plain decimal/number — client
  formats for display, never the server.
- **File upload/download:** `multipart/form-data` for document uploads; downloads are short-lived
  signed URLs or a streamed authenticated response, never a public static path
  (BUSINESS_REQUIREMENTS.md §17.3).

---

## 2. Tenant & ownership resolution (applies to every endpoint below)

No endpoint accepts an `officeId` from the client as something that changes what it does. The
tenant is always derived server-side from the caller (`UserOfficeMembership` for office staff, the
`JobSeekerProfile`/`Employer` row the `UserId` owns for those roles). If a path/body id belongs to
a different tenant than the caller's, the response is **`404 NotFound`**, not `403` — existence is
not disclosed across tenants (RBAC_PERMISSIONS.md §4). Full detail at
RBAC_PERMISSIONS.md §4–§5; every table in §3 below names the permission required rather than
repeating this paragraph per endpoint.

---

## 3. Endpoint inventory

Each sub-section maps to one `Features/{Feature}` folder per BACKEND_GUIDELINES §4.

### 3.1 Auth

**Confirmed (RBAC_PERMISSIONS.md §0 item 10):** login identifier is the **mobile number**;
registration requires OTP verification of that number before the account is usable.

| Method | Route | Auth | Permission |
|---|---|---|---|
| `POST` | `/api/auth/otp/request` | Anonymous | — |
| `POST` | `/api/auth/otp/verify` | Anonymous | — |
| `POST` | `/api/auth/register/job-seeker` | Anonymous (requires a just-verified OTP) | — |
| `POST` | `/api/auth/register/employer` | Anonymous (requires a just-verified OTP) | — |
| `POST` | `/api/auth/login` | Anonymous | — |
| `POST` | `/api/auth/refresh-token` | Anonymous (valid refresh token) | — |
| `POST` | `/api/auth/logout` | Authenticated | — |

```json
// POST /api/auth/otp/request
{ "mobileNumber": "string", "purpose": "Registration" }
// Response data: {} (204-style ack; never echoes the code)

// POST /api/auth/otp/verify
{ "mobileNumber": "string", "code": "string", "purpose": "Registration" }
// Response data
{ "otpToken": "string" }
// otpToken is a short-lived, single-use proof of verification passed into the register call below —
// the register endpoint re-validates it server-side rather than trusting a bare "verified: true" flag.
```

**`POST /api/auth/register/job-seeker`**

```json
// Request
{ "fullName": "string", "mobileNumber": "string", "otpToken": "string", "email": "string?", "password": "string", "officeId": "guid" }
// Response data
{ "userId": "guid", "jobSeekerProfileId": "guid", "accessToken": "string", "refreshToken": "string" }
```

**`POST /api/auth/login`**

```json
// Request — identifier is the mobile number (confirmed)
{ "identifier": "string", "password": "string" }
// Response data
{ "accessToken": "string", "refreshToken": "string", "expiresAt": "datetime", "role": "string", "officeId": "guid?", "permissionCodes": ["string"] }
// permissionCodes: every catalogued code for OfficeAdmin, the StaffPermissionGrant subset for
// OfficeStaff, empty for SuperAdmin/JobSeeker/Employer (each covered by its own frontend shortcut
// instead — RBAC_PERMISSIONS.md §2/§3).
```

---

### 3.2 Offices (Super Admin)

| Method | Route | Permission |
|---|---|---|
| `GET` | `/api/offices` (paged, filterable by status/name) | `offices.view` |
| `GET` | `/api/offices/{id:guid}` | `offices.view` |
| `POST` | `/api/offices` | `offices.create` |
| `PUT` | `/api/offices/{id:guid}` | `offices.edit` |
| `DELETE` | `/api/offices/{id:guid}` (deactivate, soft) | `offices.deactivate` |
| `POST` | `/api/offices/{id:guid}/admins` (invite first `OfficeAdmin`) | `officeusers.invite` |

```json
// CreateOfficeDto
{ "name": "string", "arName": "string?", "code": "string", "address": "string?", "city": "string?", "phone": "string?", "email": "string?" }
```

---

### 3.3 Office staff accounts

| Method | Route | Permission |
|---|---|---|
| `GET` | `/api/office-users` (scoped to caller's office) | `officeusers.view` |
| `POST` | `/api/office-users/invite` | `officeusers.invite` — **SuperAdmin may target any office (the "first OfficeAdmin" case, BUSINESS_REQUIREMENTS.md §12.3); an OfficeAdmin always invites into their own office** |
| `PUT` | `/api/office-users/{id:guid}/permissions` | `officeusers.edit` |
| `DELETE` | `/api/office-users/{id:guid}` (revoke) | `officeusers.deactivate` |
| `POST` | `/api/auth/accept-staff-invite` | public — the invitee only, see below |

```json
// InviteOfficeStaffDto
{ "fullName": "string", "mobileNumber": "string", "role": "OfficeStaff", "permissionCodes": ["jobseekers.view", "jobseekers.review"], "officeId": "guid?" }
// officeId is read only for a SuperAdmin caller (required then) — an OfficeAdmin's own office is
// used regardless of what's sent. Login is by mobile number platform-wide (§0 item 10), so this
// uses mobileNumber, not the email the original draft of this section showed — no email-sending
// capability exists in this app.

// AcceptOfficeInviteDto (POST /api/auth/accept-staff-invite)
{ "mobileNumber": "string", "otpToken": "string", "password": "string" }
```

Implemented as the same OTP-then-self-registration shape as job-seeker/employer registration
(§3.1), not an admin-set or emailed password: `invite` only records who's invited (into
`OfficeStaffInvite`, not yet a real account); the invitee proves their own number via
`POST /api/auth/otp/request`/`otp/verify` with `"purpose": "StaffInvite"`, then calls
`accept-staff-invite` to set their own password — nobody but the invitee ever sets or sees it. The
response is an ordinary `AuthResultDto` (§3.1), i.e. accepting logs you in immediately.

`permissionCodes` is validated against the seeded `PermissionDefinition` catalogue
(DATABASE_DOCUMENTATION.md §3) — an unknown code is a `400 Validation` error, not silently ignored,
and so is any `PermissionCatalogue.OfficeAdminOnly` code (RBAC_PERMISSIONS.md footnote 5) — those
can never be granted to an OfficeStaff invite regardless of who's inviting.

---

### 3.4 Job Seeker profile & documents

| Method | Route | Permission |
|---|---|---|
| `GET` | `/api/job-seekers/me` | `JobSeeker` — own profile |
| `PUT` | `/api/job-seekers/me` | `JobSeeker` — own profile |
| `POST` | `/api/job-seekers/me/documents` (`multipart/form-data`, field `documentType`) | `JobSeeker` — own profile |
| `GET` | `/api/job-seekers/me/documents` | `JobSeeker` — own profile |
| `POST` | `/api/job-seekers/me/submit` | `JobSeeker` — own profile |
| `GET` | `/api/office/job-seekers` (paged, filter by `reviewStatus`, name, city) | `jobseekers.view` |
| `GET` | `/api/office/job-seekers/{id:guid}` | `jobseekers.view`, same office |
| `GET` | `/api/office/job-seekers/{id:guid}/documents/{documentId:guid}` (signed download) | `jobseekers.viewdocuments`, same office |
| `POST` | `/api/office/job-seekers/{id:guid}/documents/{documentId:guid}/approve` | `jobseekers.review` |
| `POST` | `/api/office/job-seekers/{id:guid}/documents/{documentId:guid}/reject` | `jobseekers.review` |
| `POST` | `/api/office/job-seekers/{id:guid}/approve` | `jobseekers.approve` |
| `POST` | `/api/office/job-seekers/{id:guid}/reject` | `jobseekers.reject` |
| `POST` | `/api/office/job-seekers/{id:guid}/suspend` | `jobseekers.suspend` (see RBAC_PERMISSIONS.md footnote 5) |
| `POST` | `/api/office/job-seekers/{id:guid}/reinstate` | `jobseekers.reinstate` |

```json
// JobSeekerProfileDto (GET /api/job-seekers/me)
{
  "id": "guid", "fullName": "string", "mobileNumber": "string", "email": "string?",
  "city": "string?", "education": "string?", "yearsOfExperience": 0,
  "reviewStatus": "Submitted", "rejectionReason": "string?",
  "suspensionReason": "string?", "suspendedAt": "datetime?",
  "validComplaintCount": 0,
  "documents": [{ "id": "guid", "documentType": "Cv", "reviewStatus": "Pending", "originalFileName": "cv.pdf", "uploadedAt": "datetime" }]
}
// validComplaintCount: §10.3's alert surfaced where the office already works, rather than as a
// separate notification feature — count of complaints against this profile with
// ValidityDecision == Valid (§0 item 5), always 0 on this self-service response; populated for
// real on GET/POST /api/office/job-seekers/{id:guid}/... (§3.4) and the list items from
// GET /api/office/job-seekers. The frontend highlights it once it reaches 3.
```

`RejectReviewDto` (shared shape for job-seeker, employer, document, and advertisement rejection
endpoints): `{ "reason": "string" }` — `reason` is required, `400 Validation` if blank.

**Confirmed (RBAC_PERMISSIONS.md §0 item 11):** `PUT /api/job-seekers/me` and
`POST /api/job-seekers/me/documents` **automatically** revert an `Approved` profile to
`UnderReview` as a side effect of the write — there is no separate "resubmit for review" step for
an already-approved profile; editing *is* the resubmit.

---

### 3.5 Employer company & documents

Mirrors §3.4 with `Employer`/`EmployerDocument` and the `employers.*` permissions — same self-service
shape too (`employers.viewown/.editown/.uploaddocuments/.submit`, granted by having an `Employer`
row at all, not a staff grant):

| Method | Route | Permission |
|---|---|---|
| `GET` | `/api/employers/me` | `employers.viewown` |
| `PUT` | `/api/employers/me` | `employers.editown` |
| `POST` | `/api/employers/me/documents` (`multipart/form-data`, field `documentType`: `CommercialRegister`\|`TaxCard`\|`OwnerNationalId`) | `employers.uploaddocuments` |
| `POST` | `/api/employers/me/submit` | `employers.submit` |
| `GET` | `/api/office/employers` (paged, filter by `verificationStatus`) | `employers.view` |
| `GET` | `/api/office/employers/{id:guid}` | `employers.view`, same office |
| `POST` | `/api/office/employers/{id:guid}/documents/{documentId:guid}/approve` | `employers.review` |
| `POST` | `/api/office/employers/{id:guid}/documents/{documentId:guid}/reject` | `employers.review` |
| `POST` | `/api/office/employers/{id:guid}/approve` | `employers.approve` |
| `POST` | `/api/office/employers/{id:guid}/reject` | `employers.reject` |
| `POST` | `/api/office/employers/{id:guid}/stop-cooperation` | `complaints.decide` (RBAC_PERMISSIONS.md footnote 5) — **not** `employers.suspend`, which is seeded but unused |
| `POST` | `/api/office/employers/{id:guid}/resume-cooperation` | `complaints.decide` |

`EmployerProfileDto` additionally carries `cooperationStatus`, `cooperationStoppedReason`/
`cooperationStoppedAt` (WORKFLOWS_AND_STATE_MACHINES.md §4 — independent of `verificationStatus`,
always `Active`/`null` until stopped) and `validComplaintCount` (same §10.3 surfacing as §3.4,
always 0 on the self-service response).

---

### 3.6 Job advertisements

| Method | Route | Permission |
|---|---|---|
| `GET` | `/api/employer/job-advertisements` (own, paged, filter by status) | `Employer` — own company |
| `GET` | `/api/employer/job-advertisements/{id:guid}` | `Employer` — own, same tenant |
| `POST` | `/api/employer/job-advertisements` | `jobadvertisements.create` |
| `PUT` | `/api/employer/job-advertisements/{id:guid}` | `jobadvertisements.edit`, status `Draft`, `Rejected`, **or `Published`** |
| `POST` | `/api/employer/job-advertisements/{id:guid}/submit` | `jobadvertisements.submit` |
| `POST` | `/api/employer/job-advertisements/{id:guid}/close` | `jobadvertisements.close` |
| `POST` | `/api/employer/job-advertisements/{id:guid}/renew` | `jobadvertisements.renew` — creates a **new** advertisement + new payment (confirmed, WORKFLOWS_AND_STATE_MACHINES.md §5) |
| `GET` | `/api/office/job-advertisements` (review queue, paged, filter by status) | `jobadvertisements.review` |
| `POST` | `/api/office/job-advertisements/{id:guid}/approve` | `jobadvertisements.approve` |
| `POST` | `/api/office/job-advertisements/{id:guid}/reject` | `jobadvertisements.reject` |
| `POST` | `/api/office/job-advertisements/{id:guid}/pause` | `jobadvertisements.pause` |
| `POST` | `/api/office/job-advertisements/{id:guid}/resume` | `jobadvertisements.pause` |
| `POST` | `/api/office/job-advertisements/{id:guid}/payment/confirm` | `advertisementpayments.record` — manual office confirmation (confirmed, §0 item 18) |

**Confirmed (RBAC_PERMISSIONS.md §0 item 16):** `PUT` on a `Published` advertisement is allowed,
but the service moves `Status` back to `UnderReview` as a side effect — `PublishedAt`/`ExpiresAt`
are **not** reset. The ad is excluded from public search while re-review is pending.

```json
// CreateJobAdvertisementDto
{
  "title": "string", "description": "string?", "experienceRequirement": "string?",
  "workingHours": "string?", "address": "string?", "categoryId": "guid?", "city": "string?",
  "employmentType": "FullTime", "salaryMin": 0, "salaryMax": 0, "vacancyCount": 1
}
```

```json
// JobAdvertisementDto (employer/office view — includes everything)
{
  "id": "guid", "title": "string", "status": "Published",
  "publishedAt": "datetime?", "expiresAt": "datetime?",
  "payment": { "paymentStatus": "Paid", "amount": 500.00, "currency": "EGP", "paymentMethod": "Manual", "paidAt": "datetime" }
}
```

```json
// POST /api/office/job-advertisements/{id}/payment/confirm — ConfirmAdvertisementPaymentDto
{ "amount": 500.00, "currency": "EGP" }
// RecordedBy is taken from ICurrentUser server-side, never from the request body.
```

**Implementation note (2026-11-19):** this one action does both the payment confirmation *and*
the `Paid -> Published` transition in a single atomic step — WORKFLOWS_AND_STATE_MACHINES.md §5's
`PaymentPending` state is modeled on the `JobAdvertisementStatus` enum but has no reachable
transition in this slice, since there is no payment gateway and no documented "Employer initiates
payment" endpoint to land there from. Revisit when a real payment gateway is added.

---

### 3.7 Public job search & details (no login required)

This is the Wuzzuf-style surface — see UI_UX_GUIDELINES.md §4 for the filter sidebar this
endpoint's query params drive.

| Method | Route | Auth |
|---|---|---|
| `GET` | `/api/jobs?keyword=&city=&category=&employmentType=&pageIndex=&pageSize=` | Anonymous |
| `GET` | `/api/jobs/{id:guid}` | Anonymous |

**Implementation note (2026-11-19):** no `/api/lookups/job-categories`/`/api/lookups/cities`
endpoints exist — DATABASE_DOCUMENTATION.md §7.1's lookup tables were not built (see that section's
implementation note); `city`/`category` are free-text `Contains` filters on `/api/jobs` instead of
lookup ids.

```
GET /api/jobs?keyword=محاسب&cityId={guid}&categoryId={guid}&experience=1-3&employmentType=FullTime&datePosted=last7days&pageIndex=0&pageSize=20
```

```json
// JobListItemDto — never includes employer contact info (BUSINESS_REQUIREMENTS.md §6.2)
{
  "id": "guid", "title": "string", "employerDisplayName": "string",
  "city": "string", "categoryName": "string", "employmentType": "FullTime",
  "experienceRequirement": "string?", "publishedAt": "datetime", "expiresAt": "datetime"
}
```

```json
// JobDetailsDto (GET /api/jobs/{id})
{
  "id": "guid", "title": "string", "description": "string?", "experienceRequirement": "string?",
  "workingHours": "string?", "address": "string?", "city": "string", "categoryName": "string",
  "employmentType": "FullTime", "salaryMin": 0, "salaryMax": 0, "vacancyCount": 1,
  "employerDisplayName": "string",
  "contactNote": "Contact is coordinated through the Employment Office only."
}
```

`JobDetailsDto` has **no** `employerEmail`/`employerPhone` field at all — not a null one. There is
nothing for a bug or a browser devtools session to expose, because the service layer never fetches
those columns into this DTO (RBAC_PERMISSIONS.md §5 principle applied to public data too).

404 is returned for a job whose `Status` is anything other than `Published` — a draft, rejected, or
expired advertisement id must not even confirm its own existence to an anonymous caller.

---

### 3.8 Job applications

| Method | Route | Permission |
|---|---|---|
| `POST` | `/api/job-seekers/me/applications` | `JobSeeker` — eligibility checked server-side |
| `GET` | `/api/job-seekers/me/applications` (paged) | `jobapplications.viewown` |
| `POST` | `/api/job-seekers/me/applications/{id:guid}/withdraw` | `JobSeeker` — own application (confirmed, RBAC_PERMISSIONS.md §0 item 14) |
| `GET` | `/api/employer/job-advertisements/{adId:guid}/applicants` (paged) | `jobapplications.view`, own ad; CV/contact fields gated per RBAC_PERMISSIONS.md §5 |
| `POST` | `/api/employer/applications/{id:guid}/outcome` | `jobapplications.updateoutcome` |
| `GET` | `/api/office/job-advertisements/{adId:guid}/applicants` | `jobapplications.view`, same office — never payment-gated for office staff |

```json
// CreateJobApplicationDto
{ "jobAdvertisementId": "guid" }
```

Validation on `POST /api/job-seekers/me/applications`: **confirmed** —
`JobSeekerProfile.ReviewStatus == Approved` (§0 item 13), `JobAdvertisement.Status == Published`,
no existing **active** (non-withdrawn) application for the same pair (filtered unique index,
DATABASE_DOCUMENTATION.md §6) → otherwise `409 Conflict`. A Job Seeker may re-apply to the same
advertisement after withdrawing a prior application (confirmed, §0 item 14) — that call simply
creates a new row, since the old one is no longer "active".

```json
// ApplicantListItemDto — before payment
{ "applicationId": "guid", "jobSeekerName": "string", "appliedAt": "datetime", "employerFacingStatus": "NotYetContacted" }
// ApplicantListItemDto — after payment (additional fields present)
{ "...": "...", "phone": "string", "email": "string?", "cvDownloadUrl": "string" }
```

```json
// UpdateApplicationOutcomeDto
{ "outcome": "ContactedInterviewScheduled", "notes": "string?" }
```

**Implementation note (2026-11-19):** `cvDownloadUrl` is a relative, authenticated API path
(`GET /api/employer/applications/{id}/cv`, same Bearer token, re-checks the payment gate on every
call) rather than a signed, short-lived URL — no signed-URL/file-token infrastructure exists yet in
this codebase. It streams the Job Seeker's active `Cv` `JobSeekerDocument` through the existing
`IFileStorage` abstraction.

---

### 3.9 Interviews & reports

**Confirmed (RBAC_PERMISSIONS.md §0 item 21):** the Employer schedules the interview and writes
`InterviewReport`; the office writes `HiringFollowUpReport`, following up with both sides.

| Method | Route | Permission |
|---|---|---|
| `POST` | `/api/applications/{id:guid}/interview` | `interviews.schedule` — **Employer**, own advertisement |
| `PUT` | `/api/applications/{id:guid}/interview` | `interviews.edit` — **Employer** |
| `GET` | `/api/applications/{id:guid}/interview` | `interviews.view` |
| `POST` | `/api/applications/{id:guid}/interview-report` | `interviewreports.create` — **Employer** |
| `GET` | `/api/applications/{id:guid}/interview-report` | `interviewreports.view` |
| `POST` | `/api/applications/{id:guid}/hiring-follow-up` (create, on marking hired) | `hiringfollowupreports.create` — **office** |
| `PUT` | `/api/applications/{id:guid}/hiring-follow-up` (complete) | `hiringfollowupreports.create` — **office**, after following up with both the Employer and the Job Seeker |
| `GET` | `/api/applications/{id:guid}/hiring-follow-up` | `hiringfollowupreports.view` — not in the original inventory, added so Office/Employer can read a single application's follow-up status, matching the §3 matrix's "Employer: own ads only (read)" grant |
| `GET` | `/api/office/hiring-follow-ups/due` (paged, office dashboard feed) | `hiringfollowupreports.view` |
| `POST` | `/api/applications/{id:guid}/evaluation` | `applicantevaluationreports.create` — Employer |
| `GET` | `/api/applications/{id:guid}/evaluation` | `applicantevaluationreports.view` |

A `HiringFollowUpReport` that passes `DueAt` unfiled moves to `Status = Overdue` and notifies the
responsible office staff (confirmed, mandatory for every hire — no stronger enforcement than that
notification). **Implementation note (2026-11-19):** the notification itself isn't sent — no
Notifications system exists yet in this codebase (a known, separately-flagged gap). `Overdue` is
computed at read time on the due-list (`PendingDue` past `DueAt`) rather than written to the
database by a background job, same convention as `JobAdvertisement` expiry.

```json
// CreateInterviewDto
{ "scheduledAt": "datetime", "locationOrMeetingDetails": "string?" }

// CreateInterviewReportDto
{ "attendance": "Attended", "interviewResult": "string?", "employerEvaluation": "string?",
  "strengths": "string?", "concerns": "string?", "recommendation": "string?", "notes": "string?" }

// CreateApplicantEvaluationDto
{ "outcome": "Suitable", "notes": "string?" }
```

---

### 3.10 Complaints & suggestions

| Method | Route | Permission |
|---|---|---|
| `POST` | `/api/complaints` | `complaints.create` |
| `GET` | `/api/complaints/mine` (paged) | `complaints.viewown` |
| `GET` | `/api/office/complaints` (paged, filter by status/validity/subjectType) | `complaints.view` |
| `GET` | `/api/office/complaints/{id:guid}` | `complaints.view`, same office — opening a `New` complaint implicitly moves it to `UnderReview`, same convention as `GET /api/office/job-seekers/{id:guid}` |
| `POST` | `/api/office/complaints/{id:guid}/triage` (set `ValidityDecision`) | `complaints.triage` |
| `POST` | `/api/office/complaints/{id:guid}/resolve` | `complaints.resolve` |
| `POST` | `/api/office/complaints/{id:guid}/decide` (fires `OfficeDecision`) | `complaints.decide` — **`OfficeAdmin` only** (confirmed, RBAC_PERMISSIONS.md §0 item 8) |
| `POST` | `/api/suggestions` | `suggestions.create` |
| `GET` | `/api/suggestions/mine` (paged) | `suggestions.viewown` |
| `GET` | `/api/office/suggestions` (paged) | `suggestions.view` |
| `POST` | `/api/office/suggestions/{id:guid}/respond` | `suggestions.respond` |

```json
// CreateComplaintDto
{ "subjectType": "Employer", "subjectId": "guid", "complaintText": "string" }

// TriageComplaintDto
{ "validityDecision": "Valid" }

// ResolveComplaintDto
{ "outcome": "Resolved", "resolutionNotes": "string" }
// outcome must be Resolved or Rejected; only legal from UnderReview/WaitingForInformation.

// DecideEnforcementDto
{ "decisionType": "Suspend", "reason": "string" }
// Requires the complaint's ValidityDecision already be Valid. Response data is the updated
// JobSeekerProfileDto (§3.4) — decide changes the subject, not the complaint. This slice only
// supports a JobSeeker subject; an Employer subject returns a Validation error (Employer
// enforcement isn't built yet).
```

Implemented as: both `POST /api/office/job-seekers/{id}/suspend` (§3.4) and
`POST /api/office/complaints/{id}/decide` route through the same
`JobSeekerReviewService.SuspendAsync`/`ReinstateAsync` — one underlying code path, one
`OfficeDecision` audit row per suspension, reached from either URL. Read "the only endpoint that
may change `JobSeekerProfile.ReviewStatus` to `Suspended`" below as "the only code path", not
literally one route.

`POST /api/office/complaints/{id}/decide` is the **only** endpoint that may change
`JobSeekerProfile.ReviewStatus` to `Suspended` or `Employer.CooperationStatus` to `Stopped` — the
3-strike alert (WORKFLOWS_AND_STATE_MACHINES.md §10.3) only ever produces a notification/task that
points a staff member at this endpoint; it never calls it automatically.

---

### 3.11 Notifications

| Method | Route | Permission |
|---|---|---|
| `GET` | `/api/notifications/mine` (paged, filter `unreadOnly`) | `notifications.viewown` |
| `POST` | `/api/notifications/{id:guid}/read` | `notifications.viewown` |
| `POST` | `/api/notifications/read-all` | `notifications.viewown` |

---

### 3.12 Dashboards & reports

| Method | Route | Permission |
|---|---|---|
| `GET` | `/api/office/dashboard` | `dashboards.view` (office-scoped aggregate counts, BUSINESS_REQUIREMENTS.md §14.1) |
| `GET` | `/api/employer/dashboard` | `dashboards.view` (own-company aggregate counts, §14.2) |
| `GET` | `/api/job-seekers/me/dashboard` | `dashboards.view` (own-profile summary, §14.3) |
| `GET` | `/api/admin/dashboard` | `dashboards.view`, `SuperAdmin` (platform aggregate counts only — no document/content access, RBAC_PERMISSIONS.md §0.3) |

All dashboard endpoints return **counts and aggregates only** — no endpoint returns a raw list of
sensitive records under the `dashboard` route; a dashboard's "view details" action always links to
the proper paged/permission-checked list endpoint above.

---

## 4. Idempotency & concurrency notes

- Payment-confirmation callbacks (`advertisementpayments.record`) must be idempotent — the same
  provider webhook can arrive more than once; a second call against an already-`Paid` payment
  returns `200` with the existing state, not a duplicate side effect.
- `PUT` endpoints that edit a record while another staff member might be editing it concurrently
  (profile/document review, advertisement edit) should carry a concurrency token (`ModifiedDate`
  or a `RowVersion`) and return `409 Conflict` on a stale write, mapped from
  `DbUpdateConcurrencyException` (BACKEND_GUIDELINES §9, rule 5).

---

*Update this document in the same change that adds or changes a controller action — an endpoint
that exists in code but not here is undocumented, not "self-explanatory".*
