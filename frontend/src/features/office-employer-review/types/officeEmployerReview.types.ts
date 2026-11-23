/** Mirrors backend/src/Tawzeef.Application/Features/Employers/EmployerReviewDtos.cs. */

export type EmployerVerificationStatusCode =
  'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Suspended'

export type EmployerCooperationStatusCode = 'Active' | 'Stopped'

export type DocumentReviewStatusCode = 'Pending' | 'Approved' | 'Rejected'

export type EmployerDocumentTypeCode = 'CommercialRegister' | 'TaxCard' | 'OwnerNationalId'

export type OfficeEmployerListItem = {
  id: string
  legalName: string
  displayName: string
  city: string | null
  verificationStatus: EmployerVerificationStatusCode
  cooperationStatus: EmployerCooperationStatusCode
  lastUpdatedAt: string
  validComplaintCount: number
}

export type OfficeEmployerDocument = {
  id: string
  documentType: EmployerDocumentTypeCode
  originalFileName: string
  reviewStatus: DocumentReviewStatusCode
  rejectionReason: string | null
  uploadedAt: string
}

export type OfficeEmployerDetail = {
  id: string
  legalName: string
  displayName: string
  businessActivity: string | null
  address: string | null
  city: string | null
  companyEmail: string | null
  companyPhone: string | null
  representativeName: string | null
  representativeRole: string | null
  verificationStatus: EmployerVerificationStatusCode
  rejectionReason: string | null
  cooperationStatus: EmployerCooperationStatusCode
  cooperationStoppedReason: string | null
  cooperationStoppedAt: string | null
  validComplaintCount: number
  documents: OfficeEmployerDocument[]
}

/** Mirrors backend's UpdateEmployerProfileDto (same shape the employer's own profile form posts)
 * — office-side correction of a typo, never resets VerificationStatus. */
export type UpdateEmployerPayload = {
  businessActivity?: string
  address?: string
  city?: string
  companyEmail?: string
  companyPhone?: string
  representativeName?: string
  representativeRole?: string
}

export type ReviewQueueFilter = {
  verificationStatus?: EmployerVerificationStatusCode
  pageIndex: number
  pageSize: number
}
