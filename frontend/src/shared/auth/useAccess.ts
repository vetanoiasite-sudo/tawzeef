import { useSessionStore } from './sessionStore'

// Mirrors backend/src/Tawzeef.Infrastructure/Authorization/PermissionChecker.cs's
// JobSeekerOwnPermissions — granted by holding the JobSeeker role itself, not a staff grant
// (RBAC_PERMISSIONS.md §2/§3). Keep the two lists in sync.
const JOB_SEEKER_OWN_PERMISSIONS = [
  'jobseekers.viewown',
  'jobseekers.editown',
  'jobseekers.uploaddocuments',
  'jobseekers.submit',
  'jobapplications.create',
  'jobapplications.viewown',
  'jobapplications.withdraw',
  'interviews.view',
  'jobseekercourses.view',
  'jobseekercourses.create',
  'jobseekercourses.edit',
  'jobseekercourses.delete',
]

// Mirrors PermissionChecker's EmployerOwnPermissions.
const EMPLOYER_OWN_PERMISSIONS = [
  'employers.viewown',
  'employers.editown',
  'employers.uploaddocuments',
  'employers.submit',
  'jobadvertisements.view',
  'jobadvertisements.create',
  'jobadvertisements.edit',
  'jobadvertisements.submit',
  'jobadvertisements.renew',
  'jobadvertisements.close',
  'jobapplications.view',
  'jobapplications.updateoutcome',
  'interviews.view',
  'interviews.schedule',
  'interviews.edit',
  'interviewreports.create',
  'interviewreports.view',
  'hiringfollowupreports.view',
  'applicantevaluationreports.create',
  'applicantevaluationreports.view',
]

// Mirrors PermissionChecker's SharedOwnPermissions — granted to either a JobSeeker or an
// Employer simply by holding that role.
const SHARED_OWN_PERMISSIONS = [
  'complaints.create',
  'complaints.viewown',
  'suggestions.create',
  'suggestions.viewown',
  'notifications.viewown',
]

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
    if (user.role === 'JobSeeker' && JOB_SEEKER_OWN_PERMISSIONS.includes(permissionCode))
      return true
    if (user.role === 'Employer' && EMPLOYER_OWN_PERMISSIONS.includes(permissionCode)) return true
    if (
      (user.role === 'JobSeeker' || user.role === 'Employer') &&
      SHARED_OWN_PERMISSIONS.includes(permissionCode)
    )
      return true
    return user.permissions.includes(permissionCode)
  }

  return {
    isAuthenticated: user !== null,
    isSuperAdmin: user?.isSuperAdmin ?? false,
    hasPermission,
  }
}
