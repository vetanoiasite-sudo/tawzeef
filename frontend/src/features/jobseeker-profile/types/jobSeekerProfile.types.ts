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
  /** Only ever set on the response right after uploading a Cv document, when the local
   * text-heuristic parser found something — a suggestion to review/edit, never auto-saved. */
  suggestedJobTitle?: string | null
  suggestedYearsOfExperience?: number | null
}

/** One office's own status for this job seeker — a job seeker may now submit to several
 * (confirmed 2026-11-24). "NotSubmitted" is a pseudo-status: no application row for that office
 * yet. Always null on JobSeekerProfile.officeApplication (self-view has no single office) — the
 * full per-office list is GET /api/jobseekers/me/offices instead (useMyJobSeekerOffices). */
export type JobSeekerOfficeStatus = {
  officeId: string
  officeName: string
  city: string | null
  reviewStatus: JobSeekerReviewStatusCode | 'NotSubmitted'
  rejectionReason: string | null
  suspendedAt: string | null
  suspensionReason: string | null
  employedAt: string | null
  leftJobAt: string | null
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
  jobTitle: string | null
  /** The job seeker's own single preferred category — drives the "new ad in your field"
   * notification, set separately via useUpdateDesiredCategory (never through UpdateMyProfile, so
   * it never reverts an Approved office application). */
  desiredCategoryId: string | null
  /** No raw URL — the photo is private, fetched through the authenticated
   * GET /api/jobseekers/me/photo (see photoApi). */
  hasPhoto: boolean
  officeApplication: JobSeekerOfficeStatus | null
  documents: JobSeekerDocument[]
}

export type UpdateDesiredCategoryPayload = {
  desiredCategoryId?: string
}

export type SubmitJobSeekerOfficesPayload = {
  officeIds: string[]
  allOffices: boolean
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
  jobTitle?: string
}

/** The 3 required documents (BUSINESS_REQUIREMENTS.md §5.1 Step 4) — fixed, not server-driven. */
export const REQUIRED_DOCUMENT_TYPES: JobSeekerDocumentTypeCode[] = [
  'RecommendationLetter',
  'NationalId',
  'Cv',
]
