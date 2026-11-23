import { httpClient } from '@/shared/api/httpClient'
import type { CreateSuggestionPayload, Suggestion } from '../types/suggestions.types'

export const suggestionsApi = {
  create: (payload: CreateSuggestionPayload) =>
    httpClient.post<Suggestion>('/api/suggestions', payload),
  listMine: (pageIndex: number, pageSize: number) =>
    httpClient.getPaged<Suggestion[]>('/api/suggestions/mine', { pageIndex, pageSize }),
}
