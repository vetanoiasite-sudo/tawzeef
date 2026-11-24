import { httpClient } from '@/shared/api/httpClient'
import type { OfficeBanner } from '../types/officeBanners.types'

export const officeBannersApi = {
  list: () => httpClient.get<OfficeBanner[]>('/api/office/banners'),
  create: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return httpClient.post<OfficeBanner>('/api/office/banners', form)
  },
  pause: (id: string) => httpClient.post<OfficeBanner>(`/api/office/banners/${id}/pause`),
  resume: (id: string) => httpClient.post<OfficeBanner>(`/api/office/banners/${id}/resume`),
}
