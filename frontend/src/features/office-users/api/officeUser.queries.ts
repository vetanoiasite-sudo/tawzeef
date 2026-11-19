import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeUserApi } from './officeUser.api'
import { officeUserKeys } from './officeUser.keys'

export function useOfficeUsers() {
  return useQuery({
    queryKey: officeUserKeys.list(),
    queryFn: officeUserApi.list,
  })
}

export function useInviteOfficeUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: officeUserApi.invite,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeUserKeys.list() }),
  })
}

export function useUpdateOfficeUserPermissions() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: officeUserApi.updatePermissions,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeUserKeys.list() }),
  })
}

export function useDeactivateOfficeUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: officeUserApi.deactivate,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: officeUserKeys.list() }),
  })
}
