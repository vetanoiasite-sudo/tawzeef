import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const jobSeekerProfileRoutes: AppRouteDef[] = [
  {
    path: '/profile',
    Component: lazy(() => import('./pages/JobSeekerProfilePage')),
    access: { permission: 'jobseekers.viewown' },
    nav: { labelKey: 'jobSeekerProfile:pageTitle' },
  },
]
