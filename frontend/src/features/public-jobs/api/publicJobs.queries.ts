import { useQuery } from '@tanstack/react-query'
import { publicJobsApi } from './publicJobs.api'
import { publicJobsKeys } from './publicJobs.keys'
import type { JobSearchFilter } from '../types/publicJobs.types'

export function useJobSearch(filter: JobSearchFilter) {
  return useQuery({
    queryKey: publicJobsKeys.search(filter),
    queryFn: () => publicJobsApi.search(filter),
  })
}

export function useJobDetails(id: string) {
  return useQuery({
    queryKey: publicJobsKeys.detail(id),
    queryFn: () => publicJobsApi.getById(id),
    enabled: id.length > 0,
  })
}
