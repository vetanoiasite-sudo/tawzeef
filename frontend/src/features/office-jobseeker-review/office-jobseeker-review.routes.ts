import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeJobSeekerReviewRoutes: AppRouteDef[] = [
  {
    path: '/office/job-seekers',
    Component: lazy(() => import('./pages/ReviewQueuePage')),
    access: { permission: 'jobseekers.view' },
    nav: { labelKey: 'officeJobSeekerReview:queueTitle' },
  },
  {
    path: '/office/job-seekers/:id',
    Component: lazy(() => import('./pages/ReviewDetailPage')),
    access: { permission: 'jobseekers.view' },
  },
]
