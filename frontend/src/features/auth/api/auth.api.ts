import { httpClient } from '@/shared/api/httpClient'
import type {
  AcceptStaffInvitePayload,
  AuthResult,
  LoginPayload,
  OtpVerifiedResult,
  PublicOffice,
  RegisterEmployerPayload,
  RegisterJobSeekerPayload,
  RequestEmailOtpPayload,
  RequestOtpPayload,
  ResetPasswordPayload,
  VerifyEmailOtpPayload,
  VerifyOtpPayload,
} from '../types/auth.types'

export const authApi = {
  requestOtp: (payload: RequestOtpPayload) =>
    httpClient.post<null>('/api/auth/otp/request', payload),
  verifyOtp: (payload: VerifyOtpPayload) =>
    httpClient.post<OtpVerifiedResult>('/api/auth/otp/verify', payload),
  requestEmailOtp: (payload: RequestEmailOtpPayload) =>
    httpClient.post<null>('/api/auth/otp/email/request', payload),
  verifyEmailOtp: (payload: VerifyEmailOtpPayload) =>
    httpClient.post<OtpVerifiedResult>('/api/auth/otp/email/verify', payload),
  registerJobSeeker: (payload: RegisterJobSeekerPayload) =>
    httpClient.post<AuthResult>('/api/auth/register/job-seeker', payload),
  registerEmployer: (payload: RegisterEmployerPayload) =>
    httpClient.post<AuthResult>('/api/auth/register/employer', payload),
  acceptStaffInvite: (payload: AcceptStaffInvitePayload) =>
    httpClient.post<AuthResult>('/api/auth/accept-staff-invite', payload),
  login: (payload: LoginPayload) => httpClient.post<AuthResult>('/api/auth/login', payload),
  resetPassword: (payload: ResetPasswordPayload) =>
    httpClient.post<null>('/api/auth/password/reset', payload),
  // Belongs here rather than a standalone offices feature — nothing beyond this dropdown needs
  // the office list yet (RBAC_PERMISSIONS.md §0 item 3).
  getPublicOffices: () => httpClient.get<PublicOffice[]>('/api/offices/public'),
}
