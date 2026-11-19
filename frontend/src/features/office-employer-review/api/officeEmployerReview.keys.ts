import type { ReviewQueueFilter } from '../types/officeEmployerReview.types'

export const officeEmployerReviewKeys = {
  all: ['officeEmployerReview'] as const,
  queue: (filter: ReviewQueueFilter) => [...officeEmployerReviewKeys.all, 'queue', filter] as const,
  detail: (id: string) => [...officeEmployerReviewKeys.all, 'detail', id] as const,
}
