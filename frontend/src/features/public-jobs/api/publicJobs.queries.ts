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

export function usePublicPlatformStats() {
  return useQuery({
    queryKey: publicJobsKeys.stats(),
    queryFn: publicJobsApi.getStats,
    staleTime: 60_000,
  })
}

export function usePublicCities() {
  return useQuery({
    queryKey: publicJobsKeys.cities(),
    queryFn: publicJobsApi.getCities,
    staleTime: 60_000,
  })
}
