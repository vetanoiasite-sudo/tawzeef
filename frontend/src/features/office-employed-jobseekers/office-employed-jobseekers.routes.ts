import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeEmployedJobSeekersRoutes: AppRouteDef[] = [
  {
    path: '/office/job-seekers/employed',
    Component: lazy(() => import('./pages/OfficeEmployedJobSeekersPage')),
    access: { permission: 'jobseekers.view' },
    nav: { labelKey: 'officeEmployedJobSeekers:pageTitle' },
  },
]
