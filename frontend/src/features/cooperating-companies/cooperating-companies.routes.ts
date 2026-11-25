import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'

export const cooperatingCompaniesRoutes: AppRouteDef[] = [
  {
    path: '/companies',
    Component: lazy(() => import('./pages/CompanyListPage')),
    access: { public: true },
  },
  {
    path: '/companies/:id',
    Component: lazy(() => import('./pages/CompanyDetailPage')),
    access: { public: true },
  },
]
