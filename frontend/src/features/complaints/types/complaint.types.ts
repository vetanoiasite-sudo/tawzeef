/** Mirrors backend/src/Tawzeef.Application/Features/Complaints/ComplaintDtos.cs. */

export type ComplaintSubjectTypeCode = 'JobSeeker' | 'Employer'

export type ComplaintStatusCode =
  'New' | 'UnderReview' | 'WaitingForInformation' | 'Resolved' | 'Rejected' | 'Closed'

export type ComplaintValidityDecisionCode = 'Pending' | 'Valid' | 'Invalid'

export type Complaint = {
  id: string
  senderType: 'JobSeeker' | 'Employer'
  subjectType: ComplaintSubjectTypeCode
  subjectId: string
  subjectName: string
  complaintText: string
  status: ComplaintStatusCode
  validityDecision: ComplaintValidityDecisionCode
  resolutionNotes: string | null
  createdAt: string
}

export type CreateComplaintPayload = {
  subjectType: ComplaintSubjectTypeCode
  subjectId: string
  complaintText: string
}
