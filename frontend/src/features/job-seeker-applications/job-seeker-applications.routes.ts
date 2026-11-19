import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const jobSeekerApplicationsRoutes: AppRouteDef[] = [
  {
    path: '/my-applications',
    Component: lazy(() => import('./pages/MyApplicationsPage')),
    access: { permission: 'jobapplications.viewown' },
    nav: { labelKey: 'jobSeekerApplications:pageTitle' },
  },
]
