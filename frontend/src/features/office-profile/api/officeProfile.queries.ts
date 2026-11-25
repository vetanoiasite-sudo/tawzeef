import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeProfileApi } from './officeProfile.api'
import { officeProfileKeys } from './officeProfile.keys'

export function useMyOffice() {
  return useQuery({
    queryKey: officeProfileKeys.myOffice(),
    queryFn: officeProfileApi.getMyOffice,
  })
}

export function useUpdateMyOffice() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: officeProfileApi.updateMyOffice,
    onSuccess: (data) => queryClient.setQueryData(officeProfileKeys.myOffice(), data),
  })
}

export function useUploadMyOfficeLogo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: officeProfileApi.uploadMyLogo,
    onSuccess: (data) => queryClient.setQueryData(officeProfileKeys.myOffice(), data),
  })
}

export function usePublicOffice(id: string) {
  return useQuery({
    queryKey: officeProfileKeys.publicOffice(id),
    queryFn: () => officeProfileApi.getPublicOffice(id),
    enabled: id.length > 0,
  })
}
