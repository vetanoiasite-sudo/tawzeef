import { Suspense, type ReactNode } from 'react'
import { createBrowserRouter, type RouteObject } from 'react-router-dom'
import { AppLayout } from '@/app/layouts/AppLayout'
import { routeRegistry } from './routeRegistry'
import type { AppRouteDef } from './types'
import { RequireAccess } from './guards/RequireAccess'
import { RequireAuth } from './guards/RequireAuth'
import { RouteErrorBoundary } from './RouteErrorBoundary'
import ForbiddenPage from './ForbiddenPage'
import NotFoundPage from './NotFoundPage'

function guard(element: ReactNode, access: AppRouteDef['access']): ReactNode {
  if (access.public) return element
  return (
    <RequireAuth>
      <RequireAccess permission={access.permission}>{element}</RequireAccess>
    </RequireAuth>
  )
}

function toRouteObject(route: AppRouteDef): RouteObject {
  const element = <Suspense fallback={null}>{guard(<route.Component />, route.access)}</Suspense>

  // react-router children use relative paths; "/" is the index route, not a path segment.
  return route.path === '/'
    ? { index: true, element }
    : { path: route.path.replace(/^\//, ''), element }
}

const children: RouteObject[] = [
  ...routeRegistry.map(toRouteObject),
  { path: 'forbidden', element: <ForbiddenPage /> },
  { path: '*', element: <NotFoundPage /> },
]

export const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    errorElement: <RouteErrorBoundary />,
    children,
  },
])
