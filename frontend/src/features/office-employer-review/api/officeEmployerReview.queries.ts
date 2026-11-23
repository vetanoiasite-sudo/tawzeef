import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeEmployerReviewApi } from './officeEmployerReview.api'
import { officeEmployerReviewKeys } from './officeEmployerReview.keys'
import type { ReviewQueueFilter, UpdateEmployerPayload } from '../types/officeEmployerReview.types'

export function useReviewQueue(filter: ReviewQueueFilter) {
  return useQuery({
    queryKey: officeEmployerReviewKeys.queue(filter),
    queryFn: () => officeEmployerReviewApi.listQueue(filter),
  })
}

export function useOfficeEmployerDetail(id: string) {
  return useQuery({
    queryKey: officeEmployerReviewKeys.detail(id),
    queryFn: () => officeEmployerReviewApi.getById(id),
  })
}

export function useApproveDocument(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (documentId: string) =>
      officeEmployerReviewApi.approveDocument({ employerId, documentId }),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: officeEmployerReviewKeys.detail(employerId) }),
  })
}

export function useRejectDocument(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, reason }: { documentId: string; reason: string }) =>
      officeEmployerReviewApi.rejectDocument({ employerId, documentId, reason }),
    onSuccess: () =>
      queryClient.invalidateQueries({ queryKey: officeEmployerReviewKeys.detail(employerId) }),
  })
}

export function useUpdateEmployerProfile(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: UpdateEmployerPayload) =>
      officeEmployerReviewApi.update({ employerId, payload }),
    onSuccess: (data) =>
      queryClient.setQueryData(officeEmployerReviewKeys.detail(employerId), data),
  })
}

export function useApproveProfile(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => officeEmployerReviewApi.approveProfile(employerId),
    onSuccess: (data) =>
      queryClient.setQueryData(officeEmployerReviewKeys.detail(employerId), data),
  })
}

export function useRejectProfile(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeEmployerReviewApi.rejectProfile({ employerId, reason }),
    onSuccess: (data) =>
      queryClient.setQueryData(officeEmployerReviewKeys.detail(employerId), data),
  })
}

export function useStopCooperation(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeEmployerReviewApi.stopCooperation({ employerId, reason }),
    onSuccess: (data) =>
      queryClient.setQueryData(officeEmployerReviewKeys.detail(employerId), data),
  })
}

export function useResumeCooperation(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) =>
      officeEmployerReviewApi.resumeCooperation({ employerId, reason }),
    onSuccess: (data) =>
      queryClient.setQueryData(officeEmployerReviewKeys.detail(employerId), data),
  })
}
