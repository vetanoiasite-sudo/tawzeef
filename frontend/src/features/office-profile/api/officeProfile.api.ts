import { httpClient } from '@/shared/api/httpClient'
import type { OfficeProfile, UpdateOfficeProfilePayload } from '../types/officeProfile.types'

export const officeProfileApi = {
  getMyOffice: () => httpClient.get<OfficeProfile>('/api/offices/me'),
  updateMyOffice: (payload: UpdateOfficeProfilePayload) =>
    httpClient.put<OfficeProfile>('/api/offices/me', payload),
  uploadMyLogo: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return httpClient.post<OfficeProfile>('/api/offices/me/logo', form)
  },
  getPublicOffice: (id: string) => httpClient.get<OfficeProfile>(`/api/offices/${id}/public`),
}
