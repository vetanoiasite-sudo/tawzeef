# Church Employment Office Platform

## Product Requirements Document (PRD) & Business/Functional Specification

**Document status:** Initial business specification\
**Version:** 1.0\
**Prepared for:** Product planning and implementation with Claude Code\
**Language:** Arabic business explanation with standard English
technical terminology\
**Important:** This document describes the requirements explicitly
provided by the product owner. Any item marked **\[قرار مطلوب\]** is not
yet defined and must be confirmed before implementation. Claude must not
silently invent business rules for these items.

------------------------------------------------------------------------

# 1. Purpose of This Document

This document is the single source of truth for understanding,
designing, implementing, testing, and evolving the Church Employment
Office Platform.

The platform connects: 1. **Employment Office** -- reviews and manages
job seekers, employers, job advertisements, applications, reports,
complaints, and suggestions. 2. **Job Seeker** -- creates a profile,
submits required documents, searches for jobs, applies, and follows
application progress. 3. **Employer** -- registers a company, submits
legal documents, publishes paid job advertisements, reviews applicants,
updates applicant outcomes, and submits evaluation reports.

The platform must also support a **Super Admin** who can manage multiple
independent employment offices from one system.

Claude Code must read this document before planning or changing the
application. It must not begin implementation based on assumptions where
a decision is marked as pending.

# 2. Product Vision

Build a trusted, controlled employment platform operated by church
employment offices. The platform is not an unrestricted job board. The
Employment Office is the trusted intermediary between job seekers and
employers.

Core principles: - Job seekers and employers must be reviewed and
approved by the Employment Office before being allowed to use the
relevant protected features. - Job seeker contact information and CVs
must not be publicly exposed. - Employer contact information must not be
shown in public job advertisements. - Communication between a job seeker
and an employer is coordinated through the Employment Office. -
Employers pay a subscription/advertising fee for each job
advertisement. - A job advertisement is displayed for **15 days**. - The
Employment Office controls approval, rejection, suspension, and
complaint-related decisions. - Multiple Employment Offices must be
supported with strict separation of their data. - A Super Admin oversees
the overall platform and manages offices.

# 3. Scope

## 3.1 Included in the initial product scope

-   Multi-tenant platform supporting multiple Employment Offices.
-   Super Admin management of Employment Offices.
-   Job seeker registration, profile, document upload, review, approval,
    rejection, and suspension.
-   Employer/company registration, legal document upload, review,
    approval, and rejection.
-   Job advertisement creation, review, payment/subscription handling,
    publication, and expiry after 15 days.
-   Job search and job details.
-   Job applications and application status tracking.
-   Employer review of applicants after the required payment/access
    condition is met.
-   Employer applicant outcome updates.
-   Interview and post-hiring follow-up reports.
-   Complaints and suggestions.
-   Complaint thresholds and office-controlled enforcement.
-   Notifications and audit history.
-   Administrative dashboards and reports.

## 3.2 Not defined yet / potentially later scope

The following must not be assumed to exist in the first release unless
the product owner confirms: - Online chat or direct messaging. - Video
interviews. - CV parsing or AI candidate matching. - Automated payment
gateway integration. - Mobile applications versus responsive web
application. - SMS, WhatsApp, or email provider integration. - Public
employer/company pages. - Ratings and reviews. - Automated identity
verification. - Multiple languages. - Job seeker subscription plans. -
Employer subscription packages covering multiple advertisements.

# 4. Actors, Roles, and Permissions

## 4.1 Super Admin

The Super Admin is the platform-level administrator.

Responsibilities: - Create, edit, activate, deactivate, and manage
Employment Offices. - Manage office-level administrator accounts and
permissions. - View platform-level operational information. - Access
tenant-level information only according to explicitly assigned
permissions and platform policy. - Configure global settings where
applicable. - Monitor office activity and system-level statistics.

Important: - A Super Admin is not the same as an Employment Office
Admin. - Each Employment Office must have its own isolated data and
operational scope. - The exact ability of the Super Admin to view
personal documents, CVs, complaints, and reports requires a privacy
decision. See Section 24.

## 4.2 Employment Office Admin / Office Staff

The Employment Office is responsible for trust, moderation, and
coordination.

Responsibilities: - Review and approve/reject job seeker registrations
and documents. - Review and approve/reject employer registrations and
legal documents. - Review and approve/reject job advertisements. -
Manage job seekers, employers, jobs, and applications within the
office. - Coordinate communication between job seekers and employers. -
Monitor application progress. - Review interview and hiring follow-up
reports. - Receive and handle complaints and suggestions. - Decide
whether complaint thresholds should lead to suspension or stopping
cooperation. - Add internal notes and record decisions. - Manage
office-level operational settings if permitted.

The system should support permission separation between office staff
members if required. **\[قرار مطلوب\]** Define whether the first version
has one office admin role only or several staff roles (reviewer,
complaint manager, finance, read-only, etc.).

## 4.3 Job Seeker

A Job Seeker can: - Browse/search available job advertisements. - View
public job details. - Register/login. - Complete personal profile
information. - Upload the required documents. - Submit profile for
Employment Office review. - See profile verification/review status. -
Apply for available jobs, subject to approval and eligibility rules. -
View jobs they applied to. - Track the status of each application. -
Submit complaints and suggestions. - Receive notifications about profile
review, application updates, interviews, and other relevant events.

A Job Seeker must not: - See an employer's private phone number or
direct contact details. - Access another job seeker's CV or private
documents. - Bypass the Employment Office to contact employers through
platform-disclosed contact details. - Modify an application decision
made by the employer or office.

## 4.4 Employer

An Employer represents a company/business, not merely an unverified
individual.

An Employer can: - Register a company account. - Submit company and
representative information. - Upload required legal documents. - Submit
the company profile for review. - View approval/rejection status. -
Create job advertisements. - Submit advertisements for review. - Pay the
required fee for each advertisement, according to the configured payment
process. - View applicants for an advertisement only after the required
advertisement payment/access condition is satisfied. - View applicant
CVs and contact details only when access has been granted under the
approved business rules. - Update applicant contact/interview
outcomes. - Submit applicant evaluation reports. - Submit complaints and
suggestions.

An Employer must not: - View applicants' CVs or phone numbers before the
required payment/access condition. - Publish a job advertisement with
direct employer contact information visible to job seekers. - Access
applicants belonging to another employer's advertisement. - Access
another employer's company documents or reports.

------------------------------------------------------------------------

# 5. Registration and Verification

## 5.1 Job Seeker Registration

### Step 1 -- Discover jobs

A visitor can browse and search job advertisements without necessarily
creating an account.

### Step 2 -- Authentication

When the visitor chooses to apply, the system requires
login/registration.

**\[قرار مطلوب\]** Confirm whether registration uses: - Email and
password; - Mobile number and OTP; - Both; - Another authentication
provider.

### Step 3 -- Personal information

The Job Seeker fills in a personal profile. The exact mandatory fields
must be finalized. Suggested fields for confirmation: - Full name. -
Mobile number. - Email address, if used. - Address. -
City/governorate. - Date of birth or age, if required. - Education. -
Qualification. - Years of experience. - Skills. - Career summary. -
Preferred job categories. - Any additional office-specific fields.

These are suggested fields, not all confirmed mandatory requirements.

### Step 4 -- Required document uploads

The Job Seeker must upload: 1. **Recommendation letter (جواب تزكية)**
from their Father of Confession (أب الاعتراف). 2. **Personal ID card
image** (صورة البطاقة الشخصية). 3. **CV / Resume** (السيرة الذاتية).

The system should: - Store each document securely. - Associate every
file with its owner and Employment Office tenant. - Record upload time
and document type. - Allow authorized office staff to review the
files. - Show the Job Seeker the document/review status. - Prevent
public access to private documents. - Validate allowed file formats and
file size according to configurable limits. **\[قرار مطلوب\]**

### Step 5 -- Submit for review

The Job Seeker submits their profile and documents to the Employment
Office.

Possible profile review states (proposed state model): - Draft /
Incomplete. - Submitted / Pending Review. - Under Review. - Approved. -
Rejected. - Suspended.

The office reviews the profile and documents and makes the
approval/rejection decision.

### Step 6 -- Office decision

If approved: - The Job Seeker becomes eligible to use the approved job
application features. - The account/profile status is updated. - The Job
Seeker is notified.

If rejected: - The rejection reason should be recorded. - The Job Seeker
should be notified. - The Job Seeker should be able to see what action
is required, if resubmission is allowed.

**\[قرار مطلوب\]** Can a rejected Job Seeker edit and resubmit
documents? Is there a limit on resubmissions? Can an office suspend a
previously approved Job Seeker?

## 5.2 Employer / Company Registration

### Required company documents

The Employer must upload: 1. **Commercial Register** (صورة من السجل
التجاري). 2. **Tax Card** (صورة من البطاقة الضريبية). 3. **National ID
card of one of the company owners listed in the Commercial Register**
(صورة البطاقة الشخصية لأحد ملاك الشركة المسجل في السجل التجاري).

### Company profile information

The exact fields must be confirmed. Suggested information: - Company
legal name. - Company display name. - Company address. -
City/governorate. - Business activity/industry. - Company email. -
Company phone number. - Name of authorized representative. -
Representative's role. - Any additional contact information required by
the office.

### Employer verification workflow

1.  Employer creates an account.
2.  Employer enters company information.
3.  Employer uploads all three required documents.
4.  Employer submits the company profile for review.
5.  Employment Office reviews the submitted information and documents.
6.  Office approves or rejects the company.
7.  Employer receives the decision and, if rejected, the recorded
    reason.

Suggested company statuses: - Draft / Incomplete. - Submitted / Pending
Review. - Under Review. - Approved. - Rejected. - Suspended. -
Deactivated.

Only approved employers should be able to proceed to the protected
job-advertisement workflow.

**\[قرار مطلوب\]** Confirm whether an employer can update legal
documents after approval and whether any update triggers re-verification
or temporary restriction.

------------------------------------------------------------------------

# 6. Job Advertisement Management

## 6.1 Job Advertisement Information

An Employer creates a job advertisement containing the job details.

Confirmed information from the product owner: - Job title. - Required
experience. - Working hours. - Address/location. - Job category. -
City. - Contact is through the Employment Office only; no employer phone
number is displayed in the advertisement. - Other relevant job details.

Suggested additional fields to confirm: - Job description. -
Responsibilities. - Required qualifications/education. - Required
skills. - Employment type (full-time, part-time, contract, etc.). -
Salary or salary range. - Number of vacancies. - Gender requirement,
only if lawful and appropriate. - Work location details. - Working
days. - Benefits. - Application deadline. - Special requirements.

Do not make suggested fields mandatory until confirmed.

## 6.2 No Direct Employer Contact in Public Listings

This is a critical business rule: - Public job details must never
display the Employer's private phone number. - Public job details must
never expose the Employer's direct email or other direct contact channel
unless explicitly approved by the office. - Job Seekers communicate with
the Employer through the Employment Office only. - The office
coordinates contact and interview arrangements.

The platform should distinguish: - Public job information; - Private
employer account/contact information; - Internal office contact
information.

## 6.3 Advertisement Review and Publication Workflow

Proposed workflow: 1. Approved Employer creates a job advertisement. 2.
Employer submits it for office review. 3. Office checks the
advertisement content and compliance. 4. Office approves or rejects the
advertisement. 5. If approved, the advertisement proceeds to the
payment/access step. 6. Once payment is confirmed (if payment is
required before publication), the advertisement is published. 7. The
advertisement remains visible for 15 days. 8. At the end of 15 days, it
automatically expires and is removed from active public search results.

**\[قرار مطلوب\]** Confirm the exact order: - Does the Employer pay
before the office reviews the advertisement? - Does the office approve
first and then the Employer pays? - Does the 15-day period begin at
payment confirmation or publication time? - Can the office manually
pause, reject, or remove a live advertisement? - Can an Employer renew
an expired advertisement? Is renewal a new paid advertisement?

## 6.4 Advertisement Lifecycle

Suggested statuses: - Draft. - Submitted for Review. - Under Review. -
Rejected. - Approved -- Awaiting Payment. - Payment Pending. - Paid. -
Scheduled (if future publishing is supported). - Published / Active. -
Paused. - Expired. - Closed / Filled. - Cancelled.

Only use the statuses needed by the confirmed workflow. Store status
history, including actor, timestamp, and reason.

## 6.5 Advertisement Duration

Confirmed rule: - Each advertisement is displayed for **15 days only**.

System behavior: - Save the publication start timestamp. - Calculate the
expiry timestamp from the confirmed start event. - Expire the
advertisement automatically when the 15-day period ends. - Exclude
expired advertisements from active public search. - Preserve expired
advertisements and their application/report history for authorized
users. - Notify the Employer before expiry only if this is approved as a
feature. **\[قرار مطلوب\]**

## 6.6 Paid Advertisement and Applicant Data Access

Confirmed rule: - The Employer pays a fee for each advertisement. -
Applicant CVs and phone/contact information are not exposed to the
Employer until the required subscription/payment for that advertisement
is paid.

The system must enforce this rule at the backend/API level, not merely
hide the information in the frontend.

Before payment/access is confirmed: - Employer may see only the
applicant information explicitly permitted by the office. - CV
download/view must be blocked. - Applicant phone number and private
contact information must be blocked. - Direct file URLs must not be
publicly accessible.

After payment/access is confirmed: - Employer can access applicant CVs
and contact details for the relevant advertisement according to
permissions. - Access must be limited to applicants of that
advertisement. - Every sensitive document access/download should be
logged.

**\[قرار مطلوب\]** Define the fee amount, currency, payment method,
refund rules, and whether payment unlocks applicant data for the entire
advertisement period or another period.

------------------------------------------------------------------------

# 7. Job Search and Job Application

## 7.1 Public Job Search

Visitors and Job Seekers can search/filter active job advertisements.

Confirmed job details include: - Job title. - Experience. - Working
hours. - Address. - Category. - City. - Other approved job details.

Suggested filters to confirm: - City. - Category. - Experience. -
Working hours. - Employment type. - Salary range. - Date posted.

Search results must show only active, published, non-expired
advertisements.

## 7.2 Application Eligibility

The platform must check eligibility before accepting an application.

At minimum, the Job Seeker should have: - A registered account. - A
completed personal profile. - Submitted required documents. - Employment
Office approval, if the office's approval is required before applying.

**\[قرار مطلوب\]** Confirm whether an application is allowed while the
Job Seeker is pending review, or only after approval.

## 7.3 Applying to a Job

Proposed flow: 1. Job Seeker opens a job advertisement. 2. Job Seeker
selects Apply. 3. System validates account/profile approval and
advertisement availability. 4. System creates an application linked to
the Job Seeker and the specific Job Advertisement. 5. System records the
application timestamp. 6. Job Seeker sees the job in their applied-jobs
list. 7. Employer and office can process the application according to
their permissions.

Prevent duplicate applications to the same advertisement unless the
office explicitly allows reapplication.

**\[قرار مطلوب\]** Confirm whether a Job Seeker can withdraw an
application and whether they can apply again after withdrawal.

------------------------------------------------------------------------

# 8. Application Tracking -- Job Seeker View

The Job Seeker must have a section listing every job they applied to and
the current status of each application.

The product owner specified these Job Seeker-facing states: 1. **لم يتم
التواصل** -- Not Contacted. 2. **تم التواصل وير مناسبة** -- Contacted
and not suitable. The exact intended Arabic phrase should be confirmed;
interpreted as "تم التواصل وغير مناسبة". 3. **تم التواصل وتحديد مقابلة
شخصية** -- Contacted and an interview has been scheduled.

The system should display: - Job title. - Company name only if permitted
by the office's privacy policy. - Application date. - Current status. -
Last status update date. - Interview date/time, if an interview has been
scheduled and the office allows showing it. - Office-provided
instructions or notes, if applicable.

The Job Seeker must not see internal employer notes or confidential
office notes.

**Important status distinction:** The Job Seeker-facing statuses are not
necessarily identical to the Employer's internal evaluation statuses.
Maintain a clear mapping between them rather than forcing one status
field to represent two different workflows.

------------------------------------------------------------------------

# 9. Employer Applicant Management

For every advertisement, the Employer should have an applicants list.

The Employer's ability to see CVs and contact details is subject to the
paid-access rule in Section 6.6.

The Employer can review applicants and update the outcome of
communication.

The product owner specified Employer-side outcomes: 1. **تم التواصل
وتحديد ميعاد مقابلة** -- Contacted and interview scheduled. 2. **تم
التواصل وقام بالرفض** -- Contacted and the applicant declined/refused.
3. **لم يرد بعدها** -- No response after contact.

These outcomes must be recorded against the correct application, not
merely against the Job Seeker's general profile.

The Employer should also be able to submit a final evaluation/report for
each applicant: - Accepted / suitable. - Not suitable. - Did not attend
the interview. - Refused the job.

A notes field must be available for additional context.

**\[قرار مطلوب\]** Confirm whether "لم يرد بعدها" is the same as "لم يتم
التواصل" from the Job Seeker's perspective. They should likely be
separate internal states because one means no contact attempt has
occurred, while the other means contact was attempted but no response
was received.

------------------------------------------------------------------------

# 10. Interview and Hiring Follow-up Reports

The platform requires structured follow-up reports at two stages for the
Job Seeker.

## 10.1 First Report -- After the Personal Interview

After the interview, a report about the Job Seeker must be recorded.

The product owner confirmed the existence of this report, but did not
specify its exact fields.

Suggested fields to confirm: - Related Job Seeker. - Related Employer. -
Related Job Advertisement. - Related Application. - Interview date. -
Interview attendance (attended / did not attend). - Interview result. -
Employer evaluation. - Skills/experience observations. - Strengths. -
Concerns. - Recommendation. - Notes. - Report creator. - Report creation
date.

## 10.2 Second Report -- One Month After Hiring

If the Job Seeker is hired, a second follow-up report must be created
after one month of employment.

The system should: - Record the hiring/acceptance date. - Calculate the
one-month follow-up due date. - Create a pending follow-up
task/report. - Notify the responsible office staff when it is due, if
notifications are enabled. - Record the follow-up outcome and notes.

Suggested fields to confirm: - Is the Job Seeker still employed? - Is
the employer satisfied? - Is the Job Seeker satisfied? -
Attendance/commitment observations. - Performance observations. - Any
issues or complaints. - Follow-up action. - Notes. - Report creator and
timestamp.

**\[قرار مطلوب\]** Define who completes each report: Employer,
Employment Office, Job Seeker, or more than one party. Also define
whether the first report is mandatory before an application can be
marked hired.

## 10.3 Employer Report for Every CV / Applicant

The Employer must be able to provide a report for each applicant/CV with
one of these outcomes: - Accepted / Suitable. - Not Suitable. - Did Not
Attend the Interview. - Refused the Job.

An additional notes field is required.

The report must be linked to: - Employer. - Advertisement. -
Applicant. - Application. - Interview, if applicable.

The system should retain report history and identify who submitted each
report and when.

------------------------------------------------------------------------

# 11. Complaints and Suggestions

The platform must provide separate features for Complaints and
Suggestions.

## 11.1 Suggestions

A user can submit a suggestion by writing its content.

The system must identify the sender: - Job Seeker; or - Employer.

The Employment Office can view: - Suggestion content. - Sender type. -
Sender identity. - Submission date/time. - Related office/tenant. -
Processing status, if status management is implemented.

Suggested statuses: - New. - Under Review. - Responded. - Closed.

**\[قرار مطلوب\]** Confirm whether users can submit anonymous
suggestions. Current stated requirement implies the office can see who
submitted the suggestion.

## 11.2 Complaints

A user can submit a complaint by writing its content.

The complaint should record: - Complaint text. - Sender identity. -
Sender type (Job Seeker or Employer). - Submission date/time. - Related
office. - Related company, Job Seeker, advertisement, or application, if
applicable. - Complaint status. - Assigned office staff member, if
assignment is supported. - Internal notes. - Resolution and closure
date.

Suggested complaint statuses: - New. - Under Review. - Waiting for
Information. - Resolved. - Rejected. - Closed.

The Employment Office must be able to investigate and decide the
appropriate action.

## 11.3 Three Consecutive Complaints Against a Job Seeker

Confirmed rule: - If a Job Seeker receives **three complaints in a
row**, their CV/profile is suspended. - This suspension must occur with
Employment Office approval.

System requirements: - Count complaints against the specific Job
Seeker. - Distinguish complaints that are merely submitted from
complaints reviewed/confirmed by the office. **\[قرار مطلوب\]** -
Preserve complaint history. - Alert the office when the threshold is
reached. - Require an authorized office decision before suspension. -
Record the decision maker, date, and reason. - Once suspended, prevent
the Job Seeker's CV/profile from being used in new applications or shown
to Employers, according to the final suspension policy. - Do not
permanently delete the Job Seeker's account or documents merely because
of suspension.

The phrase "three complaints in a row" needs a precise definition: -
Does it mean three consecutive complaints regardless of outcome? - Three
confirmed/valid complaints? - Three complaints within a time period? -
Does a rejected/unsubstantiated complaint break the sequence? These
rules must be confirmed before coding.

## 11.4 Three Complaints Against a Company / Employer

Confirmed rule: - If a company receives **three complaints**,
cooperation with that company is stopped. - This action requires
approval from the Employment Office.

System requirements: - Count complaints linked to the specific
company/employer. - Alert the office when the threshold is reached. -
Require an authorized office decision before stopping cooperation. -
Record the decision maker, timestamp, and reason. - Block the company
from creating/publishing new advertisements and/or accessing the
platform according to the confirmed enforcement policy. - Preserve
historical advertisements, applications, payments, and reports. - Do not
delete company records.

**\[قرار مطلوب\]** Confirm whether the three complaints must be
confirmed/valid, whether the count is cumulative or time-limited, and
whether stopping cooperation means temporary suspension or permanent
blocking.

## 11.5 Complaint Decision and Audit Trail

No automatic punitive action should occur solely because a raw complaint
count reaches three unless the product owner explicitly approves
automation.

Recommended behavior: 1. Complaint is submitted. 2. Office reviews it.
3. Office marks it valid, invalid, or pending. 4. System calculates the
applicable complaint threshold. 5. System creates an alert/task for the
office. 6. Authorized office staff decides whether to suspend the Job
Seeker or stop cooperation with the company. 7. System records the
decision and reason.

------------------------------------------------------------------------

# 12. Super Admin and Multi-Tenant Architecture

## 12.1 Multi-Tenant Requirement

The platform must support multiple independent Employment Offices using
the same application.

Each Employment Office is a **Tenant**.

Tenant-owned data includes: - Job Seekers. - Employers/companies. - Job
advertisements. - Applications. - Uploaded documents. - Complaints. -
Suggestions. - Interview reports. - Follow-up reports. - Office staff
and office-level settings.

Every tenant-owned record must be associated with an `office_id` (or
equivalent tenant identifier), either directly or through a securely
enforced parent relationship.

## 12.2 Data Isolation

A user belonging to Office A must never access Office B's private data
unless a specifically authorized platform-level permission allows it.

Isolation must be enforced in: - Backend authorization. - Database
queries. - API endpoints. - File storage access. - Background jobs. -
Reports and exports. - Search and filtering. - Notifications.

Frontend filtering alone is not sufficient.

## 12.3 Super Admin Capabilities

The Super Admin should be able to: - Create a new Employment Office. -
Edit office information. - Activate/deactivate an office. - Create or
invite the first office administrator. - Reset or manage office admin
access according to security policy. - View office status and high-level
usage statistics. - Search/filter offices. - View platform-level audit
events where permitted.

Suggested Employment Office fields: - Office name. - Office code/slug. -
Address. - City/governorate. - Contact phone. - Contact email. - Status
(Active/Inactive). - Creation date. - Office admin(s). - Tenant
configuration.

## 12.4 Tenant Resolution

The application must reliably identify the current tenant for every
authenticated request.

Possible approaches include: - User account is assigned to one office. -
A user may belong to more than one office. - A user chooses an office
during registration or follows an office-specific link.

**\[قرار مطلوب\]** Decide how a Job Seeker or Employer chooses the
relevant Employment Office. Can one Job Seeker or Employer belong to
multiple offices, or must each account belong to exactly one office?

## 12.5 Cross-Office Records

Do not assume records are shared across offices. By default, each
office's records are isolated.

**\[قرار مطلوب\]** Define whether a Job Seeker can be registered with
multiple offices using the same phone/email/identity document, and how
duplicate profiles should be handled.

------------------------------------------------------------------------

# 13. Notifications

Notifications are needed for important workflow events.

Potential notification events: - Job Seeker registration submitted. -
Job Seeker profile approved/rejected. - Job Seeker document needs
correction. - Employer registration submitted. - Employer
approved/rejected. - Advertisement submitted for review. - Advertisement
approved/rejected. - Payment confirmed/failed. - Advertisement
published. - Advertisement nearing expiry. - Advertisement expired. -
New application received. - Application status changed. - Interview
scheduled or changed. - Interview follow-up report due. - One-month
hiring follow-up due. - Complaint submitted. - Complaint status
changed. - Complaint threshold reached. - Suspension or cooperation-stop
decision recorded. - Suggestion submitted.

Notification channels are not yet confirmed: - In-app notifications. -
Email. - SMS. - WhatsApp. - Push notifications.

**\[قرار مطلوب\]** Choose the channels for the first release. The system
should keep notification generation separate from the delivery provider
so channels can be added later.

Each notification should include: - Recipient. - Tenant/office. - Event
type. - Related record. - Message. - Created timestamp. - Read/unread
state. - Delivery status, if an external channel is used.

------------------------------------------------------------------------

# 14. Dashboards and Reporting

## 14.1 Employment Office Dashboard

Suggested metrics: - Total Job Seekers. - Pending Job Seeker reviews. -
Approved Job Seekers. - Rejected/suspended Job Seekers. - Total
Employers. - Pending Employer reviews. - Approved Employers. - Suspended
companies. - Pending job advertisements. - Active advertisements. -
Expired advertisements. - Total applications. - Applications awaiting
contact. - Interviews scheduled. - Hired Job Seekers. - Follow-up
reports due. - New complaints. - Complaints requiring a decision. - New
suggestions. - Advertisement payments/revenue, if payment records are
part of the platform.

## 14.2 Employer Dashboard

Suggested metrics: - Total advertisements. - Draft advertisements. -
Pending office approval. - Awaiting payment. - Active advertisements. -
Expired advertisements. - Total applicants. - Applicants not yet
contacted. - Interviews scheduled. - Applicants accepted/rejected. -
Reports awaiting completion.

## 14.3 Job Seeker Dashboard

Suggested sections: - Profile completion. - Profile verification
status. - Missing/incorrect documents. - Applied jobs. - Application
statuses. - Upcoming interviews. - Notifications. - Complaints and
suggestions history, if visible to the user.

## 14.4 Super Admin Dashboard

Suggested metrics: - Total Employment Offices. - Active/inactive
offices. - Total users by role. - Aggregate platform activity. -
Office-level usage comparison. - Platform-level issues.

Access to personal or sensitive information must not be inferred from
dashboard access. Apply least-privilege permissions.

------------------------------------------------------------------------

# 15. Core Data Model -- Initial Entity Inventory

This is a conceptual entity inventory, not a final database schema.
Claude must inspect the existing codebase and database before proposing
migrations. Do not create duplicate entities if equivalent models
already exist.

## 15.1 Main entities

### EmploymentOffice

Represents a tenant. Suggested fields: - id - name - code/slug -
address - city - phone - email - status - created_at - updated_at

### User

Authentication identity. Suggested fields: - id - name - email and/or
phone - password/authentication reference - account status -
created_at - updated_at

Do not store passwords in plain text.

### UserOfficeMembership

Use if one user can belong to one or multiple offices. Suggested
fields: - id - user_id - office_id - role - membership_status -
created_at

### JobSeekerProfile

Suggested fields: - id - user_id - office_id - personal profile fields -
review_status - approved_by - approved_at - rejection_reason -
suspended_at - suspension_reason - created_at - updated_at

### JobSeekerDocument

Suggested fields: - id - job_seeker_id - office_id - document_type
(recommendation_letter, national_id, cv) - secure_file_reference -
original_file_name - mime_type - file_size - review_status -
reviewed_by - reviewed_at - rejection_reason - uploaded_at

### Employer

Represents the employer/company profile. Suggested fields: - id -
user_id - office_id - legal_name - display_name - business_activity -
address - city - company contact information - verification_status -
approved_by - approved_at - rejection_reason - cooperation_status -
created_at - updated_at

### EmployerDocument

Suggested fields: - id - employer_id - office_id - document_type
(commercial_register, tax_card, owner_national_id) -
secure_file_reference - original_file_name - mime_type - file_size -
review_status - reviewed_by - reviewed_at - rejection_reason -
uploaded_at

### JobAdvertisement

Suggested fields: - id - office_id - employer_id - title - description -
experience_requirement - working_hours - address - category - city -
other confirmed job fields - status - submitted_at - approved_by -
approved_at - published_at - expires_at - closed_at - created_at -
updated_at

### AdvertisementPayment

Suggested fields: - id - office_id - employer_id -
job_advertisement_id - amount - currency - payment_status -
payment_method - transaction_reference - paid_at - recorded_by -
created_at

Payment fields and workflow depend on the confirmed payment
implementation.

### JobApplication

Suggested fields: - id - office_id - job_advertisement_id -
job_seeker_id - application_status - applied_at -
last_status_changed_at - created_at - updated_at

Add a uniqueness constraint for the intended duplicate-application rule
once confirmed.

### ApplicationStatusHistory

Suggested fields: - id - application_id - old_status - new_status -
changed_by - actor_role - notes - changed_at

### ApplicantEvaluationReport

Suggested fields: - id - office_id - application_id - employer_id -
outcome (suitable, not_suitable, did_not_attend, refused_job) - notes -
submitted_by - submitted_at

### Interview

Suggested fields: - id - office_id - application_id - scheduled_at -
location or meeting details - status - created_by - updated_at

### InterviewReport

Suggested fields: - id - office_id - application_id - interview_id -
report fields approved by the product owner - notes - created_by -
created_at

### HiringFollowUpReport

Suggested fields: - id - office_id - application_id - hired_at -
due_at - report fields approved by the product owner - notes - status -
created_by - created_at - completed_at

### Complaint

Suggested fields: - id - office_id - submitted_by_user_id -
sender_type - subject_type - subject_id - complaint_text - status -
validity_decision - assigned_to - resolution - reviewed_by -
reviewed_at - created_at - updated_at

`subject_type` and `subject_id` identify the Job Seeker, Employer,
advertisement, or other subject of the complaint. The allowed subject
types must be finalized.

### Suggestion

Suggested fields: - id - office_id - submitted_by_user_id -
sender_type - content - status - office_notes - created_at - updated_at

### OfficeDecision

Suggested fields: - id - office_id - target_type - target_id -
decision_type - reason - decided_by - decided_at

Use this or an equivalent audit-friendly structure for formal
suspension/cooperation decisions.

### Notification

Suggested fields: - id - office_id - recipient_user_id - event_type -
related_type - related_id - message - read_at - delivery_status -
created_at

### AuditLog

Suggested fields: - id - office_id (nullable only for platform-level
events) - actor_user_id - actor_role - action - entity_type -
entity_id - before_data / after_data where appropriate and safe -
ip/device metadata if legally and technically appropriate - created_at

Avoid copying sensitive document contents or secrets into audit logs.

------------------------------------------------------------------------

# 16. Relationships and Integrity Rules

Expected high-level relationships: - One Employment Office has many Job
Seeker profiles. - One Employment Office has many Employers. - One
Employment Office has many Job Advertisements. - One Employer can create
many Job Advertisements. - One Job Advertisement can have many Job
Applications. - One Job Seeker can submit many Job Applications. - One
Job Application belongs to one Job Seeker and one Job Advertisement. -
One Job Application can have many status-history records. - One Job
Application can have interview and evaluation/follow-up records. - One
Job Seeker has many Job Seeker Documents. - One Employer has many
Employer Documents. - One Employer/Job Seeker can be the subject of
multiple Complaints. - One user can submit multiple Complaints and
Suggestions. - One Employment Office has many staff members and
office-level users.

Integrity requirements: - Do not allow an application to connect a Job
Seeker from one office to an advertisement from another office unless
cross-tenant applications are explicitly designed and approved. -
Validate that the employer owns the advertisement being edited. -
Validate that a report belongs to the application being evaluated. -
Validate that the office user has access to the record's tenant. -
Preserve historical records when users, employers, advertisements, or
offices are deactivated. - Use database constraints and backend
validation where appropriate.

------------------------------------------------------------------------

# 17. Security, Privacy, and Access Control

Security is a core product requirement because the platform stores
identity documents, CVs, company legal documents, and contact
information.

## 17.1 Sensitive Data

Sensitive data includes: - Job Seeker national ID. - Job Seeker CV. -
Recommendation letter. - Job Seeker phone/email/address. - Employer
commercial register. - Employer tax card. - Owner national ID. -
Employer private contact details. - Internal complaint details. -
Interview and hiring reports.

## 17.2 Access Rules

-   Public visitors: active public job information only.
-   Job Seeker: own profile, own documents/status, own applications, own
    notifications, and permitted complaint/suggestion records.
-   Employer: own company, own advertisements, and applicants linked to
    its advertisements, with sensitive applicant data gated by
    payment/access status.
-   Office staff: records belonging to their Employment Office and
    allowed by their assigned permissions.
-   Super Admin: platform-level capabilities defined by explicit policy;
    no implicit unrestricted access to all private documents.

## 17.3 File Storage

-   Store private files in private storage, not a publicly readable
    folder/bucket.
-   Use authenticated, authorized file access.
-   Use short-lived signed URLs if the storage provider requires them.
-   Verify authorization on every download/view request.
-   Validate file type and size on the server.
-   Do not trust filename extensions alone.
-   Consider malware scanning where available.
-   Prevent one tenant from accessing another tenant's files.
-   Do not expose predictable public file paths.

## 17.4 Backend Authorization

All permissions must be checked server-side for every protected
endpoint/action.

Do not rely on: - Hidden frontend buttons. - Frontend route guards
alone. - Client-provided office IDs without validation. - Client-side
payment flags. - Client-side status values without authorization.

## 17.5 Auditability

Record important actions: - Approvals/rejections. - Document reviews. -
Job advertisement changes. - Payment confirmation/refund actions. -
Applicant status changes. - CV/contact detail access where feasible. -
Complaint decisions. - Suspension and reinstatement. - Employer
cooperation stop/restart. - Super Admin tenant changes.

------------------------------------------------------------------------

# 18. Business Rules -- Consolidated

The following are confirmed rules and must be treated as
acceptance-critical:

1.  The platform has three primary business parties: Employment Office,
    Job Seeker, and Employer.
2.  A Super Admin manages the platform and multiple Employment Offices.
3.  Job Seekers can browse/search jobs before login, but applying
    requires authentication.
4.  Job Seekers submit personal information and required documents.
5.  Required Job Seeker documents are: Father of Confession
    recommendation letter, personal ID card, and CV.
6.  Employers submit company information and required documents.
7.  Required Employer documents are: Commercial Register, Tax Card, and
    national ID of one owner listed in the Commercial Register.
8.  The Employment Office reviews and approves/rejects both Job Seekers
    and Employers.
9.  Job advertisements must not display the Employer's phone number.
10. Communication between Job Seeker and Employer takes place through
    the Employment Office only.
11. A job advertisement contains job title, experience, working hours,
    address, category, city, and other approved details.
12. The Employer pays a fee for each advertisement.
13. Applicant CVs and contact details are not exposed to the Employer
    before the required payment/access is confirmed.
14. Each advertisement is displayed for 15 days only.
15. Job Seekers can view the status of every job they applied to.
16. Job Seeker-facing application states include: not contacted,
    contacted/not suitable, and contacted/interview scheduled.
17. Employer-side applicant outcomes include: contacted/interview
    scheduled, contacted/applicant refused, and no response afterward.
18. Employers can submit an evaluation for every applicant: suitable,
    not suitable, did not attend, or refused the job.
19. An additional notes field is available for applicant reports.
20. A first Job Seeker report is required after the personal interview.
21. A second Job Seeker follow-up report is required one month after
    hiring.
22. Users can submit suggestions; the office can identify the sender and
    whether the sender is a Job Seeker or Employer.
23. Users can submit complaints.
24. Three consecutive complaints against a Job Seeker can lead to
    CV/profile suspension, subject to Employment Office approval.
25. Three complaints against a company can lead to stopping cooperation
    with that company, subject to Employment Office approval.
26. The system must support multiple Employment Offices with
    tenant-level data isolation.

------------------------------------------------------------------------

# 19. User Interface / UX Requirements

The product should be easy to use for users with different levels of
technical experience.

General principles: - Clear navigation based on the user's role. -
Separate dashboards for Job Seeker, Employer, Office Staff, and Super
Admin. - Clear status labels and explanations. - Clear validation
messages for missing/invalid information. - Upload progress and file
validation feedback. - Confirmation before sensitive or irreversible
actions. - Search, filters, sorting, and pagination for large lists. -
Responsive layout for mobile, tablet, and desktop. - Accessible form
labels and readable contrast. - Empty states that explain what the user
should do next. - Loading, error, and success states for all major
workflows. - Avoid exposing private information in tables or public
pages.

Suggested primary navigation:

### Public

-   Home.
-   Browse Jobs.
-   Job Details.
-   Login.
-   Register.

### Job Seeker

-   Dashboard.
-   My Profile.
-   My Documents.
-   Find Jobs.
-   My Applications.
-   Notifications.
-   Complaints.
-   Suggestions.

### Employer

-   Dashboard.
-   Company Profile.
-   Company Documents.
-   My Job Advertisements.
-   Create Advertisement.
-   Applicants.
-   Applicant Reports.
-   Payments/Invoices (if implemented).
-   Notifications.
-   Complaints.
-   Suggestions.

### Employment Office

-   Dashboard.
-   Job Seeker Management.
-   Employer Management.
-   Job Advertisement Review.
-   Applications.
-   Interview Reports.
-   Hiring Follow-up Reports.
-   Complaints.
-   Suggestions.
-   Payments/Financial Records (if implemented).
-   Office Staff and Permissions.
-   Reports.

### Super Admin

-   Platform Dashboard.
-   Employment Offices.
-   Office Administrators.
-   Platform Settings.
-   Platform Audit Logs.

These navigation items are a proposed information architecture and may
be adjusted after UX design.

------------------------------------------------------------------------

# 20. Acceptance Criteria / Test Scenarios

The following scenarios must pass before the corresponding features are
considered complete.

## 20.1 Job Seeker

-   A visitor can browse active jobs without seeing employer private
    contact information.
-   A visitor cannot submit an application without logging in.
-   A Job Seeker can enter personal details and upload all three
    required documents.
-   The system prevents submission when required fields/documents are
    missing.
-   The office can view and review submitted documents.
-   The office can approve or reject the profile.
-   The Job Seeker sees the correct decision/status.
-   An unapproved or suspended Job Seeker cannot perform actions
    prohibited by the final eligibility policy.
-   A Job Seeker can view their own applications and their current
    statuses.
-   A Job Seeker cannot access another Job Seeker's private profile or
    documents.

## 20.2 Employer

-   An Employer can register a company profile and upload the three
    required legal documents.
-   The office can approve/reject the Employer.
-   An unapproved/suspended Employer cannot create/publish
    advertisements according to the final policy.
-   An Employer can create an advertisement with required fields.
-   The office can approve/reject the advertisement.
-   The Employer must pay the per-advertisement fee before the gated
    applicant data is exposed.
-   Before payment, applicant CVs and private contact details are
    inaccessible through both UI and API.
-   After valid payment/access, the Employer can access only applicants
    of the relevant advertisement.
-   The advertisement expires after 15 days from the confirmed start
    event.
-   Expired advertisements are absent from active public search.
-   Historical data remains accessible to authorized users.

## 20.3 Application and Reports

-   A Job Seeker can apply to an eligible active advertisement.
-   Duplicate application behavior follows the confirmed rule.
-   Employer updates are recorded against the correct application.
-   Job Seeker sees only the public/approved status mapping.
-   Employer can submit one of the defined applicant outcomes.
-   Employer can record an applicant evaluation and notes.
-   The first interview report can be created and linked to the
    application.
-   Hiring creates or schedules the one-month follow-up task/report.
-   Reports are accessible only to authorized users.

## 20.4 Complaints and Suggestions

-   A Job Seeker can submit a complaint or suggestion.
-   An Employer can submit a complaint or suggestion.
-   The office can identify the sender and sender type.
-   Complaint records can be reviewed and resolved.
-   The system alerts the office when the defined complaint threshold is
    reached.
-   The system does not suspend a Job Seeker or stop cooperation with a
    company without the required office approval.
-   Every decision is auditable.

## 20.5 Multi-Tenant

-   Office A staff cannot access Office B's users, employers, jobs,
    applications, complaints, suggestions, reports, or files.
-   Employer A cannot access Employer B's applicants or advertisements.
-   A user cannot bypass tenant isolation by changing IDs in API
    requests.
-   Super Admin can manage offices according to assigned platform
    permissions.
-   Deactivating an office follows a defined policy and does not
    silently delete historical records.

------------------------------------------------------------------------

# 21. Implementation Guidance for Claude Code

Claude Code must follow this process.

## Phase 0 -- Repository and Existing-System Discovery

Before writing code: 1. Inspect the entire repository structure. 2.
Identify frontend framework, backend framework, database, ORM,
authentication, file storage, and existing UI libraries. 3. Read
package/configuration files and environment variable examples. 4.
Identify existing modules, routes, pages, components, services, API
endpoints, models, migrations, and tests. 5. Identify reusable
components and existing conventions. 6. Identify security-sensitive
areas and current authorization implementation. 7. Identify existing
data structures that overlap with this specification. 8. Produce an
**Existing System Audit** document. 9. Produce a **Requirements Gap
Analysis** mapping current implementation to this specification. 10. Do
not delete or replace existing code before explaining the impact and
receiving approval.

## Phase 1 -- Product and Technical Planning

Produce: - Finalized role/permission matrix. - User journeys. - Workflow
diagrams. - Entity Relationship Diagram (ERD). - Proposed database
schema. - API endpoint inventory. - Frontend route/page inventory. -
File upload/storage design. - Tenant isolation design. - Payment
workflow design. - Notification event design. - Testing strategy. -
Migration and deployment plan.

List all unresolved decisions from Section 24. Do not silently decide
them.

## Phase 2 -- Foundation

Implement only after the plan is reviewed: - Authentication. -
Role-based access control (RBAC). - Tenant resolution and isolation. -
Employment Office management. - Audit logging foundation. - Secure file
storage foundation. - Shared UI structure and design system.

## Phase 3 -- Registration and Verification

-   Job Seeker profile and documents.
-   Employer profile and legal documents.
-   Office review queues.
-   Approval/rejection workflows.
-   Notifications.

## Phase 4 -- Job Advertisements and Payments

-   Advertisement creation/editing.
-   Office moderation.
-   Payment records/integration.
-   Paid applicant-data access control.
-   15-day publishing lifecycle and expiry.

## Phase 5 -- Applications and Reports

-   Job search and application.
-   Applicant lists.
-   Status transitions.
-   Employer evaluation reports.
-   Interview scheduling and reports.
-   One-month hiring follow-up.

## Phase 6 -- Complaints, Suggestions, and Enforcement

-   Complaint/suggestion submission.
-   Office review.
-   Complaint threshold calculation.
-   Office approval workflow for suspension/cooperation stop.
-   Audit history and notifications.

## Phase 7 -- Dashboards, QA, and Release

-   Role-specific dashboards.
-   Operational reports.
-   Security testing.
-   Tenant-isolation tests.
-   End-to-end workflow tests.
-   Responsive and accessibility checks.
-   Deployment readiness.

## Claude Code working rules

-   Treat this document as the product requirements baseline.
-   Do not implement all modules in one uncontrolled change.
-   Work in small, reviewable phases.
-   Before modifying a module, explain the intended changes and affected
    files.
-   Preserve existing working behavior unless a change is explicitly
    required.
-   Do not invent business rules for unresolved decisions.
-   Do not hardcode tenant IDs, payment status, or permissions.
-   Enforce authorization on the backend.
-   Never expose private CVs, identity documents, or phone numbers
    through public endpoints.
-   Use migrations for database changes.
-   Add tests for important business rules, especially tenant isolation,
    payment gating, advertisement expiry, and complaint enforcement.
-   Keep business logic out of UI components where possible.
-   Use clear naming and consistent architecture.
-   Document environment variables and setup steps without committing
    secrets.
-   Do not claim a feature is complete until its acceptance criteria and
    tests pass.

------------------------------------------------------------------------

# 22. Required Workflow Diagrams

Claude should create diagrams for these workflows before implementation:

1.  Job Seeker registration → personal information → document upload →
    office review → approval/rejection.
2.  Employer registration → legal document upload → office review →
    approval/rejection.
3.  Employer creates advertisement → office review → payment →
    publication → 15-day expiry.
4.  Job Seeker browses → logs in → applies → office/employer processing
    → status updates.
5.  Employer paid access → applicant CV/contact access → communication
    outcome → interview.
6.  Interview → first report → hiring decision → one-month follow-up
    report.
7.  Complaint submission → office investigation → threshold calculation
    → office decision → suspension/cooperation stop.
8.  Suggestion submission → office review.
9.  Super Admin creates office → assigns office administrator → office
    operates independently.
10. Tenant authorization flow for every protected request.

Use Mermaid where appropriate.

------------------------------------------------------------------------

# 23. Important Status Design Guidance

Do not use one generic `status` field for unrelated business processes.

Separate status domains: - User account status. - Job Seeker profile
verification status. - Individual document review status. - Employer
verification status. - Employer cooperation status. - Job advertisement
status. - Payment status. - Job application status. - Interview
status. - Report status. - Complaint status. - Suggestion status. -
Employment Office status.

Every status transition should be validated by the backend and recorded
in history when it materially affects business operations.

The user-facing status wording may be Arabic, while internal enum values
should be stable and language-independent.

------------------------------------------------------------------------

# 24. Open Questions -- Decisions Required Before Final Implementation

The product owner must answer these questions. Claude should surface
them during planning and must not silently choose answers.

## A. Offices and account ownership

1.  Does every Job Seeker belong to exactly one Employment Office, or
    can one Job Seeker register with multiple offices?
2.  Does every Employer belong to exactly one office, or can a company
    work with multiple offices?
3.  How does a user select an office: public registration page,
    office-specific URL, invitation, or admin assignment?
4.  Can an office have multiple administrators/staff members with
    different permissions?
5.  Can Super Admin access private documents, or only manage offices and
    view aggregate data?

## B. Authentication and profile

6.  Is login based on phone number, email, or both?
7.  Is OTP required?
8.  What exact personal fields are mandatory for Job Seekers?
9.  What exact company fields are mandatory for Employers?
10. Can users edit profile information after approval?
11. Do profile/document edits trigger a new office review?
12. Can rejected users resubmit? Can suspended users appeal?

## C. Job advertisements

13. What are the exact mandatory job advertisement fields?
14. Is salary required or optional?
15. Does the Employer pay before office review or after approval?
16. Does the 15-day duration begin at publication or payment?
17. Can an advertisement be paused?
18. Can an Employer renew an expired advertisement?
19. Is renewal charged as a new advertisement?
20. Can the office close an advertisement early?
21. Can an Employer edit an active advertisement, and does editing
    require re-approval?

## D. Payment and access

22. What is the fee per advertisement?
23. What currency is used?
24. Which payment methods are supported?
25. Is payment online through a gateway or manually confirmed by office
    staff?
26. Does payment unlock CVs/contact details for all applicants of that
    advertisement?
27. Is applicant data access available for the entire advertisement
    period or a different period?
28. What is the refund/cancellation policy?
29. Does the Employer pay only for the advertisement, or is there an
    additional fee for applicant data access?

## E. Applications and communication

30. Can a Job Seeker apply while profile review is pending?
31. Can a Job Seeker withdraw an application?
32. Can a Job Seeker apply more than once to the same advertisement?
33. Does the office manually coordinate every contact/interview, or can
    the Employer contact the applicant after paid access?
34. What exactly does the Job Seeker-facing status "تم التواصل وير
    مناسبة" mean? Is the intended wording "تم التواصل وغير مناسبة"?
35. Who changes the Job Seeker-facing status: Employer, office, or both?
36. How are Employer-side statuses mapped to Job Seeker-facing statuses?
37. Does "لم يرد بعدها" mean the applicant did not answer after contact
    attempts? How many attempts are allowed before selecting it?

## F. Interviews and reports

38. Who schedules the interview?
39. Who records the interview date/time and location?
40. Who completes the first report after the interview?
41. What are the exact fields in the first report?
42. Who completes the one-month follow-up report?
43. What are the exact fields in the one-month report?
44. Is the one-month follow-up mandatory for every hired applicant?
45. What happens if the Employer does not submit a required report?

## G. Complaints and enforcement

46. Does the threshold count all submitted complaints or only complaints
    confirmed as valid by the office?
47. What exactly does "three complaints in a row" mean for a Job Seeker?
48. Do complaints expire/reset after a period?
49. Does an invalid complaint break the consecutive sequence?
50. What exactly happens when a Job Seeker's CV is suspended: can they
    log in, apply, or only become hidden from Employers?
51. For companies, are three complaints cumulative or within a defined
    period?
52. Does stopping cooperation mean temporary suspension or permanent
    blocking?
53. Can a suspended Job Seeker or blocked company appeal?
54. Who in the office is authorized to approve enforcement decisions?

## H. Notifications, language, and platform

55. Which notification channels are required in the first release?
56. Is the platform Arabic only, or Arabic and English?
57. Is the primary interface RTL?
58. Is the product a responsive web application, mobile app, or both?
59. Are there office-specific branding/settings?
60. Are exports (PDF/Excel) required for reports?
61. What data-retention policy applies to rejected users, expired jobs,
    complaints, and uploaded documents?

------------------------------------------------------------------------

# 25. Definition of Done

A feature is not considered complete merely because its screen exists.

A feature is complete only when: - Its business rules are implemented. -
Its role permissions are enforced server-side. - Tenant isolation is
enforced. - Required validation and error handling exist. - Sensitive
data is protected. - Relevant audit history is recorded. - Loading,
empty, success, and failure states are implemented. - Tests cover its
important acceptance criteria. - The feature works on supported screen
sizes. - Documentation is updated. - No unresolved business decision has
been silently converted into code.

------------------------------------------------------------------------

# 26. Final Instruction to Claude Code

Before writing or changing code, read this entire document.

Then: 1. Inspect the existing project and explain its current
architecture. 2. Create an Existing System Audit and Requirements Gap
Analysis. 3. Identify the unanswered decisions in Section 24. 4. Propose
a final architecture suitable for the actual repository and technology
stack. 5. Present the ERD, role-permission matrix, workflows, API plan,
and implementation phases. 6. Wait for product-owner approval before
making large architectural changes. 7. Implement one approved phase at a
time. 8. Test each phase against the acceptance criteria in this
document.

**Do not assume that this is a simple job board. It is a multi-tenant,
office-mediated employment management system with document verification,
paid job advertisements, protected applicant information, application
lifecycle tracking, structured follow-up, complaints, and administrative
enforcement.**
