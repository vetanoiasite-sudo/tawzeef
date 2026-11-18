import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'

export const homeRoutes: AppRouteDef[] = [
  {
    path: '/',
    Component: lazy(() => import('./pages/HomePage')),
    access: { public: true },
    nav: { labelKey: 'nav:home' },
  },
]
