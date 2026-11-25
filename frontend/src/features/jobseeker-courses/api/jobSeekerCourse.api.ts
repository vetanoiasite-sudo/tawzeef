import { httpClient } from '@/shared/api/httpClient'
import type {
  CreateJobSeekerCoursePayload,
  JobSeekerCourse,
  UpdateJobSeekerCoursePayload,
} from '../types/jobSeekerCourse.types'

export const jobSeekerCourseApi = {
  listMine: () => httpClient.get<JobSeekerCourse[]>('/api/jobseekers/me/courses'),
  create: (payload: CreateJobSeekerCoursePayload) =>
    httpClient.post<JobSeekerCourse>('/api/jobseekers/me/courses', payload),
  update: ({ id, payload }: { id: string; payload: UpdateJobSeekerCoursePayload }) =>
    httpClient.put<JobSeekerCourse>(`/api/jobseekers/me/courses/${id}`, payload),
  remove: (id: string) => httpClient.delete<void>(`/api/jobseekers/me/courses/${id}`),
  uploadCertificate: ({ id, file }: { id: string; file: File }) => {
    const form = new FormData()
    form.append('File', file)
    return httpClient.post<JobSeekerCourse>(`/api/jobseekers/me/courses/${id}/certificate`, form)
  },
  getCertificate: (id: string) => httpClient.getBlob(`/api/jobseekers/me/courses/${id}/certificate`),
}
