import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const employerProfileRoutes: AppRouteDef[] = [
  {
    path: '/company-profile',
    Component: lazy(() => import('./pages/EmployerProfilePage')),
    access: { permission: 'employers.viewown' },
    nav: { labelKey: 'employerProfile:pageTitle' },
  },
]
