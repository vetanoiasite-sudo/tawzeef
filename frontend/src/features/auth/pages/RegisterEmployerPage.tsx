import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { toSessionUser } from '../api/auth.mapper'
import {
  useRegisterEmployer,
  useRequestEmailOtp,
  useVerifyEmailOtp,
} from '../api/auth.queries'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import { EmailStep } from '../components/EmailStep'
import { EmployerDetailsStep } from '../components/EmployerDetailsStep'
import { OtpStep } from '../components/OtpStep'
import { RegistrationStepper } from '../components/RegistrationStepper'
import type { EmployerDetailsFormValues } from '../schemas/auth.schemas'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

// Step 1 of 3 at the UI level — email + OTP verification, then company/account details (no office:
// that's chosen afterward, as the real "step 2" of registration per the product ask — see
// EmployerProfilePage's office-selection card, landed on right after this, 2026-11-23).
export default function RegisterEmployerPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const [step, setStep] = useState<'email' | 'otp' | 'details'>('email')
  const [email, setEmail] = useState('')
  const [otpToken, setOtpToken] = useState('')

  const requestEmailOtp = useRequestEmailOtp()
  const verifyEmailOtp = useVerifyEmailOtp()
  const registerEmployer = useRegisterEmployer()

  function handleRequestOtp(values: { email: string }) {
    requestEmailOtp.mutate(
      { email: values.email },
      {
        onSuccess: () => {
          setEmail(values.email)
          setStep('otp')
        },
      },
    )
  }

  function handleVerifyOtp(values: { code: string }) {
    verifyEmailOtp.mutate(
      { email, code: values.code },
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
        mobileNumber: values.mobileNumber,
        email,
        otpToken,
        password: values.password,
        legalName: values.legalName,
        displayName: values.displayName,
      },
      {
        onSuccess: (result) => {
          setSession(result.accessToken, toSessionUser(result))
          // Step 2 (product ask): pick the office to apply to, then upload documents — both live
          // on the company-profile page.
          void navigate('/company-profile', { replace: true })
        },
      },
    )
  }

  return (
    <AuthScaffold maxWidth={520}>
      <AuthHeader title={t('registerEmployerTitle')} />

      <RegistrationStepper current={step} />

      {step === 'email' && (
        <>
          <p className="mb-4 text-sm text-content-secondary">{t('emailStepHint')}</p>
          <EmailStep
            defaultEmail={email}
            isSubmitting={requestEmailOtp.isPending}
            submitError={errorMessage(requestEmailOtp.error)}
            onSubmit={handleRequestOtp}
          />
        </>
      )}

      {step === 'otp' && (
        <OtpStep
          contact={email}
          isSubmitting={verifyEmailOtp.isPending}
          isResending={requestEmailOtp.isPending}
          submitError={errorMessage(verifyEmailOtp.error)}
          onSubmit={handleVerifyOtp}
          onResend={() => requestEmailOtp.mutate({ email })}
          onChangeContact={() => setStep('email')}
        />
      )}

      {step === 'details' && (
        <EmployerDetailsStep
          email={email}
          isSubmitting={registerEmployer.isPending}
          submitError={errorMessage(registerEmployer.error)}
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
          to="/register/job-seeker"
          className="font-bold text-brand hover:text-brand-hover hover:underline"
        >
          {t('registerLink')}
        </Link>
      </p>
    </AuthScaffold>
  )
}
