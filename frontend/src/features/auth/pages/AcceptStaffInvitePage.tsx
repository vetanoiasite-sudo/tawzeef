import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { roleLandingPath } from '@/shared/auth/roleLandingPath'
import { toSessionUser } from '../api/auth.mapper'
import { useAcceptStaffInvite, useRequestOtp, useVerifyOtp } from '../api/auth.queries'
import { AcceptInvitePasswordStep } from '../components/AcceptInvitePasswordStep'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import { OtpStep } from '../components/OtpStep'
import { PhoneStep } from '../components/PhoneStep'
import { RegistrationStepper } from '../components/RegistrationStepper'
import type {
  AcceptInvitePasswordFormValues,
  RequestOtpFormValues,
  VerifyOtpFormValues,
} from '../schemas/auth.schemas'

type Step = 'phone' | 'otp' | 'details'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

/// <summary>Only reachable after an OfficeAdmin/SuperAdmin invite (OfficeUsersController.Invite)
/// — the invitee proves their own number with OtpPurpose.StaffInvite, then sets their own
/// password. Nobody else ever sees or sets it.</summary>
export default function AcceptStaffInvitePage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const [step, setStep] = useState<Step>('phone')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [otpToken, setOtpToken] = useState('')

  const requestOtp = useRequestOtp()
  const verifyOtp = useVerifyOtp()
  const acceptInvite = useAcceptStaffInvite()

  function handleRequestOtp(values: RequestOtpFormValues) {
    requestOtp.mutate(
      { phoneNumber: values.phoneNumber, purpose: 'StaffInvite' },
      {
        onSuccess: () => {
          setPhoneNumber(values.phoneNumber)
          setStep('otp')
        },
      },
    )
  }

  function handleVerifyOtp(values: VerifyOtpFormValues) {
    verifyOtp.mutate(
      { phoneNumber, code: values.code, purpose: 'StaffInvite' },
      {
        onSuccess: (result) => {
          setOtpToken(result.otpToken)
          setStep('details')
        },
      },
    )
  }

  function handleAccept(values: AcceptInvitePasswordFormValues) {
    acceptInvite.mutate(
      { mobileNumber: phoneNumber, otpToken, password: values.password },
      {
        onSuccess: (result) => {
          setSession(result.accessToken, toSessionUser(result))
          void navigate(roleLandingPath(result.role), { replace: true })
        },
      },
    )
  }

  return (
    <AuthScaffold maxWidth={520}>
      <AuthHeader title={t('acceptInviteTitle')} />
      <RegistrationStepper current={step} />

      {step === 'phone' && (
        <PhoneStep
          defaultPhoneNumber={phoneNumber}
          isSubmitting={requestOtp.isPending}
          submitError={errorMessage(requestOtp.error)}
          onSubmit={handleRequestOtp}
        />
      )}

      {step === 'otp' && (
        <OtpStep
          phoneNumber={phoneNumber}
          isSubmitting={verifyOtp.isPending}
          isResending={requestOtp.isPending}
          submitError={errorMessage(verifyOtp.error)}
          onSubmit={handleVerifyOtp}
          onResend={() => requestOtp.mutate({ phoneNumber, purpose: 'StaffInvite' })}
          onChangeNumber={() => setStep('phone')}
        />
      )}

      {step === 'details' && (
        <AcceptInvitePasswordStep
          isSubmitting={acceptInvite.isPending}
          submitError={errorMessage(acceptInvite.error)}
          onSubmit={handleAccept}
        />
      )}
    </AuthScaffold>
  )
}
