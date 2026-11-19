import { httpClient } from '@/shared/api/httpClient'
import type {
  ApplicantsFilter,
  OfficeApplicantListItem,
  OfficeJobAdDetail,
  OfficeJobAdListItem,
  ReviewQueueFilter,
} from '../types/officeJobAdsReview.types'

export const officeJobAdsReviewApi = {
  listQueue: (filter: ReviewQueueFilter) =>
    httpClient.getPaged<OfficeJobAdListItem[]>('/api/office/job-advertisements', {
      status: filter.status,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}`),
  approve: (id: string) => httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/approve`),
  reject: ({ id, reason }: { id: string; reason: string }) =>
    httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/reject`, { reason }),
  pause: (id: string) => httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/pause`),
  resume: (id: string) => httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/resume`),
  confirmPayment: ({ id, amount, currency }: { id: string; amount: number; currency: string }) =>
    httpClient.post<OfficeJobAdDetail>(`/api/office/job-advertisements/${id}/payment/confirm`, { amount, currency }),
  listApplicants: (adId: string, filter: ApplicantsFilter) =>
    httpClient.getPaged<OfficeApplicantListItem[]>(`/api/office/job-advertisements/${adId}/applicants`, {
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
}
