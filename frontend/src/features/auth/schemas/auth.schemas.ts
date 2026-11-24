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

export const requestEmailOtpSchema = z.object({
  email: z.email('validation:emailFormat'),
})
export type RequestEmailOtpFormValues = z.infer<typeof requestEmailOtpSchema>

export const verifyOtpSchema = z.object({
  code: z
    .string()
    .trim()
    .regex(/^[0-9]{6}$/, 'validation:otpCodeFormat'),
})
export type VerifyOtpFormValues = z.infer<typeof verifyOtpSchema>

// Egyptian national ID: exactly 14 digits (AuthDtos.cs RegisterJobSeekerDto.NationalId).
const nationalIdSchema = z
  .string()
  .trim()
  .regex(/^\d{14}$/, 'validation:nationalIdFormat')

// Email is collected and verified by the earlier email-OTP step (EmailStep/OtpStep), not here.
// Confirmed 2026-11-24: a job seeker may submit to several offices (or all of them) at
// registration, not just one — see OfficeMultiSelect.
export const jobSeekerDetailsSchema = z
  .object({
    fullName: z.string().trim().min(2, 'validation:required').max(200, 'validation:tooLong'),
    nationalId: nationalIdSchema,
    mobileNumber: phoneSchema,
    password: z.string().min(8, 'validation:passwordTooShort'),
    confirmPassword: z.string().min(1, 'validation:required'),
    // No Zod .default() here — RHF's own defaultValues already seeds these as []/false, and a
    // Zod-level default would make the schema's input/output types diverge (officeIds optional on
    // input, required on output), which breaks useForm<JobSeekerDetailsFormValues>'s single type
    // parameter (confirmed 2026-11-24, caught by `tsc -b`, the project's real typecheck command —
    // bare `tsc --noEmit` on the root tsconfig.json no-ops since it has an empty "files": []).
    officeIds: z.array(z.string()),
    allOffices: z.boolean(),
  })
  .refine((v) => v.password === v.confirmPassword, {
    message: 'validation:passwordMismatch',
    path: ['confirmPassword'],
  })
  .refine((v) => v.allOffices || v.officeIds.length > 0, {
    message: 'validation:required',
    path: ['officeIds'],
  })
export type JobSeekerDetailsFormValues = z.infer<typeof jobSeekerDetailsSchema>

export const employerDetailsSchema = z
  .object({
    representativeName: z
      .string()
      .trim()
      .min(2, 'validation:required')
      .max(200, 'validation:tooLong'),
    mobileNumber: phoneSchema,
    legalName: z.string().trim().min(2, 'validation:required').max(250, 'validation:tooLong'),
    displayName: z.string().trim().min(2, 'validation:required').max(250, 'validation:tooLong'),
    password: z.string().min(8, 'validation:passwordTooShort'),
    confirmPassword: z.string().min(1, 'validation:required'),
  })
  .refine((v) => v.password === v.confirmPassword, {
    message: 'validation:passwordMismatch',
    path: ['confirmPassword'],
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

// Forgot-password flow — step 1 (request a code) reuses requestEmailOtpSchema above, and step 2
// (verify it) reuses verifyOtpSchema; this is just the final new-password step.
export const resetPasswordSchema = z
  .object({
    newPassword: z.string().min(8, 'validation:passwordTooShort'),
    confirmPassword: z.string().min(1, 'validation:required'),
  })
  .refine((v) => v.newPassword === v.confirmPassword, {
    message: 'validation:passwordMismatch',
    path: ['confirmPassword'],
  })
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>
