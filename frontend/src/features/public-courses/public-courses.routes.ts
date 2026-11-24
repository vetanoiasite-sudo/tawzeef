import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const publicCoursesRoutes: AppRouteDef[] = [
  {
    path: '/courses',
    Component: lazy(() => import('./pages/CourseListPage')),
    access: { public: true },
    nav: { labelKey: 'publicCourses:pageTitle' },
  },
  {
    path: '/courses/:id',
    Component: lazy(() => import('./pages/CourseDetailPage')),
    access: { public: true },
  },
]
