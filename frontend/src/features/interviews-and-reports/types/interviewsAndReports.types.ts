/** Mirrors backend/src/Tawzeef.Application/Features/Interviews/*. */

export type InterviewStatusCode = 'Scheduled' | 'Completed' | 'Cancelled' | 'NoShow'
export type AttendanceResultCode = 'Attended' | 'DidNotAttend'
export type FollowUpReportStatusCode = 'PendingDue' | 'Completed' | 'Overdue'
export type ApplicantEvaluationOutcomeCode =
  'Suitable' | 'NotSuitable' | 'DidNotAttend' | 'RefusedJob'

export type Interview = {
  id: string
  scheduledAt: string | null
  locationOrMeetingDetails: string | null
  status: InterviewStatusCode
}

export type CreateInterviewValues = {
  scheduledAt: string
  locationOrMeetingDetails: string
}

export type UpdateInterviewValues = {
  scheduledAt: string
  locationOrMeetingDetails: string
  status: InterviewStatusCode | ''
}

export type InterviewReport = {
  id: string
  attendance: AttendanceResultCode
  interviewResult: string | null
  employerEvaluation: string | null
  strengths: string | null
  concerns: string | null
  recommendation: string | null
  notes: string | null
  creationDate: string
}

export type CreateInterviewReportValues = {
  attendance: AttendanceResultCode
  interviewResult: string
  employerEvaluation: string
  strengths: string
  concerns: string
  recommendation: string
  notes: string
}

export type ApplicantEvaluation = {
  id: string
  outcome: ApplicantEvaluationOutcomeCode
  notes: string | null
  submittedAt: string
}

export type CreateEvaluationValues = {
  outcome: ApplicantEvaluationOutcomeCode | ''
  notes: string
}

export type HiringFollowUp = {
  id: string
  jobApplicationId: string
  hiredAt: string
  dueAt: string
  status: FollowUpReportStatusCode
  stillEmployed: boolean | null
  employerSatisfied: boolean | null
  jobSeekerSatisfied: boolean | null
  notes: string | null
  completedAt: string | null
}

export type CompleteHiringFollowUpValues = {
  stillEmployed: boolean | null
  employerSatisfied: boolean | null
  jobSeekerSatisfied: boolean | null
  notes: string
}

export type HiringFollowUpListItem = {
  id: string
  jobApplicationId: string
  jobSeekerName: string
  employerDisplayName: string
  jobTitle: string
  hiredAt: string
  dueAt: string
  status: FollowUpReportStatusCode
}

export type HiringFollowUpQuery = {
  pageIndex: number
  pageSize: number
}
