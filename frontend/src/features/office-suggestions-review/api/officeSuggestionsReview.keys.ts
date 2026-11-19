import type { SuggestionQueueFilter } from '../types/officeSuggestionsReview.types'

export const officeSuggestionsReviewKeys = {
  all: ['officeSuggestionsReview'] as const,
  queue: (filter: SuggestionQueueFilter) => [...officeSuggestionsReviewKeys.all, 'queue', filter] as const,
  detail: (id: string) => [...officeSuggestionsReviewKeys.all, 'detail', id] as const,
}
