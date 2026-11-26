import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeJobAdPackagesRoutes: AppRouteDef[] = [
  {
    path: '/office/job-advertisement-packages',
    Component: lazy(() => import('./pages/JobAdPackagesManagementPage')),
    access: { permission: 'jobadvertisementpackages.manage' },
    nav: { labelKey: 'officeJobAdPackages:pageTitle' },
  },
]
