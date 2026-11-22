import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { platformSettingsApi } from './platformSettings.api'
import { platformSettingsKeys } from './platformSettings.keys'

export function usePlatformSettings() {
  return useQuery({
    queryKey: platformSettingsKeys.list(),
    queryFn: platformSettingsApi.list,
  })
}

export function useUpdatePlatformSetting() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ key, value }: { key: string; value: string }) => platformSettingsApi.update(key, value),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: platformSettingsKeys.list() })
    },
  })
}
