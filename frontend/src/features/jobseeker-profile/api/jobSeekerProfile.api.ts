import { httpClient } from '@/shared/api/httpClient'
import type {
  JobSeekerDocument,
  JobSeekerDocumentTypeCode,
  JobSeekerOfficeStatus,
  JobSeekerProfile,
  SubmitJobSeekerOfficesPayload,
  UpdateJobSeekerProfilePayload,
} from '../types/jobSeekerProfile.types'

export const jobSeekerProfileApi = {
  getMyProfile: () => httpClient.get<JobSeekerProfile>('/api/jobseekers/me'),
  updateMyProfile: (payload: UpdateJobSeekerProfilePayload) =>
    httpClient.put<JobSeekerProfile>('/api/jobseekers/me', payload),
  uploadDocument: ({
    documentType,
    file,
  }: {
    documentType: JobSeekerDocumentTypeCode
    file: File
  }) => {
    const form = new FormData()
    form.append('DocumentType', documentType)
    form.append('File', file)
    return httpClient.post<JobSeekerDocument>('/api/jobseekers/me/documents', form)
  },
  submitForReview: () => httpClient.post<JobSeekerProfile>('/api/jobseekers/me/submit'),
  getMyOffices: () => httpClient.get<JobSeekerOfficeStatus[]>('/api/jobseekers/me/offices'),
  submitToOffices: (payload: SubmitJobSeekerOfficesPayload) =>
    httpClient.post<JobSeekerOfficeStatus[]>('/api/jobseekers/me/offices', payload),
  uploadPhoto: (file: File) => {
    const form = new FormData()
    form.append('File', file)
    return httpClient.post<JobSeekerProfile>('/api/jobseekers/me/photo', form)
  },
  getPhoto: () => httpClient.getBlob('/api/jobseekers/me/photo'),
}
