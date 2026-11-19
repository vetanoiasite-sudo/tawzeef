import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const complaintsRoutes: AppRouteDef[] = [
  {
    path: '/complaints',
    Component: lazy(() => import('./pages/MyComplaintsPage')),
    access: { permission: 'complaints.viewown' },
    nav: { labelKey: 'complaints:pageTitle' },
  },
]
