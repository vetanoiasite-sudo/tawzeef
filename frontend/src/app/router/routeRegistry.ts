import { authRoutes } from '@/features/auth'
import { homeRoutes } from '@/features/home'
import { jobSeekerProfileRoutes } from '@/features/jobseeker-profile'
import { officeJobSeekerReviewRoutes } from '@/features/office-jobseeker-review'
import type { AppRouteDef } from './types'

/**
 * The single list of routes (PROJECT_ARCHITECTURE_FRONTEND.md §13.1) — the router tree, the nav
 * menu, and any "which routes can this role reach" test all derive from this array, not from
 * three separately-maintained lists.
 */
export const routeRegistry: AppRouteDef[] = [
  ...homeRoutes,
  ...authRoutes,
  ...jobSeekerProfileRoutes,
  ...officeJobSeekerReviewRoutes,
]
