import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { jobSeekerCourseApi } from './jobSeekerCourse.api'
import { jobSeekerCourseKeys } from './jobSeekerCourse.keys'

export function useMyCourses() {
  return useQuery({
    queryKey: jobSeekerCourseKeys.mine(),
    queryFn: jobSeekerCourseApi.listMine,
  })
}

export function useCreateCourse() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerCourseApi.create,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: jobSeekerCourseKeys.mine() }),
  })
}

export function useUpdateCourse() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerCourseApi.update,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: jobSeekerCourseKeys.mine() }),
  })
}

export function useDeleteCourse() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerCourseApi.remove,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: jobSeekerCourseKeys.mine() }),
  })
}

export function useUploadCourseCertificate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: jobSeekerCourseApi.uploadCertificate,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: jobSeekerCourseKeys.mine() }),
  })
}
