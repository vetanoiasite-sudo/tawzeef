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

export type AdvertisementPayment = {
  paymentStatus: 'Pending' | 'Paid' | 'Failed' | 'Refunded'
  amount: number
  currency: string
  paymentMethod: string
  paidAt: string | null
}

export type OfficeJobAdListItem = {
  id: string
  title: string
  city: string | null
  /** The employer's own free-text suggestion — never authoritative. */
  category: string | null
  /** The office-assigned real category's name, once set (JobAdvertisement.CategoryId's backend
   * doc comment) — null until assigned. */
  categoryName: string | null
  status: JobAdvertisementStatusCode
  vacancyCount: number
  submittedAt: string | null
  publishedAt: string | null
  expiresAt: string | null
  employerDisplayName: string | null
}

export type OfficeJobAdDetail = {
  id: string
  title: string
  description: string | null
  experienceRequirement: string | null
  workingHours: string | null
  address: string | null
  /** The employer's own free-text suggestion — never authoritative. */
  category: string | null
  /** The office-assigned real category's id/name, once set — null until assigned
   * (JobAdvertisement.CategoryId's backend doc comment). Approval is blocked until this is set. */
  categoryId: string | null
  categoryName: string | null
  city: string | null
  employmentType: string | null
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
  employerDisplayName: string | null
  payment: AdvertisementPayment | null
}

/** Mirrors backend's UpdateJobAdvertisementDto (same shape the employer's own edit form posts) —
 * office-side correction of a typo the employer got wrong, no re-approval triggered. */
export type UpdateJobAdPayload = {
  title: string
  description?: string
  experienceRequirement?: string
  workingHours?: string
  address?: string
  category?: string
  city?: string
  employmentType?: string
  salaryMin?: number
  salaryMax?: number
  vacancyCount: number
}

export type ReviewQueueFilter = {
  status?: JobAdvertisementStatusCode
  pageIndex: number
  pageSize: number
}

export type OfficeApplicantListItem = {
  applicationId: string
  jobSeekerName: string
  appliedAt: string
  employerFacingStatus: string
  withdrawnAt: string | null
  phone?: string | null
  email?: string | null
}

export type ApplicantsFilter = {
  pageIndex: number
  pageSize: number
}
