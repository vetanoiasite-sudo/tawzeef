import type { ReactNode } from 'react'
import { useAccess } from '@/shared/auth/useAccess'

type CanProps = {
  permission: string
  children: ReactNode
  fallback?: ReactNode
}

/** PROJECT_ARCHITECTURE_FRONTEND.md §10.3 — access-gated rendering, never a role-name check. */
export function Can({ permission, children, fallback = null }: CanProps) {
  const { hasPermission } = useAccess()
  return hasPermission(permission) ? children : fallback
}
