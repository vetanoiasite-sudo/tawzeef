import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { suggestionsApi } from './suggestions.api'
import { suggestionsKeys } from './suggestions.keys'
import type { CreateSuggestionPayload } from '../types/suggestions.types'

export function useMySuggestions(pageIndex: number, pageSize: number) {
  return useQuery({
    queryKey: suggestionsKeys.mine(pageIndex, pageSize),
    queryFn: () => suggestionsApi.listMine(pageIndex, pageSize),
  })
}

export function useCreateSuggestion() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateSuggestionPayload) => suggestionsApi.create(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: suggestionsKeys.all }),
  })
}
