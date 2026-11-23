import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeComplaintReviewApi } from './officeComplaintReview.api'
import { officeComplaintReviewKeys } from './officeComplaintReview.keys'
import type {
  ComplaintQueueFilter,
  ComplaintValidityDecisionCode,
  OfficeDecisionTypeCode,
} from '../types/officeComplaintReview.types'

export function useComplaintQueue(filter: ComplaintQueueFilter) {
  return useQuery({
    queryKey: officeComplaintReviewKeys.queue(filter),
    queryFn: () => officeComplaintReviewApi.listQueue(filter),
  })
}

export function useOfficeComplaintDetail(id: string) {
  return useQuery({
    queryKey: officeComplaintReviewKeys.detail(id),
    queryFn: () => officeComplaintReviewApi.getById(id),
  })
}

export function useTriageComplaint(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (validityDecision: ComplaintValidityDecisionCode) =>
      officeComplaintReviewApi.triage({ id, validityDecision }),
    onSuccess: (data) => queryClient.setQueryData(officeComplaintReviewKeys.detail(id), data),
  })
}

export function useResolveComplaint(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({
      outcome,
      resolutionNotes,
    }: {
      outcome: 'Resolved' | 'Rejected'
      resolutionNotes: string
    }) => officeComplaintReviewApi.resolve({ id, outcome, resolutionNotes }),
    onSuccess: (data) => queryClient.setQueryData(officeComplaintReviewKeys.detail(id), data),
  })
}

export function useDecideComplaint(id: string) {
  return useMutation({
    mutationFn: ({
      decisionType,
      reason,
    }: {
      decisionType: OfficeDecisionTypeCode
      reason: string
    }) => officeComplaintReviewApi.decide({ id, decisionType, reason }),
  })
}
