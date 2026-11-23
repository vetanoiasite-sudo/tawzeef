import { useQuery } from '@tanstack/react-query'
import { jobCategoriesApi } from './jobCategories.api'
import { jobCategoriesKeys } from './jobCategories.keys'

export function useJobCategories() {
  return useQuery({
    queryKey: jobCategoriesKeys.list(),
    queryFn: jobCategoriesApi.list,
    staleTime: 60_000,
  })
}
