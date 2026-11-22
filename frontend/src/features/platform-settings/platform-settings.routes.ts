import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const platformSettingsRoutes: AppRouteDef[] = [
  {
    path: '/super-admin/settings',
    Component: lazy(() => import('./pages/PlatformSettingsPage')),
    access: { permission: 'platformsettings.view' },
    nav: { labelKey: 'platformSettings:nav' },
  },
]
