import type { CourseQueryFilter } from '../types/officeCourses.types'

export const officeCoursesKeys = {
  all: ['officeCourses'] as const,
  list: (filter: CourseQueryFilter) => [...officeCoursesKeys.all, 'list', filter] as const,
  detail: (id: string) => [...officeCoursesKeys.all, 'detail', id] as const,
}
