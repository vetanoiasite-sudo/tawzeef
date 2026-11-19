import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const suggestionsRoutes: AppRouteDef[] = [
  {
    path: '/suggestions',
    Component: lazy(() => import('./pages/MySuggestionsPage')),
    access: { permission: 'suggestions.viewown' },
    nav: { labelKey: 'suggestions:pageTitle' },
  },
]
