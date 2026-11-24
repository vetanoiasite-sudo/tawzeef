import type { CourseSearchFilter } from '../types/courses.types'

export const publicCoursesKeys = {
  all: ['publicCourses'] as const,
  search: (filter: CourseSearchFilter) => [...publicCoursesKeys.all, 'search', filter] as const,
  detail: (id: string) => [...publicCoursesKeys.all, 'detail', id] as const,
}
