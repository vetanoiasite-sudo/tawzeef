import { useSessionStore } from './sessionStore'

/**
 * Element-level gating uses this (or <Can>), never a raw role-name comparison
 * (PROJECT_ARCHITECTURE_FRONTEND.md §13.3). This is a UX layer only — the backend's
 * PermissionAuthorizationFilter is the actual enforcement (RBAC_PERMISSIONS.md §8).
 */
export function useAccess() {
  const user = useSessionStore((s) => s.user)

  function hasPermission(permissionCode: string): boolean {
    if (!user) return false
    if (user.isSuperAdmin) return true
    return user.permissions.includes(permissionCode)
  }

  return {
    isAuthenticated: user !== null,
    isSuperAdmin: user?.isSuperAdmin ?? false,
    hasPermission,
  }
}
