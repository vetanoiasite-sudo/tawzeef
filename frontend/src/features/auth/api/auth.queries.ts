import { useMutation, useQuery } from '@tanstack/react-query'
import { authApi } from './auth.api'
import { authKeys } from './auth.keys'

export function usePublicOffices() {
  return useQuery({
    queryKey: authKeys.publicOffices(),
    queryFn: authApi.getPublicOffices,
  })
}

export function useRequestOtp() {
  return useMutation({ mutationFn: authApi.requestOtp })
}

export function useVerifyOtp() {
  return useMutation({ mutationFn: authApi.verifyOtp })
}

export function useRequestEmailOtp() {
  return useMutation({ mutationFn: authApi.requestEmailOtp })
}

export function useVerifyEmailOtp() {
  return useMutation({ mutationFn: authApi.verifyEmailOtp })
}

export function useRegisterJobSeeker() {
  return useMutation({ mutationFn: authApi.registerJobSeeker })
}

export function useRegisterEmployer() {
  return useMutation({ mutationFn: authApi.registerEmployer })
}

export function useAcceptStaffInvite() {
  return useMutation({ mutationFn: authApi.acceptStaffInvite })
}

export function useLogin() {
  return useMutation({ mutationFn: authApi.login })
}

export function useResetPassword() {
  return useMutation({ mutationFn: authApi.resetPassword })
}
