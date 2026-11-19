import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeUsersRoutes: AppRouteDef[] = [
  {
    path: '/office/users',
    Component: lazy(() => import('./pages/OfficeUsersPage')),
    access: { permission: 'officeusers.view' },
    nav: { labelKey: 'officeUsers:pageTitle' },
  },
]
