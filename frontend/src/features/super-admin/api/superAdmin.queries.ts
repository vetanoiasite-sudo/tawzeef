import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { superAdminApi } from './superAdmin.api'
import { superAdminKeys } from './superAdmin.keys'

export function useOffices() {
  return useQuery({
    queryKey: superAdminKeys.officeList(),
    queryFn: superAdminApi.listOffices,
  })
}

export function useOfficePlatformStats() {
  return useQuery({
    queryKey: superAdminKeys.stats(),
    queryFn: superAdminApi.getStats,
  })
}

export function useCreateOffice() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: superAdminApi.createOffice,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: superAdminKeys.officeList() })
      queryClient.invalidateQueries({ queryKey: superAdminKeys.stats() })
    },
  })
}

export function useUpdateOffice() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: superAdminApi.updateOffice,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: superAdminKeys.officeList() }),
  })
}

export function useActivateOffice() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: superAdminApi.activateOffice,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: superAdminKeys.officeList() })
      queryClient.invalidateQueries({ queryKey: superAdminKeys.stats() })
    },
  })
}

export function useDeactivateOffice() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: superAdminApi.deactivateOffice,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: superAdminKeys.officeList() })
      queryClient.invalidateQueries({ queryKey: superAdminKeys.stats() })
    },
  })
}
