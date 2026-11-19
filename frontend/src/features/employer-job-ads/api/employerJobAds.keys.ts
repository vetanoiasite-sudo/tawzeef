import type { ApplicantsFilter, JobAdQueryFilter } from '../types/employerJobAds.types'

export const employerJobAdsKeys = {
  all: ['employerJobAds'] as const,
  list: (filter: JobAdQueryFilter) => [...employerJobAdsKeys.all, 'list', filter] as const,
  detail: (id: string) => [...employerJobAdsKeys.all, 'detail', id] as const,
  applicants: (adId: string, filter: ApplicantsFilter) => [...employerJobAdsKeys.all, 'applicants', adId, filter] as const,
}
