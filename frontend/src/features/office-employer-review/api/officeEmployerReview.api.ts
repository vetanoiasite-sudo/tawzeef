import { httpClient } from '@/shared/api/httpClient'
import type {
  OfficeEmployerDetail,
  OfficeEmployerDocument,
  OfficeEmployerListItem,
  ReviewQueueFilter,
} from '../types/officeEmployerReview.types'

export const officeEmployerReviewApi = {
  listQueue: (filter: ReviewQueueFilter) =>
    httpClient.getPaged<OfficeEmployerListItem[]>('/api/office/employers', {
      verificationStatus: filter.verificationStatus,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<OfficeEmployerDetail>(`/api/office/employers/${id}`),
  approveDocument: ({ employerId, documentId }: { employerId: string; documentId: string }) =>
    httpClient.post<OfficeEmployerDocument>(`/api/office/employers/${employerId}/documents/${documentId}/approve`),
  rejectDocument: ({ employerId, documentId, reason }: { employerId: string; documentId: string; reason: string }) =>
    httpClient.post<OfficeEmployerDocument>(
      `/api/office/employers/${employerId}/documents/${documentId}/reject`,
      { reason },
    ),
  approveProfile: (employerId: string) =>
    httpClient.post<OfficeEmployerDetail>(`/api/office/employers/${employerId}/approve`),
  rejectProfile: ({ employerId, reason }: { employerId: string; reason: string }) =>
    httpClient.post<OfficeEmployerDetail>(`/api/office/employers/${employerId}/reject`, { reason }),
  stopCooperation: ({ employerId, reason }: { employerId: string; reason: string }) =>
    httpClient.post<OfficeEmployerDetail>(`/api/office/employers/${employerId}/stop-cooperation`, { reason }),
  resumeCooperation: ({ employerId, reason }: { employerId: string; reason: string }) =>
    httpClient.post<OfficeEmployerDetail>(`/api/office/employers/${employerId}/resume-cooperation`, { reason }),
}
