import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { officeCoursesApi } from './officeCourses.api'
import { officeCoursesKeys } from './officeCourses.keys'
import type { CourseFormValues, CourseQueryFilter } from '../types/officeCourses.types'

export function useOfficeCourses(filter: CourseQueryFilter) {
  return useQuery({
    queryKey: officeCoursesKeys.list(filter),
    queryFn: () => officeCoursesApi.list(filter),
  })
}

export function useOfficeCourse(id: string) {
  return useQuery({
    queryKey: officeCoursesKeys.detail(id),
    queryFn: () => officeCoursesApi.getById(id),
    enabled: id.length > 0,
  })
}

export function useCreateCourse() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: CourseFormValues) => officeCoursesApi.create(values),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: officeCoursesKeys.all }),
  })
}

export function useUpdateCourse(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (values: CourseFormValues) => officeCoursesApi.update(id, values),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: officeCoursesKeys.all }),
  })
}

export function useUploadCourseImage(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (file: File) => officeCoursesApi.uploadImage(id, file),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: officeCoursesKeys.all }),
  })
}

export function useToggleCoursePause() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, pause }: { id: string; pause: boolean }) =>
      pause ? officeCoursesApi.pause(id) : officeCoursesApi.resume(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: officeCoursesKeys.all }),
  })
}
