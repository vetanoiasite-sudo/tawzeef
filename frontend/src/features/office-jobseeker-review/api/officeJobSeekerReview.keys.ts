import type { ReviewQueueFilter } from '../types/officeJobSeekerReview.types'

export const officeJobSeekerReviewKeys = {
  all: ['officeJobSeekerReview'] as const,
  queue: (filter: ReviewQueueFilter) => [...officeJobSeekerReviewKeys.all, 'queue', filter] as const,
  detail: (id: string) => [...officeJobSeekerReviewKeys.all, 'detail', id] as const,
}
