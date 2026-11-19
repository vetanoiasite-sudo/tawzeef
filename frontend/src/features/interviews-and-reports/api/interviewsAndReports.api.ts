import { httpClient } from '@/shared/api/httpClient'
import type {
  ApplicantEvaluation,
  CompleteHiringFollowUpValues,
  CreateEvaluationValues,
  CreateInterviewReportValues,
  CreateInterviewValues,
  HiringFollowUp,
  HiringFollowUpListItem,
  HiringFollowUpQuery,
  Interview,
  InterviewReport,
  UpdateInterviewValues,
} from '../types/interviewsAndReports.types'

export const interviewsAndReportsApi = {
  getInterview: (applicationId: string) => httpClient.get<Interview>(`/api/applications/${applicationId}/interview`),
  scheduleInterview: (applicationId: string, values: CreateInterviewValues) =>
    httpClient.post<Interview>(`/api/applications/${applicationId}/interview`, {
      scheduledAt: values.scheduledAt || null,
      locationOrMeetingDetails: values.locationOrMeetingDetails || null,
    }),
  updateInterview: (applicationId: string, values: UpdateInterviewValues) =>
    httpClient.put<Interview>(`/api/applications/${applicationId}/interview`, {
      scheduledAt: values.scheduledAt || null,
      locationOrMeetingDetails: values.locationOrMeetingDetails || null,
      status: values.status || null,
    }),

  getInterviewReport: (applicationId: string) => httpClient.get<InterviewReport>(`/api/applications/${applicationId}/interview-report`),
  createInterviewReport: (applicationId: string, values: CreateInterviewReportValues) =>
    httpClient.post<InterviewReport>(`/api/applications/${applicationId}/interview-report`, {
      attendance: values.attendance,
      interviewResult: values.interviewResult || null,
      employerEvaluation: values.employerEvaluation || null,
      strengths: values.strengths || null,
      concerns: values.concerns || null,
      recommendation: values.recommendation || null,
      notes: values.notes || null,
    }),

  getEvaluation: (applicationId: string) => httpClient.get<ApplicantEvaluation>(`/api/applications/${applicationId}/evaluation`),
  createEvaluation: (applicationId: string, values: CreateEvaluationValues) =>
    httpClient.post<ApplicantEvaluation>(`/api/applications/${applicationId}/evaluation`, {
      outcome: values.outcome,
      notes: values.notes || null,
    }),

  getHiringFollowUp: (applicationId: string) => httpClient.get<HiringFollowUp>(`/api/applications/${applicationId}/hiring-follow-up`),
  createHiringFollowUp: (applicationId: string, hiredAt?: string) =>
    httpClient.post<HiringFollowUp>(`/api/applications/${applicationId}/hiring-follow-up`, { hiredAt: hiredAt || null }),
  completeHiringFollowUp: (applicationId: string, values: CompleteHiringFollowUpValues) =>
    httpClient.put<HiringFollowUp>(`/api/applications/${applicationId}/hiring-follow-up`, {
      stillEmployed: values.stillEmployed,
      employerSatisfied: values.employerSatisfied,
      jobSeekerSatisfied: values.jobSeekerSatisfied,
      notes: values.notes || null,
    }),
  listDueFollowUps: (query: HiringFollowUpQuery) =>
    httpClient.getPaged<HiringFollowUpListItem[]>('/api/office/hiring-follow-ups/due', {
      pageIndex: query.pageIndex,
      pageSize: query.pageSize,
    }),
}
