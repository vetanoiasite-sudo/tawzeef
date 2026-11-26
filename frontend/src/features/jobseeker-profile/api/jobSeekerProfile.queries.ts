import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { jobSeekerProfileApi } from './jobSeekerProfile.api'
import { jobSeekerProfileKeys } from './jobSeekerProfile.keys'

export function useMyJobSeekerProfile() {
  return useQuery({
    queryKey: jobSeekerProfileKeys.myProfile(),
    queryFn: jobSeekerProfileApi.getMyProfile,
  })
}

export function useUpdateMyJobSeekerProfile() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.updateMyProfile,
    onSuccess: (data) => {
      queryClient.setQueryData(jobSeekerProfileKeys.myProfile(), data)
      // An edit reverts any Approved office application back to UnderReview (shared documents).
      void queryClient.invalidateQueries({ queryKey: jobSeekerProfileKeys.myOffices() })
    },
  })
}

export function useUpdateDesiredCategory() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.updateDesiredCategory,
    onSuccess: (data) => queryClient.setQueryData(jobSeekerProfileKeys.myProfile(), data),
  })
}

export function useUploadJobSeekerDocument() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.uploadDocument,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: jobSeekerProfileKeys.myProfile() })
      void queryClient.invalidateQueries({ queryKey: jobSeekerProfileKeys.myOffices() })
    },
  })
}

export function useSubmitJobSeekerProfileForReview() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.submitForReview,
    onSuccess: (data) => {
      queryClient.setQueryData(jobSeekerProfileKeys.myProfile(), data)
      void queryClient.invalidateQueries({ queryKey: jobSeekerProfileKeys.myOffices() })
    },
  })
}

export function useUploadJobSeekerPhoto() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.uploadPhoto,
    onSuccess: (data) => queryClient.setQueryData(jobSeekerProfileKeys.myProfile(), data),
  })
}

export function useMyJobSeekerOffices() {
  return useQuery({
    queryKey: jobSeekerProfileKeys.myOffices(),
    queryFn: jobSeekerProfileApi.getMyOffices,
  })
}

export function useSubmitJobSeekerOffices() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.submitToOffices,
    onSuccess: (data) => queryClient.setQueryData(jobSeekerProfileKeys.myOffices(), data),
  })
}
