/** Mirrors backend/src/Tawzeef.Application/Features/JobSeekers/JobSeekerReviewDtos.cs. */

export type JobSeekerReviewStatusCode =
  'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Suspended'

export type DocumentReviewStatusCode = 'Pending' | 'Approved' | 'Rejected'

export type JobSeekerDocumentTypeCode = 'RecommendationLetter' | 'NationalId' | 'Cv'

export type OfficeJobSeekerListItem = {
  id: string
  fullName: string
  mobileNumber: string
  city: string | null
  reviewStatus: JobSeekerReviewStatusCode
  lastUpdatedAt: string
  validComplaintCount: number
}

export type OfficeJobSeekerDocument = {
  id: string
  documentType: JobSeekerDocumentTypeCode
  originalFileName: string
  reviewStatus: DocumentReviewStatusCode
  rejectionReason: string | null
  uploadedAt: string
}

export type OfficeJobSeekerDetail = {
  id: string
  fullName: string
  nationalId: string
  mobileNumber: string
  email: string | null
  address: string | null
  city: string | null
  dateOfBirth: string | null
  education: string | null
  yearsOfExperience: number | null
  skills: string | null
  careerSummary: string | null
  hasPhoto: boolean
  reviewStatus: JobSeekerReviewStatusCode
  rejectionReason: string | null
  suspensionReason: string | null
  suspendedAt: string | null
  validComplaintCount: number
  documents: OfficeJobSeekerDocument[]
}

export type ReviewQueueFilter = {
  reviewStatus?: JobSeekerReviewStatusCode
  pageIndex: number
  pageSize: number
}
