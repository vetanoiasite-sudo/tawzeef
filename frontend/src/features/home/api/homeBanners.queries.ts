import { useQuery } from '@tanstack/react-query'
import { homeBannersApi } from './homeBanners.api'

export function useHomeBanners() {
  return useQuery({
    queryKey: ['homeBanners', 'list'],
    queryFn: homeBannersApi.list,
    staleTime: 60_000,
  })
}
