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
    onSuccess: (data) => queryClient.setQueryData(jobSeekerProfileKeys.myProfile(), data),
  })
}

export function useUploadJobSeekerDocument() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.uploadDocument,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: jobSeekerProfileKeys.myProfile() }),
  })
}

export function useSubmitJobSeekerProfileForReview() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.submitForReview,
    onSuccess: (data) => queryClient.setQueryData(jobSeekerProfileKeys.myProfile(), data),
  })
}

export function useUploadJobSeekerPhoto() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerProfileApi.uploadPhoto,
    onSuccess: (data) => queryClient.setQueryData(jobSeekerProfileKeys.myProfile(), data),
  })
}
