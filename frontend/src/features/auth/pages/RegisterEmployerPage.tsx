import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'
import { Card } from '@/shared/ui/Card'
import { toSessionUser } from '../api/auth.mapper'
import { useRegisterEmployer, useRequestOtp, useVerifyOtp } from '../api/auth.queries'
import { EmployerDetailsStep } from '../components/EmployerDetailsStep'
import { OtpStep } from '../components/OtpStep'
import { PhoneStep } from '../components/PhoneStep'
import { RegistrationStepper } from '../components/RegistrationStepper'
import type {
  EmployerDetailsFormValues,
  RequestOtpFormValues,
  VerifyOtpFormValues,
} from '../schemas/auth.schemas'

type Step = 'phone' | 'otp' | 'details'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

// Same shape as RegisterJobSeekerPage (BUSINESS_REQUIREMENTS.md §5): mobile + OTP → company
// details. Document upload and the office verification queue are deferred — the backend doesn't
// have them yet either (API_DOCUMENTATION.md §3.1 covers only this much).
export default function RegisterEmployerPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const [step, setStep] = useState<Step>('phone')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [otpToken, setOtpToken] = useState('')

  const requestOtp = useRequestOtp()
  const verifyOtp = useVerifyOtp()
  const registerEmployer = useRegisterEmployer()

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

  function handleRegister(values: EmployerDetailsFormValues) {
    registerEmployer.mutate(
      {
        representativeName: values.representativeName,
        mobileNumber: phoneNumber,
        otpToken,
        email: values.email || undefined,
        password: values.password,
        officeId: values.officeId,
        legalName: values.legalName,
        displayName: values.displayName,
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
        <h1 className="text-lg font-semibold text-text">{t('registerEmployerTitle')}</h1>
      </div>

      <RegistrationStepper current={step} />

      <Card>
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
          <EmployerDetailsStep
            isSubmitting={registerEmployer.isPending}
            submitError={errorMessage(registerEmployer.error)}
            onSubmit={handleRegister}
          />
        )}
      </Card>

      <p className="text-center text-sm text-text-muted">
        {t('alreadyHaveAccount')}{' '}
        <Link to="/login" className="font-medium text-primary hover:underline">
          {t('loginLink')}
        </Link>{' '}
        ·{' '}
        <Link to="/register/job-seeker" className="font-medium text-primary hover:underline">
          {t('registerLink')}
        </Link>
      </p>
    </div>
  )
}
