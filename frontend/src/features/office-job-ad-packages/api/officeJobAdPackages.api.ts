import { httpClient } from '@/shared/api/httpClient'
import type {
  JobAdvertisementPackage,
  JobAdvertisementPackagePayload,
} from '../types/officeJobAdPackages.types'

export const officeJobAdPackagesApi = {
  list: () => httpClient.get<JobAdvertisementPackage[]>('/api/office/job-advertisement-packages'),
  update: ({ id, payload }: { id: string; payload: JobAdvertisementPackagePayload }) =>
    httpClient.put<JobAdvertisementPackage>(`/api/office/job-advertisement-packages/${id}`, payload),
  activate: (id: string) =>
    httpClient.post<JobAdvertisementPackage>(`/api/office/job-advertisement-packages/${id}/activate`),
  deactivate: (id: string) =>
    httpClient.post<JobAdvertisementPackage>(`/api/office/job-advertisement-packages/${id}/deactivate`),
}
