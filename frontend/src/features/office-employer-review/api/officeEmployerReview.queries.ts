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
    // Approving a document can also auto-advance the employer's overall verification status
    // (EmployerReviewService.ApproveDocumentAsync), so the queue list must be invalidated too, not
    // just this detail view — otherwise the queue keeps showing the old status until a manual
    // refetch/navigation.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeEmployerReviewKeys.all }),
  })
}

export function useRejectDocument(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, reason }: { documentId: string; reason: string }) =>
      officeEmployerReviewApi.rejectDocument({ employerId, documentId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeEmployerReviewKeys.all }),
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

export function useRejectProfile(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeEmployerReviewApi.rejectProfile({ employerId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeEmployerReviewKeys.all }),
  })
}

export function useStopCooperation(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeEmployerReviewApi.stopCooperation({ employerId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeEmployerReviewKeys.all }),
  })
}

export function useResumeCooperation(employerId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) =>
      officeEmployerReviewApi.resumeCooperation({ employerId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeEmployerReviewKeys.all }),
  })
}
