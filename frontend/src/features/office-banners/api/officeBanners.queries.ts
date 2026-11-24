import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeBannersApi } from './officeBanners.api'
import { officeBannersKeys } from './officeBanners.keys'

export function useOfficeBanners() {
  return useQuery({
    queryKey: officeBannersKeys.list(),
    queryFn: officeBannersApi.list,
  })
}

export function useCreateBanner() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (file: File) => officeBannersApi.create(file),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: officeBannersKeys.all }),
  })
}

export function useToggleBannerPause() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, pause }: { id: string; pause: boolean }) =>
      pause ? officeBannersApi.pause(id) : officeBannersApi.resume(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: officeBannersKeys.all }),
  })
}
