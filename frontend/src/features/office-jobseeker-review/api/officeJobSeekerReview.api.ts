import { httpClient } from '@/shared/api/httpClient'
import type {
  OfficeJobSeekerDetail,
  OfficeJobSeekerDocument,
  OfficeJobSeekerListItem,
  ReviewQueueFilter,
} from '../types/officeJobSeekerReview.types'

export const officeJobSeekerReviewApi = {
  listQueue: (filter: ReviewQueueFilter) =>
    httpClient.getPaged<OfficeJobSeekerListItem[]>('/api/office/job-seekers', {
      reviewStatus: filter.reviewStatus,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<OfficeJobSeekerDetail>(`/api/office/job-seekers/${id}`),
  approveDocument: ({ profileId, documentId }: { profileId: string; documentId: string }) =>
    httpClient.post<OfficeJobSeekerDocument>(`/api/office/job-seekers/${profileId}/documents/${documentId}/approve`),
  rejectDocument: ({ profileId, documentId, reason }: { profileId: string; documentId: string; reason: string }) =>
    httpClient.post<OfficeJobSeekerDocument>(
      `/api/office/job-seekers/${profileId}/documents/${documentId}/reject`,
      { reason },
    ),
  approveProfile: (profileId: string) =>
    httpClient.post<OfficeJobSeekerDetail>(`/api/office/job-seekers/${profileId}/approve`),
  rejectProfile: ({ profileId, reason }: { profileId: string; reason: string }) =>
    httpClient.post<OfficeJobSeekerDetail>(`/api/office/job-seekers/${profileId}/reject`, { reason }),
}
