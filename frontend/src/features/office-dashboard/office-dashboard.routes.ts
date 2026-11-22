import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeDashboardRoutes: AppRouteDef[] = [
  {
    path: '/office/dashboard',
    Component: lazy(() => import('./pages/OfficeDashboardPage')),
    access: { permission: 'dashboards.view' },
    nav: { labelKey: 'officeDashboard:dashboardNav' },
  },
]
