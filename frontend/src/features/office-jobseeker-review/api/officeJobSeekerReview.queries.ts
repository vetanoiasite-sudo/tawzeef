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
    mutationFn: (documentId: string) =>
      officeJobSeekerReviewApi.approveDocument({ profileId, documentId }),
    // Approving a document can also auto-advance the profile's overall review status
    // (JobSeekerReviewService.ApproveDocumentAsync), so the queue list must be invalidated too,
    // not just this detail view — otherwise the queue keeps showing the old status until a
    // manual refetch/navigation.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeJobSeekerReviewKeys.all }),
  })
}

export function useRejectDocument(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ documentId, reason }: { documentId: string; reason: string }) =>
      officeJobSeekerReviewApi.rejectDocument({ profileId, documentId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeJobSeekerReviewKeys.all }),
  })
}

export function useRejectProfile(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeJobSeekerReviewApi.rejectProfile({ profileId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeJobSeekerReviewKeys.all }),
  })
}

export function useSuspendProfile(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeJobSeekerReviewApi.suspend({ profileId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeJobSeekerReviewKeys.all }),
  })
}

export function useReinstateProfile(profileId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (reason: string) => officeJobSeekerReviewApi.reinstate({ profileId, reason }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeJobSeekerReviewKeys.all }),
  })
}
