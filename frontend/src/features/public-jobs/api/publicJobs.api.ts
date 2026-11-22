import { httpClient } from '@/shared/api/httpClient'
import type { JobDetails, JobListItem, JobSearchFilter, PublicPlatformStats } from '../types/publicJobs.types'

export const publicJobsApi = {
  search: (filter: JobSearchFilter) =>
    httpClient.getPaged<JobListItem[]>('/api/jobs', {
      keyword: filter.keyword,
      city: filter.city,
      category: filter.category,
      employmentType: filter.employmentType,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<JobDetails>(`/api/jobs/${id}`),
  getStats: () => httpClient.get<PublicPlatformStats>('/api/jobs/stats'),
}
