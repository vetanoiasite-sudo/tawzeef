import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { toSessionUser } from '../api/auth.mapper'
import {
  useRegisterJobSeeker,
  useRequestEmailOtp,
  useVerifyEmailOtp,
} from '../api/auth.queries'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import { DetailsStep } from '../components/DetailsStep'
import { EmailStep } from '../components/EmailStep'
import { OtpStep } from '../components/OtpStep'
import { RegistrationStepper } from '../components/RegistrationStepper'
import type { JobSeekerDetailsFormValues } from '../schemas/auth.schemas'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

// Step 1 of 3 at the UI level — email + OTP verification, then account details (name/national
// ID/mobile/password/office). After the account is created, the job seeker lands on the profile
// page for the actual "step 2" of registration per the product ask: document upload (2026-11-23).
export default function RegisterJobSeekerPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const [step, setStep] = useState<'email' | 'otp' | 'details'>('email')
  const [email, setEmail] = useState('')
  const [otpToken, setOtpToken] = useState('')

  const requestEmailOtp = useRequestEmailOtp()
  const verifyEmailOtp = useVerifyEmailOtp()
  const registerJobSeeker = useRegisterJobSeeker()

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

  function handleRegister(values: JobSeekerDetailsFormValues) {
    registerJobSeeker.mutate(
      {
        fullName: values.fullName,
        nationalId: values.nationalId,
        mobileNumber: values.mobileNumber,
        email,
        otpToken,
        password: values.password,
        officeIds: values.officeIds,
        allOffices: values.allOffices,
      },
      {
        onSuccess: (result) => {
          setSession(result.accessToken, toSessionUser(result))
          // Step 2 of registration (product ask): upload the required documents, on the profile page.
          void navigate('/profile', { replace: true })
        },
      },
    )
  }

  return (
    <AuthScaffold maxWidth={520}>
      <AuthHeader title={t('registerTitle')} />

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
        <DetailsStep
          email={email}
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
