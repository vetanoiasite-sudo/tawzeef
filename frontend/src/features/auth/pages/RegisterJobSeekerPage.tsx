import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'
import { toSessionUser } from '../api/auth.mapper'
import { useRegisterJobSeeker, useRequestOtp, useVerifyOtp } from '../api/auth.queries'
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
    <div className="mx-auto flex max-w-md flex-col gap-6 px-4 py-10">
      <div className="flex flex-col items-center gap-2">
        <TawzeefLogo height={48} />
        <h1 className="text-lg font-semibold text-text">{t('registerTitle')}</h1>
      </div>

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

      <p className="text-center text-sm text-text-muted">
        {t('alreadyHaveAccount')}{' '}
        <Link to="/login" className="text-primary">
          {t('loginLink')}
        </Link>{' '}
        ·{' '}
        <Link to="/register/employer" className="text-primary">
          {t('registerEmployerLink')}
        </Link>
      </p>
    </div>
  )
}
