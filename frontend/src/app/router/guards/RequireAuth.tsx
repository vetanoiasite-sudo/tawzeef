import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAccess } from '@/shared/auth/useAccess'

export function RequireAuth({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAccess()
  const location = useLocation()

  if (!isAuthenticated) {
    // Same-origin relative path only (PROJECT_ARCHITECTURE_FRONTEND.md §13.1) — location.pathname
    // + search is always relative, never an absolute/external URL.
    const redirect = encodeURIComponent(`${location.pathname}${location.search}`)
    return <Navigate to={`/login?redirect=${redirect}`} replace />
  }

  return children
}
