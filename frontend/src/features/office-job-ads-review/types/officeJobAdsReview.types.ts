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
  category: string | null
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
  category: string | null
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
