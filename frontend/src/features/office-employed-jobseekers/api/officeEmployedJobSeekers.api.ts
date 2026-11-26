import { httpClient } from '@/shared/api/httpClient'
import type {
  EmployedJobSeekerListItem,
  EmployedJobSeekersQuery,
} from '../types/officeEmployedJobSeekers.types'

export const officeEmployedJobSeekersApi = {
  list: (query: EmployedJobSeekersQuery) =>
    httpClient.getPaged<EmployedJobSeekerListItem[]>('/api/office/job-seekers/employed', {
      pageIndex: query.pageIndex,
      pageSize: query.pageSize,
    }),
}
