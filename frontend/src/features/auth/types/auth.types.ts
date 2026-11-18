/** Mirrors backend/src/Tawzeef.Application/Features/Auth/AuthDtos.cs. */

export type RequestOtpPayload = {
  phoneNumber: string
}

export type VerifyOtpPayload = {
  phoneNumber: string
  code: string
}

export type OtpVerifiedResult = {
  otpToken: string
}

export type RegisterJobSeekerPayload = {
  fullName: string
  mobileNumber: string
  otpToken: string
  email?: string
  password: string
  officeId: string
}

export type RegisterEmployerPayload = {
  representativeName: string
  mobileNumber: string
  otpToken: string
  email?: string
  password: string
  officeId: string
  legalName: string
  displayName: string
}

export type LoginPayload = {
  identifier: string
  password: string
}

/** "SuperAdmin" | "OfficeAdmin" | "OfficeStaff" | "JobSeeker" | "Employer" — API_DOCUMENTATION.md §3.1. */
export type AuthResult = {
  userId: string
  fullName: string
  jobSeekerProfileId: string | null
  employerId: string | null
  role: string
  officeId: string | null
  accessToken: string
  expiresAt: string
}

/** The public registration dropdown — API_DOCUMENTATION.md §3.2. */
export type PublicOffice = {
  id: string
  name: string
  city: string | null
}
