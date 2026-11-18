# UI/UX Guidelines — Church Employment Office Platform

**Status:** Draft for product-owner / design review
**Reference inspiration:** [wuzzuf.net/jobs/egypt](https://wuzzuf.net/jobs/egypt) — job-search layout,
filter sidebar, and job-card pattern, adapted to a trust-mediated, office-gated platform rather
than an open job board.
**Depends on:** [BUSINESS_REQUIREMENTS.md](./BUSINESS_REQUIREMENTS.md) §4, §19
**Conventions from:** [PROJECT_ARCHITECTURE_FRONTEND.md](./PROJECT_ARCHITECTURE_FRONTEND.md) §10, §15, §16, §18

> The platform borrows Wuzzuf's **information architecture** (search + filter sidebar + job card +
> details page) because it is a well-understood pattern for this market — not its open,
> unmoderated listing model. Every screen below still has to communicate "this is reviewed and
> mediated by a trusted church office," not "anyone can post anything."

---

## 1. Design principles

1. **Trust before volume.** The platform shows fewer, vetted listings — the UI should read as
   calm and curated, not as a dense marketplace. Avoid Wuzzuf's very high information density on
   the card itself; give each job card a bit more breathing room.
2. **Arabic-first, RTL-first.** Primary audience reads Arabic; English is a secondary/admin
   convenience. Build and test RTL first, mirror into LTR — not the other way around
   (PROJECT_ARCHITECTURE_FRONTEND §16).
3. **Status is always visible, never ambiguous.** Every entity with a state machine
   (WORKFLOWS_AND_STATE_MACHINES.md) gets a consistent, color-coded `StatusTag` wherever it
   appears in a list or detail view.
4. **Privacy is visible, not just enforced.** When data is withheld (employer contact in a public
   listing, applicant CV before payment), the UI shows a deliberate "locked" affordance explaining
   *why* — never a field that's just silently missing (see §7).
5. **One UI kit, one icon set, one notification system, one date library** — no exceptions
   (PROJECT_ARCHITECTURE_FRONTEND §21).

---

## 2. Visual foundation

### 2.1 Color tokens

Semantic names only — no raw hex in components (PROJECT_ARCHITECTURE_FRONTEND §15). Confirmed
palette, matching the approved logo (see assets/brand/README.md):

| Token | Light | Usage |
|---|---|---|
| `color-primary` | `#0A66C2` (confirmed with the logo, 2026-11-18) | Primary actions, links, active nav |
| `color-primary-dark` | `#072F5F` | The logo's cross color; header/footer dark backgrounds, pressed states |
| `color-primary-accent` | `#70B5F9` (used sparingly) | The logo's briefcase/flag accent; highlights that need to feel deliberate — e.g. a featured/verified badge, not every button |
| `color-surface` | `#FFFFFF` | Cards, panels |
| `color-surface-muted` | `#F6F7F8` | Page background, filter sidebar background |
| `color-text` / `color-text-muted` | near-black / mid-gray | Body text / secondary text |
| `color-success` | green | `Approved`, `Published`, `Paid`, `Resolved` |
| `color-warning` | amber | `Submitted`, `UnderReview`, `PendingDue`, `WaitingForInformation` |
| `color-danger` | red | `Rejected`, `Suspended`, `Stopped`, `Overdue`, `Expired` |
| `color-neutral` | gray | `Draft`, `Closed`, `Cancelled`, `NotContacted` |
| `color-info` | blue | `ContactedInterviewScheduled`, informational notices |

Define each in `app/styles/theme.ts`, feeding both the UI-kit theme and CSS variables
(PROJECT_ARCHITECTURE_FRONTEND §15), with a dark-mode set even if dark mode isn't launched day one
— cheap to add now, expensive to retrofit.

### 2.2 Typography

- **Arabic:** a well-hinted, self-hosted Arabic-Latin pairing (e.g. **IBM Plex Sans Arabic** or
  **Noto Kufi Arabic** for headings + **Noto Naskh/Cairo** for body — pick one pairing and commit;
  avoid mixing more than 2 families). Favor a pairing that reads as institutional/trustworthy
  rather than as a startup product, consistent with the "trust before volume" principle in §1.
- **Latin fallback:** a geometric sans that pairs visually with the Arabic choice (e.g. **Inter** or
  **IBM Plex Sans**).
- **Scale:** a standard type scale (12/14/16/18/20/24/32px) — job titles at 18–20px on cards,
  24–32px on the details page header; never smaller than 14px for body copy, 12px only for
  metadata (posted date, document filename).

### 2.3 Layout & breakpoints

Mobile-first, matching PROJECT_ARCHITECTURE_FRONTEND §15: `sm` ≥576px, `md` ≥768px, `lg` ≥1024px,
`xl` ≥1280px. Job search page: single column on `sm`/`md` (filters collapse into a drawer), two
columns (filters + results) from `lg` up, matching Wuzzuf's desktop layout.

---

## 3. Shared UI kit (PROJECT_ARCHITECTURE_FRONTEND §10.3, extended for this domain)

| Component | Purpose |
|---|---|
| `PageHeader`, `DataTable`, `FormField`, `IconButton`, `ConfirmAction`, `SearchInput`, `StatusTag`, `EmptyState`, `ErrorState`, `LoadingState`, `Can` | Generic kit, unchanged from the architecture baseline. |
| `StatusTag` — **domain-bound variant** | Takes a `statusDomain` (`jobSeekerReview`, `employerVerification`, `advertisement`, `payment`, `applicationEmployerFacing`, `applicationJobSeekerFacing`, `complaint`, `suggestion`) + the enum value, and resolves the correct color/label/translation key from one lookup table — so a status never gets a color by accident of CSS class order. |
| `JobCard` | The search-results row (see §4.2). |
| `JobFilterSidebar` | Facet filters bound to URL search params (see §4.1). |
| `GatedField` | Renders a blurred/locked placeholder with a reason + CTA when a value the API omitted would normally render here (see §7). |
| `DocumentUploadField` | File picker + progress + `DocumentReviewStatus` tag + rejection-reason inline message, used for all 6 document types across Job Seeker/Employer flows. |
| `ApplicantCard` | Employer's per-applicant row — renders `GatedField` for CV/contact until paid. |
| `ReviewQueueRow` | Office staff's generic review-list row (job seeker / employer / advertisement), with inline Approve/Reject actions + `ConfirmAction` + reason textarea on reject. |
| `ComplaintThreadPanel` | Complaint detail: text, validity decision selector, resolution notes, linked `OfficeDecision` history. |
| `DashboardStatCard` | One metric tile (BUSINESS_REQUIREMENTS.md §14) — number, label, trend/delta optional, links to the filtered list behind it. |

---

## 4. Key screens

### 4.1 Public job search (home) — Wuzzuf-equivalent

```
┌─────────────────────────────────────────────────────────────────┐
│ Header: logo · [search input: keyword]  [city select]  [Search] │
│          nav: Browse Jobs · Login · Register        [AR/EN]      │
├───────────────┬───────────────────────────────────────────────────┤
│ Filter sidebar│  Sort: [Most relevant ▾]       1,245 jobs found    │
│ - Category    │  ┌───────────────────────────────────────────┐   │
│ - City        │  │ JobCard: title · company · city · tags     │   │
│ - Experience  │  │          posted 3 days ago · [Save]        │   │
│ - Job type    │  └───────────────────────────────────────────┘   │
│ - Salary      │  ┌───────────────────────────────────────────┐   │
│ - Date posted │  │ JobCard ...                                 │   │
│ [Clear all]   │  └───────────────────────────────────────────┘   │
│               │  « Pagination »                                   │
└───────────────┴───────────────────────────────────────────────────┘
```

- Filters are **URL search params** (PROJECT_ARCHITECTURE_FRONTEND §11), not component state —
  shareable/bookmarkable search, matching Wuzzuf's own URL-driven filters.
- Filter options (`category`, `city`) come from the lookup endpoints in API_DOCUMENTATION.md §3.7
  — never hard-coded in the frontend.
- Every job card and the details page explicitly **never** renders an employer phone/email —
  there is nothing to hide because `JobListItemDto`/`JobDetailsDto` never carry that field
  (API_DOCUMENTATION.md §3.7). Replace Wuzzuf's "easy apply"/external-link affordance with a single
  **"Apply through the Employment Office"** button that requires login.
- Mobile: filter sidebar becomes a bottom-sheet triggered by a "Filters" button above the results,
  same pattern Wuzzuf uses on mobile.

### 4.2 `JobCard` anatomy

```
┌───────────────────────────────────────────────────────────┐
│ [Category icon]  Job Title                    [Save ♡]     │
│                   Company display name · City              │
│                   [Experience tag] [Job type tag]           │
│                   Posted 3 days ago                         │
└───────────────────────────────────────────────────────────┘
```

No employer logo upload in scope yet (not in BUSINESS_REQUIREMENTS.md) — use a category icon or a
generated initial-avatar placeholder instead of leaving an empty image slot.

### 4.3 Job details page

Header (title, company display name, city, category, Apply button — sticky on mobile) → overview
panel (experience, working hours, employment type, vacancies, salary if present) → description →
"How this works" note explaining office-mediated contact — this last block is new relative to
Wuzzuf and is there specifically to set the right expectation (BUSINESS_REQUIREMENTS.md §6.2).

### 4.4 Job Seeker dashboard

Sections per BUSINESS_REQUIREMENTS.md §14.3 / §19: profile completion meter, verification
`StatusTag`, missing-document callout (`EmptyState` variant pointing at the upload screen), **My
Applications** table (job title, applied date, `StatusTag[applicationJobSeekerFacing]`, last
update, interview date if scheduled and office allows it), notifications list.

### 4.5 Employer dashboard & create-advertisement

- Dashboard: `DashboardStatCard` row (BUSINESS_REQUIREMENTS.md §14.2) + advertisements table with
  `StatusTag[advertisement]`.
- **Create/Edit Advertisement** is one component, mode from the route, per
  PROJECT_ARCHITECTURE_FRONTEND §10.4 ("Create / Edit" pattern) — not two near-duplicate forms.
  **Confirmed (RBAC_PERMISSIONS.md §0 item 16):** editing an already-`Published` ad through this
  same form sends it back to `UnderReview` — the form must say so before the Employer confirms the
  save (e.g. *"سيتم مراجعة الإعلان مرة أخرى بعد التعديل"* / "the ad will be re-reviewed after this
  edit"), not just silently change the status.
- **Payment step (confirmed, §0 item 18 — manual office confirmation, v1):** once approved, the
  advertisement's status card shows *"بانتظار تأكيد الدفع من المكتب"* ("awaiting payment
  confirmation from the office") with the fee amount in **EGP** and instructions for how to pay
  the office offline (visit / bank transfer / whatever the office's own process is — this is
  content the office configures, not a checkout flow the platform processes). There is **no
  "Pay now" button** in v1 — the Employer pays the office directly, and an office staff member
  flips the status from their side (`POST .../payment/confirm`). Design the screen so a future
  online-gateway "Pay now" button can replace this block without touching anything else on the
  page.
- **Applicants list** per advertisement: `ApplicantCard` list. Before payment, every card shows a
  `GatedField` instead of phone/CV (§7). A persistent banner above the list: *"ادفع رسم الإعلان من
  خلال المكتب لعرض بيانات المتقدمين وملفات الـ CV"* ("pay the advertisement fee through the office
  to view applicant data and CVs") — this is the single most important "visible privacy" moment in
  the product. **Confirmed (§0 item 9):** once unlocked, the card shows the phone number directly
  and the Employer is expected to contact the applicant themselves — no in-app messaging/relay UI
  is needed for v1.

### 4.6 Office review queues

A generic **list → details → decide** pattern reused for Job Seekers, Employers, and
Advertisements:

- List: `ReviewQueueRow` items, filterable by status, with a pending-count badge in the nav
  (matches BUSINESS_REQUIREMENTS.md §19's "Employment Office" nav: *Job Seeker Management*,
  *Employer Management*, *Job Advertisement Review*). An advertisement that returned to
  `UnderReview` from `Published` (an Employer's edit) appears in the same queue as a brand-new
  submission — the UI should distinguish it with a small "re-review" badge so staff know it was
  already live once.
- Details: full profile/company/ad content + document viewer (in-app preview for PDFs/images,
  never a raw downloadable public link) + Approve / Reject (reason required) actions. For a
  `PaymentPending` advertisement, the details view carries the **"Confirm Payment"** action
  (`advertisementpayments.record`) with an amount field pre-filled from the configured EGP fee.
- Complaints: `ComplaintThreadPanel` + the 3-strike alert surfaced as a dashboard notice, **not**
  as an automatic action (WORKFLOWS_AND_STATE_MACHINES.md §10.3) — the UI must make it obvious that
  clicking "Suspend" is a deliberate decision with a reason field, not a confirmation of something
  already done. **Confirmed:** only an `OfficeAdmin` sees this action enabled; `OfficeStaff` sees
  the alert but a disabled/explained button if they lack `complaints.decide`.

### 4.7 Super Admin

Office list/create/edit, office-admin invite flow, platform dashboard (`DashboardStatCard`s only —
counts, no drill-down into document/complaint content per RBAC_PERMISSIONS.md §0.3).

---

## 5. Status color mapping (single source of truth for `StatusTag`)

| Status domain | Value | Color token |
|---|---|---|
| Job Seeker review | `Draft` | `neutral` |
| | `Submitted`, `UnderReview` | `warning` |
| | `Approved` | `success` |
| | `Rejected`, `Suspended` | `danger` |
| Employer verification | same pattern as above | — |
| Employer cooperation | `Active` | `success` |
| | `Stopped` | `danger` |
| Advertisement | `Draft`, `Cancelled` | `neutral` |
| | `SubmittedForReview`, `UnderReview`, `ApprovedAwaitingPayment`, `PaymentPending` | `warning` |
| | `Published`, `Paid` | `success` |
| | `Rejected`, `Expired`, `Closed` | `danger` / `neutral` respectively — `Expired`/`Closed` use `neutral` (lifecycle end, not a failure), `Rejected` uses `danger` |
| | `Paused` | `warning` |
| Application (employer-facing) | `NotYetContacted` | `neutral` |
| | `ContactedInterviewScheduled` | `info` |
| | `ContactedRefused`, `NoResponseAfterContact` | `warning` |
| Application (job-seeker-facing) | `NotContacted` | `neutral` |
| | `ContactedNotSuitable` | `warning` |
| | `ContactedInterviewScheduled` | `info` |
| Complaint | `New` | `info` |
| | `UnderReview`, `WaitingForInformation` | `warning` |
| | `Resolved` | `success` |
| | `Rejected`, `Closed` | `neutral` |
| Suggestion | `New` | `info` |
| | `UnderReview` | `warning` |
| | `Responded`, `Closed` | `success` / `neutral` |

This table is the implementation spec for the `StatusTag` lookup in §3 — keep it in one place so a
new status value added to DATABASE_DOCUMENTATION.md §3–§7 has an obvious place to get a color.

---

## 6. Forms

- React Hook Form + Zod schema per form, per feature (PROJECT_ARCHITECTURE_FRONTEND §14).
- Registration (Job Seeker / Employer) is a **multi-step form**: **mobile number + OTP
  verification** (confirmed, RBAC_PERMISSIONS.md §0 item 10) → profile → documents → review &
  submit — mirrors BUSINESS_REQUIREMENTS.md §5 plus the OTP step, with a stepper component and the
  ability to save-as-draft between steps. The OTP step is a short 6-digit code input with a resend
  timer; never let the user proceed past it without a verified `otpToken`
  (API_DOCUMENTATION.md §3.1).
- Document upload step: `DocumentUploadField` per required document, inline validation (file
  type/size from the configured limit, §8.1 open), drag-and-drop + manual browse, clear "why we
  need this" microcopy per document (recommendation letter / national ID / CV wording tied to the
  actual Arabic terms in BUSINESS_REQUIREMENTS.md §5.1).
- Server field errors map onto the matching field; a form-level alert (`ErrorState`) catches
  anything that isn't field-specific.
- Unsaved-changes prompt on the create/edit-advertisement page.

---

## 7. The "gated" pattern (payment-gated applicant data)

This is a UI pattern specific to this product, not covered by the generic architecture doc, so it
gets its own spec:

```
┌────────────────────────────────────────────────┐
│ 🔒  Ahmed M.                                    │
│     Applied 2 days ago                          │
│     [Phone and CV hidden until payment]         │
│     [ادفع 500 جنيه من خلال المكتب لفتح البيانات] │
└────────────────────────────────────────────────┘
```

- The lock icon + explanatory microcopy is **always shown together** — never a bare blank space,
  which would look like a bug rather than a deliberate gate.
- **Confirmed (§0 item 18): the CTA leads to the ad's "awaiting payment confirmation" status
  screen (§4.5), not a checkout page** — there is no online payment flow in v1, so the CTA's job is
  to explain the offline/manual process and show the current `PaymentStatus`, not to collect a
  card.
- Once unlocked, the same card re-renders with phone/CV present — this is a **different payload**
  from the API (API_DOCUMENTATION.md §3.8), not a client-side reveal/hide of data that was already
  in memory. There must be no state where the browser ever held the gated data before payment.
- Apply the identical pattern anywhere else a payment/approval gate hides data (none currently
  planned beyond this one, but keep the component generic (`GatedField`) rather than bespoke to
  applicants, in case a second gate is added later).

---

## 8. Internationalization & RTL

**Confirmed (RBAC_PERMISSIONS.md §0 item 22): Arabic-only, RTL.** No language switcher in the
header for v1 — there is only one language to switch to.

- `dir="rtl"` fixed (not just default) on `<html>`; mirrored icons for direction-dependent ones
  (chevrons, back/forward) per PROJECT_ARCHITECTURE_FRONTEND §16.
- Logical CSS properties throughout (`ms`/`me`/`ps`/`pe`/`start`/`end`) — the filter sidebar in
  particular must not hard-code `left`/`right`. Keep this discipline even though there's no LTR
  mode to switch to yet: it's what makes adding English later (if ever asked for) a translation
  task, not a layout rewrite.
- Status labels, document-type labels, and all microcopy are still translation keys, not inline
  strings (PROJECT_ARCHITECTURE_FRONTEND §16, §21) — a single `ar` namespace set is enough for
  v1, but keeping the i18n plumbing in place costs nothing and avoids a bigger rewrite if English
  is ever requested.
- Date/number formatting through `Intl` helpers with an Arabic (`ar-EG`) locale. Eastern vs
  Western Arabic numerals preference for office staff is not covered in BUSINESS_REQUIREMENTS.md —
  confirm with the product owner if it comes up; defaulting to Western (0–9) digits is the safer
  default for data-entry-heavy office screens.

---

## 9. Accessibility checklist (WCAG 2.2 AA, per PROJECT_ARCHITECTURE_FRONTEND §18.1)

- [ ] Every `IconButton` (save job, close drawer, remove filter chip) has an accessible label.
- [ ] `FormField` wires `aria-describedby`/`aria-invalid` for every validation error, in both
      languages.
- [ ] Filter sidebar and its mobile drawer are fully keyboard-operable; focus returns to the
      "Filters" trigger button on close.
- [ ] `GatedField`'s lock state is announced to screen readers (not just a visual blur) —
      e.g. `aria-label="Hidden until the advertisement fee is paid"`.
- [ ] Color is never the only signal for status — `StatusTag` always pairs color with a text
      label.
- [ ] Contrast ≥ 4.5:1 for the confirmed primary/accent palette in §2.1 — `color-primary-accent`
      (`#70B5F9`) is light and must not carry body text; it's for fills/icons/borders only.

---

## 10. Open items affecting this document

Most of what used to be open here is now confirmed (RBAC_PERMISSIONS.md §0) — Arabic-only/RTL,
manual/office-confirmed payment with no "Pay now" button, direct Employer-to-applicant contact
after payment, and a separate native app planned for later with no impact on this web spec. What's
genuinely still open:

| Open item | Why it matters here |
|---|---|
| §24.D.22 (exact fee amount) | The `500 جنيه` figure on the payment CTA (§7) and the status screen (§4.5) is illustrative only — swap in the real amount once set. |
| Arabic numeral style | Eastern vs Western Arabic numerals for office-staff screens (§8) — not asked yet, low risk either way. |
| Appeal process | If the product owner later defines a formal appeal path for a suspended Job Seeker or blocked company (DATABASE_DOCUMENTATION.md §8), this document needs a corresponding screen — none is designed yet since no such flow exists today beyond submitting a new complaint/suggestion. |

---

*Update this document together with `app/styles/theme.ts` and `shared/ui` whenever a token, status
mapping, or shared component changes — a design decision that only lives in code drifts from what
the next screen copies.*
