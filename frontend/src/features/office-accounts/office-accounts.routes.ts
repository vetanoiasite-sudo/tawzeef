import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeAccountsRoutes: AppRouteDef[] = [
  {
    path: '/office/accounts',
    Component: lazy(() => import('./pages/OfficeAccountsPage')),
    access: { permission: 'advertisementpayments.view' },
    nav: { labelKey: 'officeAccounts:pageTitle' },
  },
]
