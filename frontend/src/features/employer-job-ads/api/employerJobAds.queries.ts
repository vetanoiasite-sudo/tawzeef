import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { employerJobAdsApi } from './employerJobAds.api'
import { employerJobAdsKeys } from './employerJobAds.keys'
import type { JobAdFormValues } from '../schemas/jobAd.schema'
import type { ApplicantsFilter, JobAdQueryFilter } from '../types/employerJobAds.types'

export function useMyJobAds(filter: JobAdQueryFilter) {
  return useQuery({
    queryKey: employerJobAdsKeys.list(filter),
    queryFn: () => employerJobAdsApi.listMine(filter),
  })
}

export function useMyJobAdDetail(id: string) {
  return useQuery({
    queryKey: employerJobAdsKeys.detail(id),
    queryFn: () => employerJobAdsApi.getById(id),
    enabled: id.length > 0,
  })
}

export function useCreateJobAd() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: JobAdFormValues) => employerJobAdsApi.create(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: employerJobAdsKeys.all }),
  })
}

export function useUpdateJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: JobAdFormValues) => employerJobAdsApi.update(id, values),
    onSuccess: (data) => queryClient.setQueryData(employerJobAdsKeys.detail(id), data),
  })
}

export function useSubmitJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => employerJobAdsApi.submit(id),
    onSuccess: (data) => queryClient.setQueryData(employerJobAdsKeys.detail(id), data),
  })
}

export function useCloseJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => employerJobAdsApi.close(id),
    onSuccess: (data) => queryClient.setQueryData(employerJobAdsKeys.detail(id), data),
  })
}

export function useRenewJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => employerJobAdsApi.renew(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: employerJobAdsKeys.all }),
  })
}

export function useJobAdApplicants(adId: string, filter: ApplicantsFilter) {
  return useQuery({
    queryKey: employerJobAdsKeys.applicants(adId, filter),
    queryFn: () => employerJobAdsApi.listApplicants(adId, filter),
  })
}

export function useUpdateApplicationOutcome(adId: string, filter: ApplicantsFilter) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { applicationId: string; outcome: string; notes?: string }) =>
      employerJobAdsApi.updateOutcome(vars),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: employerJobAdsKeys.applicants(adId, filter) }),
  })
}
