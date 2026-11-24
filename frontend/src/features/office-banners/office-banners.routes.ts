import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeBannersRoutes: AppRouteDef[] = [
  {
    path: '/office/banners',
    Component: lazy(() => import('./pages/BannersManagementPage')),
    access: { permission: 'banners.view' },
    nav: { labelKey: 'officeBanners:pageTitle' },
  },
]
