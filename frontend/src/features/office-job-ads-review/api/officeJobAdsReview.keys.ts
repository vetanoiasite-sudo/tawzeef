import type { ApplicantsFilter, ReviewQueueFilter } from '../types/officeJobAdsReview.types'

export const officeJobAdsReviewKeys = {
  all: ['officeJobAdsReview'] as const,
  queue: (filter: ReviewQueueFilter) => [...officeJobAdsReviewKeys.all, 'queue', filter] as const,
  detail: (id: string) => [...officeJobAdsReviewKeys.all, 'detail', id] as const,
  applicants: (adId: string, filter: ApplicantsFilter) =>
    [...officeJobAdsReviewKeys.all, 'applicants', adId, filter] as const,
}
