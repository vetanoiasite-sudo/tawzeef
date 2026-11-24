/** Mirrors backend/src/Tawzeef.Application/Features/Employers/EmployerProfileDtos.cs. */

export type EmployerDocumentTypeCode = 'CommercialRegister' | 'TaxCard' | 'OwnerNationalId'

export type DocumentReviewStatusCode = 'Pending' | 'Approved' | 'Rejected'

export type EmployerVerificationStatusCode =
  'Draft' | 'Submitted' | 'UnderReview' | 'Approved' | 'Rejected' | 'Suspended'

export type EmployerCooperationStatusCode = 'Active' | 'Stopped'

export type EmployerDocument = {
  id: string
  documentType: EmployerDocumentTypeCode
  originalFileName: string
  reviewStatus: DocumentReviewStatusCode
  rejectionReason: string | null
  uploadedAt: string
}

/** One office's own status for this employer — an employer may now submit to several (confirmed
 * 2026-11-24). "NotSubmitted" is a pseudo-status: no application row for that office yet. Always
 * null on EmployerProfile.officeApplication (self-view has no single office) — the full per-office
 * list is GET /api/employers/me/offices instead (useMyEmployerOffices). */
export type EmployerOfficeStatus = {
  officeId: string
  officeName: string
  city: string | null
  verificationStatus: EmployerVerificationStatusCode | 'NotSubmitted'
  rejectionReason: string | null
  cooperationStatus: EmployerCooperationStatusCode
  cooperationStoppedReason: string | null
  cooperationStoppedAt: string | null
}

export type EmployerProfile = {
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
  logoUrl: string | null
  officeApplication: EmployerOfficeStatus | null
  documents: EmployerDocument[]
}

export type SubmitEmployerOfficesPayload = {
  officeIds: string[]
  allOffices: boolean
}

export type UpdateEmployerProfilePayload = {
  businessActivity?: string
  address?: string
  city?: string
  companyEmail?: string
  companyPhone?: string
  representativeName?: string
  representativeRole?: string
}

/** The 3 required legal documents (WORKFLOWS_AND_STATE_MACHINES.md §3) — fixed, not server-driven. */
export const REQUIRED_DOCUMENT_TYPES: EmployerDocumentTypeCode[] = [
  'CommercialRegister',
  'TaxCard',
  'OwnerNationalId',
]
