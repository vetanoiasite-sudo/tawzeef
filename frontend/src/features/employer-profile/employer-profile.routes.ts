import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const employerProfileRoutes: AppRouteDef[] = [
  {
    path: '/employer/dashboard',
    Component: lazy(() => import('./pages/EmployerDashboardPage')),
    access: { permission: 'employers.viewown' },
    nav: { labelKey: 'employerProfile:dashboardNav' },
  },
  {
    path: '/company-profile',
    Component: lazy(() => import('./pages/EmployerProfilePage')),
    access: { permission: 'employers.viewown' },
    nav: { labelKey: 'employerProfile:pageTitle' },
  },
]
