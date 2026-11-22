import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const superAdminRoutes: AppRouteDef[] = [
  {
    path: '/super-admin/dashboard',
    Component: lazy(() => import('./pages/SuperAdminDashboardPage')),
    access: { permission: 'offices.view' },
    nav: { labelKey: 'superAdmin:dashboardNav' },
  },
  {
    path: '/super-admin/offices',
    Component: lazy(() => import('./pages/SuperAdminOfficesPage')),
    access: { permission: 'offices.view' },
    nav: { labelKey: 'superAdmin:officesPageTitle' },
  },
]
