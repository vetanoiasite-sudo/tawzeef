/** Mirrors backend/src/Tawzeef.Application/Features/Auth/AuthDtos.cs. */

export type RequestOtpPayload = {
  phoneNumber: string
  /** "Registration" | "StaffInvite" — omitted defaults to Registration on the backend. */
  purpose?: string
}

export type VerifyOtpPayload = {
  phoneNumber: string
  code: string
  purpose?: string
}

export type OtpVerifiedResult = {
  otpToken: string
}

export type RequestEmailOtpPayload = {
  email: string
  purpose?: string
}

export type VerifyEmailOtpPayload = {
  email: string
  code: string
  purpose?: string
}

export type AcceptStaffInvitePayload = {
  mobileNumber: string
  password: string
}

export type RegisterJobSeekerPayload = {
  fullName: string
  nationalId: string
  mobileNumber: string
  email: string
  otpToken: string
  password: string
  /** At least one, or allOffices — confirmed 2026-11-24 (OfficeMultiSelect's doc comment). */
  officeIds: string[]
  allOffices: boolean
}

export type RegisterEmployerPayload = {
  representativeName: string
  mobileNumber: string
  email: string
  otpToken: string
  password: string
  legalName: string
  displayName: string
  /** At least one, or allOffices — confirmed 2026-11-24 (same as RegisterJobSeekerPayload). */
  officeIds: string[]
  allOffices: boolean
}

export type LoginPayload = {
  identifier: string
  password: string
}

/** The forgot-password flow's last step — otpToken comes from VerifyEmailOtpPayload's result,
 * requested/verified with purpose "PasswordReset" (ResetPasswordDto's doc comment). */
export type ResetPasswordPayload = {
  email: string
  otpToken: string
  newPassword: string
}

/** "SuperAdmin" | "OfficeAdmin" | "OfficeStaff" | "JobSeeker" | "Employer" — API_DOCUMENTATION.md §3.1. */
export type AuthResult = {
  userId: string
  fullName: string
  jobSeekerProfileId: string | null
  employerId: string | null
  role: string
  officeId: string | null
  permissionCodes: string[]
  accessToken: string
  expiresAt: string
}

/** The public registration dropdown — API_DOCUMENTATION.md §3.2. */
export type PublicOffice = {
  id: string
  name: string
  city: string | null
  /** A relative API path (e.g. /api/offices/{id}/logo) — null when no logo was uploaded yet. */
  logoUrl: string | null
}
