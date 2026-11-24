import { lazy } from 'react'
import type { AppRouteDef } from '@/app/router/types'
import './i18n'

export const officeCoursesRoutes: AppRouteDef[] = [
  {
    path: '/office/courses',
    Component: lazy(() => import('./pages/CoursesManagementPage')),
    access: { permission: 'courses.view' },
    nav: { labelKey: 'officeCourses:pageTitle' },
  },
  {
    path: '/office/courses/new',
    Component: lazy(() => import('./pages/CourseCreatePage')),
    access: { permission: 'courses.create' },
  },
  {
    path: '/office/courses/:id',
    Component: lazy(() => import('./pages/CourseDetailPage')),
    access: { permission: 'courses.view' },
  },
]
