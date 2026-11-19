import type { MyApplicationsFilter } from '../types/jobSeekerApplications.types'

export const jobSeekerApplicationsKeys = {
  all: ['jobSeekerApplications'] as const,
  mine: (filter: MyApplicationsFilter) => [...jobSeekerApplicationsKeys.all, 'mine', filter] as const,
}
