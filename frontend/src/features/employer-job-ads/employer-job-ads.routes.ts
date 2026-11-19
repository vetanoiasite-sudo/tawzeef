import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const employerJobAdsRoutes: AppRouteDef[] = [
  {
    path: '/employer/job-advertisements',
    Component: lazy(() => import('./pages/MyJobAdsPage')),
    access: { permission: 'jobadvertisements.view' },
    nav: { labelKey: 'employerJobAds:pageTitle' },
  },
  {
    path: '/employer/job-advertisements/new',
    Component: lazy(() => import('./pages/JobAdCreatePage')),
    access: { permission: 'jobadvertisements.create' },
  },
  {
    path: '/employer/job-advertisements/:id',
    Component: lazy(() => import('./pages/JobAdDetailPage')),
    access: { permission: 'jobadvertisements.view' },
  },
  {
    path: '/employer/job-advertisements/:id/applicants',
    Component: lazy(() => import('./pages/JobAdApplicantsPage')),
    access: { permission: 'jobapplications.view' },
  },
]
