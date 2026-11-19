import type { StatusTone } from '@/app/styles/theme'

/**
 * UI_UX_GUIDELINES.md §5 — the single source of truth for every status color in the app. A status
 * value never gets a color by hand in a page; it looks itself up here through <StatusTag>.
 */
export type StatusDomain =
  | 'jobSeekerReview'
  | 'employerVerification'
  | 'employerCooperation'
  | 'advertisement'
  | 'advertisementPayment'
  | 'applicationEmployerFacing'
  | 'applicationJobSeekerFacing'
  | 'complaint'
  | 'suggestion'
  | 'documentReview'
  | 'interview'
  | 'hiringFollowUp'
  | 'applicantEvaluation'
  | 'officeMembership'

const TABLES: Record<StatusDomain, Record<string, StatusTone>> = {
  jobSeekerReview: {
    Draft: 'neutral',
    Submitted: 'warning',
    UnderReview: 'warning',
    Approved: 'success',
    Rejected: 'danger',
    Suspended: 'danger',
  },
  employerVerification: {
    Draft: 'neutral',
    Submitted: 'warning',
    UnderReview: 'warning',
    Approved: 'success',
    Rejected: 'danger',
    Suspended: 'danger',
  },
  employerCooperation: {
    Active: 'success',
    Stopped: 'danger',
  },
  advertisement: {
    Draft: 'neutral',
    Cancelled: 'neutral',
    SubmittedForReview: 'warning',
    UnderReview: 'warning',
    ApprovedAwaitingPayment: 'warning',
    PaymentPending: 'warning',
    Published: 'success',
    Paid: 'success',
    Rejected: 'danger',
    Expired: 'neutral',
    Closed: 'neutral',
    Paused: 'warning',
  },
  advertisementPayment: {
    Pending: 'warning',
    Paid: 'success',
    Failed: 'danger',
    Refunded: 'neutral',
  },
  applicationEmployerFacing: {
    NotYetContacted: 'neutral',
    ContactedInterviewScheduled: 'info',
    ContactedRefused: 'warning',
    NoResponseAfterContact: 'warning',
  },
  applicationJobSeekerFacing: {
    NotContacted: 'neutral',
    ContactedNotSuitable: 'warning',
    ContactedInterviewScheduled: 'info',
  },
  complaint: {
    New: 'info',
    UnderReview: 'warning',
    WaitingForInformation: 'warning',
    Resolved: 'success',
    Rejected: 'neutral',
    Closed: 'neutral',
  },
  suggestion: {
    New: 'info',
    UnderReview: 'warning',
    Responded: 'success',
    Closed: 'neutral',
  },
  documentReview: {
    Pending: 'warning',
    Approved: 'success',
    Rejected: 'danger',
  },
  // Not in UI_UX_GUIDELINES.md §5 (predates Interviews/Reports) — same warning/success/danger/
  // neutral spirit as every other domain above.
  interview: {
    Scheduled: 'info',
    Completed: 'success',
    Cancelled: 'neutral',
    NoShow: 'danger',
  },
  hiringFollowUp: {
    PendingDue: 'warning',
    Completed: 'success',
    Overdue: 'danger',
  },
  applicantEvaluation: {
    Suitable: 'success',
    NotSuitable: 'danger',
    DidNotAttend: 'neutral',
    RefusedJob: 'warning',
  },
  officeMembership: {
    Invited: 'warning',
    Active: 'success',
    Revoked: 'danger',
  },
}

export function resolveStatusTone(domain: StatusDomain, value: string): StatusTone {
  return TABLES[domain][value] ?? 'neutral'
}
