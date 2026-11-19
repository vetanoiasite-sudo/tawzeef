import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { toSessionUser } from '../api/auth.mapper'
import { useRegisterJobSeeker, useRequestOtp, useVerifyOtp } from '../api/auth.queries'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import { DetailsStep } from '../components/DetailsStep'
import { OtpStep } from '../components/OtpStep'
import { PhoneStep } from '../components/PhoneStep'
import { RegistrationStepper } from '../components/RegistrationStepper'
import type {
  JobSeekerDetailsFormValues,
  RequestOtpFormValues,
  VerifyOtpFormValues,
} from '../schemas/auth.schemas'

type Step = 'phone' | 'otp' | 'details'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

// First vertical slice (BUSINESS_REQUIREMENTS.md §5): mobile + OTP → account details. Profile
// completion, document upload and the office review queue are deferred — the backend doesn't
// have them yet either (API_DOCUMENTATION.md §3.1 covers only this much).
export default function RegisterJobSeekerPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const [step, setStep] = useState<Step>('phone')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [otpToken, setOtpToken] = useState('')

  const requestOtp = useRequestOtp()
  const verifyOtp = useVerifyOtp()
  const registerJobSeeker = useRegisterJobSeeker()

  function handleRequestOtp(values: RequestOtpFormValues) {
    requestOtp.mutate(values, {
      onSuccess: () => {
        setPhoneNumber(values.phoneNumber)
        setStep('otp')
      },
    })
  }

  function handleVerifyOtp(values: VerifyOtpFormValues) {
    verifyOtp.mutate(
      { phoneNumber, code: values.code },
      {
        onSuccess: (result) => {
          setOtpToken(result.otpToken)
          setStep('details')
        },
      },
    )
  }

  function handleRegister(values: JobSeekerDetailsFormValues) {
    registerJobSeeker.mutate(
      {
        fullName: values.fullName,
        mobileNumber: phoneNumber,
        otpToken,
        email: values.email || undefined,
        password: values.password,
        officeId: values.officeId,
      },
      {
        onSuccess: (result) => {
          setSession(result.accessToken, toSessionUser(result))
          void navigate('/', { replace: true })
        },
      },
    )
  }

  return (
    <AuthScaffold maxWidth={520}>
      <AuthHeader title={t('registerTitle')} />
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
          onResend={() => requestOtp.mutate({ phoneNumber })}
          onChangeNumber={() => setStep('phone')}
        />
      )}

      {step === 'details' && (
        <DetailsStep
          isSubmitting={registerJobSeeker.isPending}
          submitError={errorMessage(registerJobSeeker.error)}
          onSubmit={handleRegister}
        />
      )}

      <p className="mt-6 text-center text-[14px] text-content-secondary">
        {t('alreadyHaveAccount')}{' '}
        <Link to="/login" className="font-bold text-brand hover:text-brand-hover hover:underline">
          {t('loginLink')}
        </Link>
        {' · '}
        <Link
          to="/register/employer"
          className="font-bold text-brand hover:text-brand-hover hover:underline"
        >
          {t('registerEmployerLink')}
        </Link>
      </p>
    </AuthScaffold>
  )
}
