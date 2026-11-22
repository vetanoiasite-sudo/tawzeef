import { authRoutes } from '@/features/auth'
import { complaintsRoutes } from '@/features/complaints'
import { employerJobAdsRoutes } from '@/features/employer-job-ads'
import { employerProfileRoutes } from '@/features/employer-profile'
import { homeRoutes } from '@/features/home'
import { interviewsAndReportsRoutes } from '@/features/interviews-and-reports'
import { jobSeekerApplicationsRoutes } from '@/features/job-seeker-applications'
import { jobSeekerProfileRoutes } from '@/features/jobseeker-profile'
import { notificationsRoutes } from '@/features/notifications'
import { officeComplaintsReviewRoutes } from '@/features/office-complaints-review'
import { officeDashboardRoutes } from '@/features/office-dashboard'
import { officeEmployerReviewRoutes } from '@/features/office-employer-review'
import { officeJobAdsReviewRoutes } from '@/features/office-job-ads-review'
import { officeJobSeekerReviewRoutes } from '@/features/office-jobseeker-review'
import { officeSuggestionsReviewRoutes } from '@/features/office-suggestions-review'
import { officeUsersRoutes } from '@/features/office-users'
import { publicJobsRoutes } from '@/features/public-jobs'
import { suggestionsRoutes } from '@/features/suggestions'
import type { AppRouteDef } from './types'

/**
 * The single list of routes (PROJECT_ARCHITECTURE_FRONTEND.md §13.1) — the router tree, the nav
 * menu, and any "which routes can this role reach" test all derive from this array, not from
 * three separately-maintained lists.
 */
export const routeRegistry: AppRouteDef[] = [
  ...homeRoutes,
  ...authRoutes,
  ...officeDashboardRoutes,
  ...jobSeekerProfileRoutes,
  ...employerProfileRoutes,
  ...officeJobSeekerReviewRoutes,
  ...officeEmployerReviewRoutes,
  ...complaintsRoutes,
  ...officeComplaintsReviewRoutes,
  ...officeUsersRoutes,
  ...publicJobsRoutes,
  ...employerJobAdsRoutes,
  ...officeJobAdsReviewRoutes,
  ...jobSeekerApplicationsRoutes,
  ...interviewsAndReportsRoutes,
  ...suggestionsRoutes,
  ...officeSuggestionsReviewRoutes,
  ...notificationsRoutes,
]
