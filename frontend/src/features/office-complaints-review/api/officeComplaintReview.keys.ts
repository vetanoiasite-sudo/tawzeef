import type { ComplaintQueueFilter } from '../types/officeComplaintReview.types'

export const officeComplaintReviewKeys = {
  all: ['officeComplaintReview'] as const,
  queue: (filter: ComplaintQueueFilter) =>
    [...officeComplaintReviewKeys.all, 'queue', filter] as const,
  detail: (id: string) => [...officeComplaintReviewKeys.all, 'detail', id] as const,
}
