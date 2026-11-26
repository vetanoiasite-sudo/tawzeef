import { httpClient } from '@/shared/api/httpClient'
import type {
  ApplicantsFilter,
  OfficeApplicantListItem,
  OfficeJobAdDetail,
  OfficeJobAdListItem,
  ReviewQueueFilter,
  UpdateJobAdPayload,
} from '../types/officeJobAdsReview.types'

export const officeJobAdsReviewApi = {
  listQueue: (filter: ReviewQueueFilter) =>
    httpClient.getPaged<OfficeJobAdListItem[]>('/api/office/job-advertisements', {
      status: filter.status,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) =>
    httpClient.get<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}`),
  update: ({ id, payload }: { id: string; payload: UpdateJobAdPayload }) =>
    httpClient.put<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}`, payload),
  assignCategory: ({
    id,
    existingCategoryId,
    newCategoryName,
  }: {
    id: string
    existingCategoryId?: string
    newCategoryName?: string
  }) =>
    httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/category`, {
      existingCategoryId,
      newCategoryName,
    }),
  approve: (id: string) =>
    httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/approve`),
  reject: ({ id, reason }: { id: string; reason: string }) =>
    httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/reject`, { reason }),
  pause: (id: string) =>
    httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/pause`),
  resume: (id: string) =>
    httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/resume`),
  confirmPayment: ({
    id,
    jobAdvertisementPackageId,
    file,
  }: {
    id: string
    jobAdvertisementPackageId: string
    file: File
  }) => {
    const form = new FormData()
    form.append('JobAdvertisementPackageId', jobAdvertisementPackageId)
    form.append('File', file)
    return httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/payment/confirm`, form)
  },
  listApplicants: (adId: string, filter: ApplicantsFilter) =>
    httpClient.getPaged<OfficeApplicantListItem[]>(
      `/api/office/job-advertisements/${adId}/applicants`,
      {
        pageIndex: filter.pageIndex,
        pageSize: filter.pageSize,
      },
    ),
}
