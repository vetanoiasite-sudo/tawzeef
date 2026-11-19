import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeJobSeekerReviewApi } from './officeJobSeekerReview.api'
import { officeJobSeekerReviewKeys } from './officeJobSeekerReview.keys'
import type { ReviewQueueFilter } from '../types/officeJobSeekerReview.types'

export function useReviewQueue(filter: ReviewQueueFilter) {
  return useQuery({
    queryKey: officeJobSeekerReviewKeys.queue(filter),
    queryFn: () => officeJobSeekerReviewApi.listQueue(filter),
  })
}

export function useOfficeJobSeekerDetail(id: string) {
  return useQuery({
    queryKey: officeJobSeekerReviewKeys.detail(id),
    queryFn: () => officeJobSeekerReviewApi.getById(id),
  })
}

export function useApproveDocument(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (documentId: string) => officeJobSeekerReviewApi.approveDocument({ profileId, documentId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeJobSeekerReviewKeys.detail(profileId) }),
  })
}

export function useRejectDocument(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, reason }: { documentId: string; reason: string }) =>
      officeJobSeekerReviewApi.rejectDocument({ profileId, documentId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeJobSeekerReviewKeys.detail(profileId) }),
  })
}

export function useApproveProfile(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => officeJobSeekerReviewApi.approveProfile(profileId),
    onSuccess: (data) => queryClient.setQueryData(officeJobSeekerReviewKeys.detail(profileId), data),
  })
}

export function useRejectProfile(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeJobSeekerReviewApi.rejectProfile({ profileId, reason }),
    onSuccess: (data) => queryClient.setQueryData(officeJobSeekerReviewKeys.detail(profileId), data),
  })
}

export function useSuspendProfile(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeJobSeekerReviewApi.suspend({ profileId, reason }),
    onSuccess: (data) => queryClient.setQueryData(officeJobSeekerReviewKeys.detail(profileId), data),
  })
}

export function useReinstateProfile(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeJobSeekerReviewApi.reinstate({ profileId, reason }),
    onSuccess: (data) => queryClient.setQueryData(officeJobSeekerReviewKeys.detail(profileId), data),
  })
}
