import { useQuery } from '@tanstack/react-query'
import { publicCoursesApi } from './courses.api'
import { publicCoursesKeys } from './courses.keys'
import type { CourseSearchFilter } from '../types/courses.types'

export function useCourseSearch(filter: CourseSearchFilter) {
  return useQuery({
    queryKey: publicCoursesKeys.search(filter),
    queryFn: () => publicCoursesApi.search(filter),
  })
}

export function useCourseDetails(id: string) {
  return useQuery({
    queryKey: publicCoursesKeys.detail(id),
    queryFn: () => publicCoursesApi.getById(id),
    enabled: id.length > 0,
  })
}
