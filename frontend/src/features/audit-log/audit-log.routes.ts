import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const auditLogRoutes: AppRouteDef[] = [
  {
    path: '/audit-log',
    Component: lazy(() => import('./pages/AuditLogPage')),
    access: { permission: 'auditlogs.view' },
    nav: { labelKey: 'auditLog:pageTitle' },
  },
]
