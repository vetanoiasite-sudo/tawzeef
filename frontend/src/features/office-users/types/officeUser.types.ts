/** Mirrors backend/src/Tawzeef.Application/Features/OfficeUsers/OfficeUserDtos.cs. */

export type OfficeUserRoleCode = 'OfficeAdmin' | 'OfficeStaff'
export type MembershipStatusCode = 'Invited' | 'Active' | 'Revoked'

export type OfficeUser = {
  membershipId: string
  userId: string
  fullName: string
  mobileNumber: string
  role: OfficeUserRoleCode
  membershipStatus: MembershipStatusCode
  permissionCodes: string[]
}

export type InviteOfficeUserPayload = {
  fullName: string
  mobileNumber: string
  email: string
  role: OfficeUserRoleCode
  permissionCodes: string[]
  /** Only meaningful for a SuperAdmin caller inviting the first OfficeAdmin into a brand-new
   * office (OfficeUsersController.Invite's dual-mode doc comment) — an OfficeAdmin inviter always
   * invites into their own office regardless of what's sent here, so omit it in that case. */
  officeId?: string
}

/** Curated subset of PermissionCatalogue.All — only modules with a real, built feature behind
 * them (jobseekers review queue, complaints review), and never an
 * OfficeAdminOnly code (jobseekers.suspend/reinstate, complaints.decide, …) since the backend
 * would reject those for an OfficeStaff grant anyway. */
export const GRANTABLE_PERMISSION_CODES = [
  'jobseekers.view',
  'jobseekers.viewdocuments',
  'jobseekers.review',
  'jobseekers.approve',
  'jobseekers.reject',
  'complaints.view',
  'complaints.triage',
  'complaints.resolve',
] as const
