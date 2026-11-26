import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeJobAdPackagesApi } from './officeJobAdPackages.api'
import { officeJobAdPackagesKeys } from './officeJobAdPackages.keys'
import type { JobAdvertisementPackagePayload } from '../types/officeJobAdPackages.types'

export function useOfficeJobAdPackages() {
  return useQuery({
    queryKey: officeJobAdPackagesKeys.list(),
    queryFn: officeJobAdPackagesApi.list,
  })
}

export function useUpdateJobAdPackage() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: JobAdvertisementPackagePayload }) =>
      officeJobAdPackagesApi.update({ id, payload }),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: officeJobAdPackagesKeys.all }),
  })
}

export function useToggleJobAdPackageActive() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, activate }: { id: string; activate: boolean }) =>
      activate ? officeJobAdPackagesApi.activate(id) : officeJobAdPackagesApi.deactivate(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: officeJobAdPackagesKeys.all }),
  })
}
