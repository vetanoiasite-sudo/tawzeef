import { useQuery } from '@tanstack/react-query'
import { governoratesApi } from './governorates.api'
import { governoratesKeys } from './governorates.keys'

export function useGovernorates() {
  return useQuery({
    queryKey: governoratesKeys.list(),
    queryFn: governoratesApi.list,
    staleTime: 60_000,
  })
}
