import { httpClient } from '@/shared/api/httpClient'
import type {
  CreateOfficePayload,
  Office,
  OfficePlatformStats,
  UpdateOfficePayload,
} from '../types/superAdmin.types'

export const superAdminApi = {
  listOffices: () => httpClient.get<Office[]>('/api/offices'),
  createOffice: (payload: CreateOfficePayload) => httpClient.post<Office>('/api/offices', payload),
  updateOffice: ({ id, payload }: { id: string; payload: UpdateOfficePayload }) =>
    httpClient.put<Office>(`/api/offices/${id}`, payload),
  activateOffice: (id: string) => httpClient.post<Office>(`/api/offices/${id}/activate`),
  deactivateOffice: (id: string) => httpClient.post<Office>(`/api/offices/${id}/deactivate`),
  getStats: () => httpClient.get<OfficePlatformStats>('/api/offices/stats'),
  uploadLogo: ({ id, file }: { id: string; file: File }) => {
    const form = new FormData()
    form.append('file', file)
    return httpClient.post<Office>(`/api/offices/${id}/logo`, form)
  },
}
