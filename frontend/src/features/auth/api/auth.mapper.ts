import type { SessionUser } from '@/shared/auth/sessionStore'
import type { AuthResult } from '../types/auth.types'

export function toSessionUser(result: AuthResult): SessionUser {
  return {
    id: result.userId,
    fullName: result.fullName,
    officeId: result.officeId,
    isSuperAdmin: result.role === 'SuperAdmin',
    role: result.role,
    permissions: result.permissionCodes,
  }
}
