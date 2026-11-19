import { httpClient } from '@/shared/api/httpClient'
import type { MyApplicationListItem, MyApplicationsFilter } from '../types/jobSeekerApplications.types'

export const jobSeekerApplicationsApi = {
  apply: (jobAdvertisementId: string) =>
    httpClient.post<{ id: string }>('/api/job-seekers/me/applications', { jobAdvertisementId }),
  listMine: (filter: MyApplicationsFilter) =>
    httpClient.getPaged<MyApplicationListItem[]>('/api/job-seekers/me/applications', {
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  withdraw: (id: string) => httpClient.post<void>(`/api/job-seekers/me/applications/${id}/withdraw`),
}
