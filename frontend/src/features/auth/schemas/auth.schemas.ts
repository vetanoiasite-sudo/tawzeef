import { z } from 'zod'

// Loose international format — the backend is the source of truth for what counts as a valid
// number (AuthDtos.cs: [Required, MaxLength(30)]); this only catches obvious typos early.
const phoneSchema = z
  .string()
  .trim()
  .min(1, 'validation:required')
  .regex(/^\+?[0-9]{8,15}$/, 'validation:phoneFormat')

export const requestOtpSchema = z.object({
  phoneNumber: phoneSchema,
})
export type RequestOtpFormValues = z.infer<typeof requestOtpSchema>

export const verifyOtpSchema = z.object({
  code: z
    .string()
    .trim()
    .regex(/^[0-9]{6}$/, 'validation:otpCodeFormat'),
})
export type VerifyOtpFormValues = z.infer<typeof verifyOtpSchema>

export const jobSeekerDetailsSchema = z.object({
  fullName: z.string().trim().min(2, 'validation:required').max(200, 'validation:tooLong'),
  mobileNumber: phoneSchema,
  // Required — login is by email for every account now (temporary product decision, no OTP step
  // either — AuthDtos.cs LoginDto's doc comment).
  email: z.email('validation:emailFormat'),
  password: z.string().min(8, 'validation:passwordTooShort'),
  officeId: z.string().min(1, 'validation:required'),
})
export type JobSeekerDetailsFormValues = z.infer<typeof jobSeekerDetailsSchema>

export const employerDetailsSchema = z.object({
  representativeName: z
    .string()
    .trim()
    .min(2, 'validation:required')
    .max(200, 'validation:tooLong'),
  mobileNumber: phoneSchema,
  legalName: z.string().trim().min(2, 'validation:required').max(250, 'validation:tooLong'),
  displayName: z.string().trim().min(2, 'validation:required').max(250, 'validation:tooLong'),
  email: z.email('validation:emailFormat'),
  password: z.string().min(8, 'validation:passwordTooShort'),
  officeId: z.string().min(1, 'validation:required'),
})
export type EmployerDetailsFormValues = z.infer<typeof employerDetailsSchema>

export const acceptInvitePasswordSchema = z.object({
  mobileNumber: phoneSchema,
  password: z.string().min(8, 'validation:passwordTooShort'),
})
export type AcceptInvitePasswordFormValues = z.infer<typeof acceptInvitePasswordSchema>

// Email for JobSeeker/Employer, phone for OfficeAdmin/OfficeStaff/SuperAdmin — the one login
// screen serves both, so this only checks non-empty and leaves the actual format to the backend
// (AuthDtos.cs LoginDto's doc comment).
export const loginSchema = z.object({
  identifier: z.string().trim().min(1, 'validation:required'),
  password: z.string().min(1, 'validation:required'),
})
export type LoginFormValues = z.infer<typeof loginSchema>
