import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeSuggestionsReviewRoutes: AppRouteDef[] = [
  {
    path: '/office/suggestions',
    Component: lazy(() => import('./pages/SuggestionQueuePage')),
    access: { permission: 'suggestions.view' },
    nav: { labelKey: 'officeSuggestionsReview:queueTitle' },
  },
  {
    path: '/office/suggestions/:id',
    Component: lazy(() => import('./pages/SuggestionDetailPage')),
    access: { permission: 'suggestions.view' },
  },
]
