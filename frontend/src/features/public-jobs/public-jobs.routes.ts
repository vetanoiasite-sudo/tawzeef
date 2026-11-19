import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const publicJobsRoutes: AppRouteDef[] = [
  {
    path: '/jobs',
    Component: lazy(() => import('./pages/JobListPage')),
    access: { public: true },
    nav: { labelKey: 'publicJobs:pageTitle' },
  },
  {
    path: '/jobs/:id',
    Component: lazy(() => import('./pages/JobDetailPage')),
    access: { public: true },
  },
]
