import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeSuggestionsReviewApi } from './officeSuggestionsReview.api'
import { officeSuggestionsReviewKeys } from './officeSuggestionsReview.keys'
import type { SuggestionQueueFilter } from '../types/officeSuggestionsReview.types'

export function useSuggestionQueue(filter: SuggestionQueueFilter) {
  return useQuery({
    queryKey: officeSuggestionsReviewKeys.queue(filter),
    queryFn: () => officeSuggestionsReviewApi.listQueue(filter),
  })
}

export function useOfficeSuggestionDetail(id: string) {
  return useQuery({
    queryKey: officeSuggestionsReviewKeys.detail(id),
    queryFn: () => officeSuggestionsReviewApi.getById(id),
    enabled: id.length > 0,
  })
}

export function useRespondToSuggestion(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (officeNotes: string) => officeSuggestionsReviewApi.respond({ id, officeNotes }),
    onSuccess: (data) => queryClient.setQueryData(officeSuggestionsReviewKeys.detail(id), data),
  })
}
