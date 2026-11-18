/** Mirrors backend/src/Tawzeef.Application/Features/JobSeekers/JobSeekerProfileDtos.cs. */

export type JobSeekerDocumentTypeCode = 'RecommendationLetter' | 'NationalId' | 'Cv'

export type DocumentReviewStatusCode = 'Pending' | 'Approved' | 'Rejected'

export type JobSeekerReviewStatusCode =
  'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Suspended'

export type JobSeekerDocument = {
  id: string
  documentType: JobSeekerDocumentTypeCode
  originalFileName: string
  reviewStatus: DocumentReviewStatusCode
  rejectionReason: string | null
  uploadedAt: string
}

export type JobSeekerProfile = {
  id: string
  fullName: string
  mobileNumber: string
  email: string | null
  address: string | null
  city: string | null
  dateOfBirth: string | null
  education: string | null
  yearsOfExperience: number | null
  skills: string | null
  careerSummary: string | null
  reviewStatus: JobSeekerReviewStatusCode
  rejectionReason: string | null
  documents: JobSeekerDocument[]
}

export type UpdateJobSeekerProfilePayload = {
  email?: string
  address?: string
  city?: string
  dateOfBirth?: string
  education?: string
  yearsOfExperience?: number
  skills?: string
  careerSummary?: string
}

/** The 3 required documents (BUSINESS_REQUIREMENTS.md §5.1 Step 4) — fixed, not server-driven. */
export const REQUIRED_DOCUMENT_TYPES: JobSeekerDocumentTypeCode[] = [
  'RecommendationLetter',
  'NationalId',
  'Cv',
]
