import type { JobSearchFilter } from '../types/publicJobs.types'

export const publicJobsKeys = {
  all: ['publicJobs'] as const,
  search: (filter: JobSearchFilter) => [...publicJobsKeys.all, 'search', filter] as const,
  detail: (id: string) => [...publicJobsKeys.all, 'detail', id] as const,
  stats: () => [...publicJobsKeys.all, 'stats'] as const,
  cities: () => [...publicJobsKeys.all, 'cities'] as const,
}
