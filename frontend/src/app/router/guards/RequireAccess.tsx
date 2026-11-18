import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAccess } from '@/shared/auth/useAccess'

export function RequireAccess({
  permission,
  children,
}: {
  permission: string
  children: ReactNode
}) {
  const { hasPermission } = useAccess()
  return hasPermission(permission) ? children : <Navigate to="/forbidden" replace />
}
