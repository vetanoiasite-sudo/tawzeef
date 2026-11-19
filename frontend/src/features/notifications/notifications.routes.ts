import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const notificationsRoutes: AppRouteDef[] = [
  {
    path: '/notifications',
    Component: lazy(() => import('./pages/NotificationsPage')),
    access: { permission: 'notifications.viewown' },
  },
]
