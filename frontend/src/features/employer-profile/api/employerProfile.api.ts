import { httpClient } from '@/shared/api/httpClient'
import type {
  EmployerDocument,
  EmployerDocumentTypeCode,
  EmployerProfile,
  UpdateEmployerProfilePayload,
} from '../types/employerProfile.types'

export const employerProfileApi = {
  getMyProfile: () => httpClient.get<EmployerProfile>('/api/employers/me'),
  updateMyProfile: (payload: UpdateEmployerProfilePayload) =>
    httpClient.put<EmployerProfile>('/api/employers/me', payload),
  uploadDocument: ({
    documentType,
    file,
  }: {
    documentType: EmployerDocumentTypeCode
    file: File
  }) => {
    const form = new FormData()
    form.append('DocumentType', documentType)
    form.append('File', file)
    return httpClient.post<EmployerDocument>('/api/employers/me/documents', form)
  },
  submitForReview: () => httpClient.post<EmployerProfile>('/api/employers/me/submit'),
  uploadLogo: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return httpClient.post<EmployerProfile>('/api/employers/me/logo', form)
  },
}
