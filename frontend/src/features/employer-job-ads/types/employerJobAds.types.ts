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
