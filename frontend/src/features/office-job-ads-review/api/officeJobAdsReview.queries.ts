import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeJobAdsReviewApi } from './officeJobAdsReview.api'
import { officeJobAdsReviewKeys } from './officeJobAdsReview.keys'
import type {
  ApplicantsFilter,
  ReviewQueueFilter,
  UpdateJobAdPayload,
} from '../types/officeJobAdsReview.types'

export function useReviewQueue(filter: ReviewQueueFilter) {
  return useQuery({
    queryKey: officeJobAdsReviewKeys.queue(filter),
    queryFn: () => officeJobAdsReviewApi.listQueue(filter),
  })
}

export function useOfficeJobAdDetail(id: string) {
  return useQuery({
    queryKey: officeJobAdsReviewKeys.detail(id),
    queryFn: () => officeJobAdsReviewApi.getById(id),
    enabled: id.length > 0,
  })
}

export function useUpdateJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: UpdateJobAdPayload) => officeJobAdsReviewApi.update({ id, payload }),
    onSuccess: (data) => queryClient.setQueryData(officeJobAdsReviewKeys.detail(id), data),
  })
}

export function useAssignCategory(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { existingCategoryId?: string; newCategoryName?: string }) =>
      officeJobAdsReviewApi.assignCategory({ id, ...vars }),
    onSuccess: (data) => queryClient.setQueryData(officeJobAdsReviewKeys.detail(id), data),
  })
}

export function useApproveJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => officeJobAdsReviewApi.approve(id),
    onSuccess: (data) => queryClient.setQueryData(officeJobAdsReviewKeys.detail(id), data),
  })
}

export function useRejectJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeJobAdsReviewApi.reject({ id, reason }),
    onSuccess: (data) => queryClient.setQueryData(officeJobAdsReviewKeys.detail(id), data),
  })
}

export function usePauseJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => officeJobAdsReviewApi.pause(id),
    onSuccess: (data) => queryClient.setQueryData(officeJobAdsReviewKeys.detail(id), data),
  })
}

export function useResumeJobAd(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => officeJobAdsReviewApi.resume(id),
    onSuccess: (data) => queryClient.setQueryData(officeJobAdsReviewKeys.detail(id), data),
  })
}

export function useConfirmPayment(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (vars: { amount: number; currency: string; file: File }) =>
      officeJobAdsReviewApi.confirmPayment({ id, ...vars }),
    onSuccess: (data) => queryClient.setQueryData(officeJobAdsReviewKeys.detail(id), data),
  })
}

export function useOfficeJobAdApplicants(adId: string, filter: ApplicantsFilter) {
  return useQuery({
    queryKey: officeJobAdsReviewKeys.applicants(adId, filter),
    queryFn: () => officeJobAdsReviewApi.listApplicants(adId, filter),
  })
}
