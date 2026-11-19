import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeEmployerReviewRoutes: AppRouteDef[] = [
  {
    path: '/office/employers',
    Component: lazy(() => import('./pages/ReviewQueuePage')),
    access: { permission: 'employers.view' },
    nav: { labelKey: 'officeEmployerReview:queueTitle' },
  },
  {
    path: '/office/employers/:id',
    Component: lazy(() => import('./pages/ReviewDetailPage')),
    access: { permission: 'employers.view' },
  },
]
