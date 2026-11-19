import { httpClient } from '@/shared/api/httpClient'
import type {
  ComplaintQueueFilter,
  ComplaintValidityDecisionCode,
  OfficeComplaint,
  OfficeDecisionTypeCode,
} from '../types/officeComplaintReview.types'

/** Only the fields this feature reads from the decide call's result — the full shape is
 * backend/src/Tawzeef.Application/Features/JobSeekers/JobSeekerProfileDtos.cs's JobSeekerProfileDto. */
export type DecideResult = {
  id: string
  fullName: string
  reviewStatus: string
}

export const officeComplaintReviewApi = {
  listQueue: (filter: ComplaintQueueFilter) =>
    httpClient.getPaged<OfficeComplaint[]>('/api/office/complaints', {
      status: filter.status,
      validityDecision: filter.validityDecision,
      subjectType: filter.subjectType,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) => httpClient.get<OfficeComplaint>(`/api/office/complaints/${id}`),
  triage: ({ id, validityDecision }: { id: string; validityDecision: ComplaintValidityDecisionCode }) =>
    httpClient.post<OfficeComplaint>(`/api/office/complaints/${id}/triage`, { validityDecision }),
  resolve: ({ id, outcome, resolutionNotes }: { id: string; outcome: 'Resolved' | 'Rejected'; resolutionNotes: string }) =>
    httpClient.post<OfficeComplaint>(`/api/office/complaints/${id}/resolve`, { outcome, resolutionNotes }),
  decide: ({ id, decisionType, reason }: { id: string; decisionType: OfficeDecisionTypeCode; reason: string }) =>
    httpClient.post<DecideResult>(`/api/office/complaints/${id}/decide`, { decisionType, reason }),
}
