import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const jobSeekerCoursesRoutes: AppRouteDef[] = [
  {
    path: '/my-courses',
    Component: lazy(() => import('./pages/MyCoursesPage')),
    access: { permission: 'jobseekercourses.view' },
    nav: { labelKey: 'jobSeekerCourses:pageTitle' },
  },
]
