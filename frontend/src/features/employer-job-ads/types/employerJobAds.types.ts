/** Mirrors backend/src/Tawzeef.Application/Features/JobAdvertisements/JobAdvertisementDtos.cs
 * and Features/JobApplications/JobApplicationDtos.cs. */

export type JobAdvertisementStatusCode =
  | 'Draft'
  | 'SubmittedForReview'
  | 'UnderReview'
  | 'Rejected'
  | 'ApprovedAwaitingPayment'
  | 'PaymentPending'
  | 'Paid'
  | 'Published'
  | 'Paused'
  | 'Closed'
  | 'Expired'
  | 'Cancelled'

export type EmploymentTypeCode = 'FullTime' | 'PartTime' | 'Contract'

export type AdvertisementPayment = {
  paymentStatus: 'Pending' | 'Paid' | 'Failed' | 'Refunded'
  amount: number
  currency: string
  paymentMethod: string
  paidAt: string | null
}

export type JobAdListItem = {
  id: string
  title: string
  city: string | null
  category: string | null
  status: JobAdvertisementStatusCode
  vacancyCount: number
  submittedAt: string | null
  publishedAt: string | null
  expiresAt: string | null
  employerDisplayName: string | null
  /** Which office this ad posts through — shown now that an employer may post through several
   * (confirmed 2026-11-24). */
  officeId: string
  officeName: string | null
}

export type JobAdDetail = {
  id: string
  title: string
  description: string | null
  experienceRequirement: string | null
  workingHours: string | null
  address: string | null
  category: string | null
  city: string | null
  employmentType: EmploymentTypeCode | null
  salaryMin: number | null
  salaryMax: number | null
  vacancyCount: number
  status: JobAdvertisementStatusCode
  submittedAt: string | null
  approvedAt: string | null
  rejectionReason: string | null
  publishedAt: string | null
  expiresAt: string | null
  closedAt: string | null
  renewedFromAdvertisementId: string | null
  employerDisplayName: string | null
  officeId: string
  officeName: string | null
  payment: AdvertisementPayment | null

  /** Snapshotted from the package the office chose at payment time — null means either not yet
   * paid, or paid against a package with no CV cap (unlimited). */
  cvViewLimit: number | null
  cvViewsUsed: number
}

export type JobAdQueryFilter = {
  status?: JobAdvertisementStatusCode
  pageIndex: number
  pageSize: number
}

export type EmployerFacingStatusCode =
  'NotYetContacted' | 'ContactedInterviewScheduled' | 'ContactedRefused' | 'NoResponseAfterContact'

export type ApplicantListItem = {
  applicationId: string
  jobSeekerName: string
  appliedAt: string
  employerFacingStatus: EmployerFacingStatusCode
  withdrawnAt: string | null
  phone?: string | null
  email?: string | null
  cvDownloadUrl?: string | null
}

export type ApplicantsFilter = {
  pageIndex: number
  pageSize: number
}

/** The "open this applicant's profile" page — job title/skills/career summary/education/years
 * of experience/photo always present; phone/email/cvDownloadUrl follow the same payment gate as
 * ApplicantListItem (undefined, not null, until it passes). Deliberately never includes a
 * verification document (recommendation letter, national ID) — office-only. */
export type ApplicantProfile = {
  applicationId: string
  jobSeekerName: string
  jobTitle: string | null
  skills: string | null
  careerSummary: string | null
  education: string | null
  yearsOfExperience: number | null
  hasPhoto: boolean
  employerFacingStatus: EmployerFacingStatusCode
  appliedAt: string
  phone?: string | null
  email?: string | null
  cvDownloadUrl?: string | null
}
