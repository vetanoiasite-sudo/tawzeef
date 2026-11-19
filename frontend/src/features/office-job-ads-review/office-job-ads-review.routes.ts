import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeJobAdsReviewRoutes: AppRouteDef[] = [
  {
    path: '/office/job-advertisements',
    Component: lazy(() => import('./pages/ReviewQueuePage')),
    access: { permission: 'jobadvertisements.review' },
    nav: { labelKey: 'officeJobAdsReview:queueTitle' },
  },
  {
    path: '/office/job-advertisements/:id',
    Component: lazy(() => import('./pages/ReviewDetailPage')),
    access: { permission: 'jobadvertisements.review' },
  },
  {
    path: '/office/job-advertisements/:id/applicants',
    Component: lazy(() => import('./pages/OfficeJobAdApplicantsPage')),
    access: { permission: 'jobapplications.view' },
  },
]
