import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const authRoutes: AppRouteDef[] = [
  {
    path: '/register/job-seeker',
    Component: lazy(() => import('./pages/RegisterJobSeekerPage')),
    access: { public: true },
  },
  {
    path: '/login',
    Component: lazy(() => import('./pages/LoginPage')),
    access: { public: true },
  },
]
