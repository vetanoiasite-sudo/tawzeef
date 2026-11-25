import { auditLogRoutes } from '@/features/audit-log'
import { authRoutes } from '@/features/auth'
import { complaintsRoutes } from '@/features/complaints'
import { cooperatingCompaniesRoutes } from '@/features/cooperating-companies'
import { employerJobAdsRoutes } from '@/features/employer-job-ads'
import { employerProfileRoutes } from '@/features/employer-profile'
import { homeRoutes } from '@/features/home'
import { interviewsAndReportsRoutes } from '@/features/interviews-and-reports'
import { jobSeekerApplicationsRoutes } from '@/features/job-seeker-applications'
import { jobSeekerCoursesRoutes } from '@/features/jobseeker-courses'
import { jobSeekerProfileRoutes } from '@/features/jobseeker-profile'
import { notificationsRoutes } from '@/features/notifications'
import { officeAccountsRoutes } from '@/features/office-accounts'
import { officeBannersRoutes } from '@/features/office-banners'
import { officeComplaintsReviewRoutes } from '@/features/office-complaints-review'
import { officeDashboardRoutes } from '@/features/office-dashboard'
import { officeEmployerReviewRoutes } from '@/features/office-employer-review'
import { officeJobAdsReviewRoutes } from '@/features/office-job-ads-review'
import { officeJobSeekerReviewRoutes } from '@/features/office-jobseeker-review'
import { officeProfileRoutes } from '@/features/office-profile'
import { officeSuggestionsReviewRoutes } from '@/features/office-suggestions-review'
import { officeUsersRoutes } from '@/features/office-users'
import { platformSettingsRoutes } from '@/features/platform-settings'
import { publicJobsRoutes } from '@/features/public-jobs'
import { suggestionsRoutes } from '@/features/suggestions'
import { superAdminRoutes } from '@/features/super-admin'
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
  ...superAdminRoutes,
  ...jobSeekerProfileRoutes,
  ...jobSeekerCoursesRoutes,
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
  ...auditLogRoutes,
  ...platformSettingsRoutes,
  ...officeBannersRoutes,
  ...officeProfileRoutes,
  ...cooperatingCompaniesRoutes,
  ...officeAccountsRoutes,
]
