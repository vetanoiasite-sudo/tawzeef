import type { SessionUser } from '@/shared/auth/sessionStore'
import type { AuthResult } from '../types/auth.types'

// Office staff permission resolution isn't wired up yet — no staff login UI exists to consume it
// (RBAC_PERMISSIONS.md §2). SuperAdmin bypasses the permission check entirely; a job seeker has
// none to check.
export function toSessionUser(result: AuthResult): SessionUser {
  return {
    id: result.userId,
    fullName: result.fullName,
    officeId: result.officeId,
    isSuperAdmin: result.role === 'SuperAdmin',
    permissions: [],
  }
}
