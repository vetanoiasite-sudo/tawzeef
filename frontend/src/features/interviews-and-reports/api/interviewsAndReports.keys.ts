import type { HiringFollowUpQuery } from '../types/interviewsAndReports.types'

export const interviewsAndReportsKeys = {
  all: ['interviewsAndReports'] as const,
  interview: (applicationId: string) =>
    [...interviewsAndReportsKeys.all, 'interview', applicationId] as const,
  interviewReport: (applicationId: string) =>
    [...interviewsAndReportsKeys.all, 'interviewReport', applicationId] as const,
  evaluation: (applicationId: string) =>
    [...interviewsAndReportsKeys.all, 'evaluation', applicationId] as const,
  hiringFollowUp: (applicationId: string) =>
    [...interviewsAndReportsKeys.all, 'hiringFollowUp', applicationId] as const,
  dueFollowUps: (query: HiringFollowUpQuery) =>
    [...interviewsAndReportsKeys.all, 'due', query] as const,
}
