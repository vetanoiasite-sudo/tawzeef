import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { jobSeekerApplicationsApi } from './jobSeekerApplications.api'
import { jobSeekerApplicationsKeys } from './jobSeekerApplications.keys'
import type { MyApplicationsFilter } from '../types/jobSeekerApplications.types'

export function useMyApplications(filter: MyApplicationsFilter) {
  return useQuery({
    queryKey: jobSeekerApplicationsKeys.mine(filter),
    queryFn: () => jobSeekerApplicationsApi.listMine(filter),
  })
}

export function useApplyToJob() {
  return useMutation({
    mutationFn: (jobAdvertisementId: string) => jobSeekerApplicationsApi.apply(jobAdvertisementId),
  })
}

export function useWithdrawApplication() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => jobSeekerApplicationsApi.withdraw(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: jobSeekerApplicationsKeys.all }),
  })
}
