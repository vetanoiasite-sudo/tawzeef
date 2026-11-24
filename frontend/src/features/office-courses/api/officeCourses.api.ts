import { httpClient } from '@/shared/api/httpClient'
import type { CourseFormValues, CourseQueryFilter, OfficeCourse } from '../types/officeCourses.types'

export const officeCoursesApi = {
  list: (filter: CourseQueryFilter) =>
    httpClient.getPaged<OfficeCourse[]>('/api/office/courses', {
      keyword: filter.keyword,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<OfficeCourse>(`/api/office/courses/${id}`),
  create: (values: CourseFormValues) => httpClient.post<OfficeCourse>('/api/office/courses', values),
  update: (id: string, values: CourseFormValues) =>
    httpClient.put<OfficeCourse>(`/api/office/courses/${id}`, values),
  uploadImage: (id: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return httpClient.post<OfficeCourse>(`/api/office/courses/${id}/image`, form)
  },
  pause: (id: string) => httpClient.post<OfficeCourse>(`/api/office/courses/${id}/pause`),
  resume: (id: string) => httpClient.post<OfficeCourse>(`/api/office/courses/${id}/resume`),
}
