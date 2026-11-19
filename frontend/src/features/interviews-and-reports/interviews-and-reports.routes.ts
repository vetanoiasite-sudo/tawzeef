import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const interviewsAndReportsRoutes: AppRouteDef[] = [
  {
    path: '/employer/applications/:id',
    Component: lazy(() => import('./pages/EmployerApplicationDetailPage')),
    access: { permission: 'interviews.view' },
  },
  {
    path: '/office/applications/:id',
    Component: lazy(() => import('./pages/OfficeApplicationDetailPage')),
    access: { permission: 'interviews.view' },
  },
  {
    path: '/office/hiring-follow-ups',
    Component: lazy(() => import('./pages/OfficeHiringFollowUpsDuePage')),
    access: { permission: 'hiringfollowupreports.view' },
    nav: { labelKey: 'interviewsAndReports:dueFollowUpsTitle' },
  },
]
