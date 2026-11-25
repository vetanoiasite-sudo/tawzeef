import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeProfileRoutes: AppRouteDef[] = [
  {
    path: '/office/profile',
    Component: lazy(() => import('./pages/OfficeProfilePage')),
    access: { permission: 'offices.view' },
    nav: { labelKey: 'officeProfile:pageTitle' },
  },
  {
    path: '/offices/:id',
    Component: lazy(() => import('./pages/PublicOfficeProfilePage')),
    access: { public: true },
  },
]
