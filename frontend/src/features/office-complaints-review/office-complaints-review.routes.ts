import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeComplaintsReviewRoutes: AppRouteDef[] = [
  {
    path: '/office/complaints',
    Component: lazy(() => import('./pages/ComplaintQueuePage')),
    access: { permission: 'complaints.view' },
    nav: { labelKey: 'officeComplaintReview:queueTitle' },
  },
  {
    path: '/office/complaints/:id',
    Component: lazy(() => import('./pages/ComplaintDetailPage')),
    access: { permission: 'complaints.view' },
  },
]
