import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { employerProfileApi } from './employerProfile.api'
import { employerProfileKeys } from './employerProfile.keys'

export function useMyEmployerProfile() {
  return useQuery({
    queryKey: employerProfileKeys.myProfile(),
    queryFn: employerProfileApi.getMyProfile,
  })
}

export function useUpdateMyEmployerProfile() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: employerProfileApi.updateMyProfile,
    onSuccess: (data) => {
      queryClient.setQueryData(employerProfileKeys.myProfile(), data)
      // An edit reverts any Approved office application back to UnderReview (shared documents).
      void queryClient.invalidateQueries({ queryKey: employerProfileKeys.myOffices() })
    },
  })
}

export function useMyEmployerOffices() {
  return useQuery({
    queryKey: employerProfileKeys.myOffices(),
    queryFn: employerProfileApi.getMyOffices,
  })
}

export function useSubmitEmployerOffices() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: employerProfileApi.submitToOffices,
    onSuccess: (data) => queryClient.setQueryData(employerProfileKeys.myOffices(), data),
  })
}

export function useUploadEmployerDocument() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: employerProfileApi.uploadDocument,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: employerProfileKeys.myProfile() })
      void queryClient.invalidateQueries({ queryKey: employerProfileKeys.myOffices() })
    },
  })
}

export function useUploadEmployerLogo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: employerProfileApi.uploadLogo,
    onSuccess: (data) => queryClient.setQueryData(employerProfileKeys.myProfile(), data),
  })
}

export function useSubmitEmployerProfileForReview() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: employerProfileApi.submitForReview,
    onSuccess: (data) => {
      queryClient.setQueryData(employerProfileKeys.myProfile(), data)
      void queryClient.invalidateQueries({ queryKey: employerProfileKeys.myOffices() })
    },
  })
}
