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

/** This office's own application row for the profile being viewed — never null for a profile the
 * office review queue can even reach (the access check itself requires one to exist). */
export type OfficeJobSeekerApplication = {
  officeId: string
  officeName: string
  city: string | null
  reviewStatus: JobSeekerReviewStatusCode
  rejectionReason: string | null
  suspendedAt: string | null
  suspensionReason: string | null
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
  validComplaintCount: number
  officeApplication: OfficeJobSeekerApplication
  documents: OfficeJobSeekerDocument[]
}

export type ReviewQueueFilter = {
  reviewStatus?: JobSeekerReviewStatusCode
  pageIndex: number
  pageSize: number
}
