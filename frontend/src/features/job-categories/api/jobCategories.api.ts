import { httpClient } from '@/shared/api/httpClient'
import type { JobCategory } from '../types/jobCategories.types'

export const jobCategoriesApi = {
  // Public endpoint (api/jobs/categories) — reused by both the anonymous search filter and the
  // office review assignment picker, no separate authenticated endpoint needed (JobCategory's
  // doc comment: categories aren't sensitive, platform-wide).
  list: () => httpClient.get<JobCategory[]>('/api/jobs/categories'),
}
