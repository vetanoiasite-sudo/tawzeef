import { httpClient } from '@/shared/api/httpClient'
import type { Suggestion, SuggestionQueueFilter } from '../types/officeSuggestionsReview.types'

export const officeSuggestionsReviewApi = {
  listQueue: (filter: SuggestionQueueFilter) =>
    httpClient.getPaged<Suggestion[]>('/api/office/suggestions', {
      status: filter.status,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<Suggestion>(`/api/office/suggestions/${id}`),
  respond: ({ id, officeNotes }: { id: string; officeNotes: string }) =>
    httpClient.post<Suggestion>(`/api/office/suggestions/${id}/respond`, { officeNotes }),
}
