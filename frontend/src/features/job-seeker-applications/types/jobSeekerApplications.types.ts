/** Mirrors backend/src/Tawzeef.Application/Features/JobApplications/JobApplicationDtos.cs. */

export type JobSeekerFacingStatusCode =
  'NotContacted' | 'ContactedNotSuitable' | 'ContactedInterviewScheduled'

export type MyApplicationListItem = {
  id: string
  jobAdvertisementId: string
  jobTitle: string
  employerDisplayName: string
  appliedAt: string
  jobSeekerFacingStatus: JobSeekerFacingStatusCode
  withdrawnAt: string | null
}

export type MyApplicationsFilter = {
  pageIndex: number
  pageSize: number
}
