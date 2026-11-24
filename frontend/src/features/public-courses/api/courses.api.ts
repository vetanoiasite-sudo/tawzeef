import { httpClient } from '@/shared/api/httpClient'
import type { CourseSearchFilter, PublicCourseDetail, PublicCourseListItem } from '../types/courses.types'

export const publicCoursesApi = {
  search: (filter: CourseSearchFilter) =>
    httpClient.getPaged<PublicCourseListItem[]>('/api/courses', {
      keyword: filter.keyword,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<PublicCourseDetail>(`/api/courses/${id}`),
}
