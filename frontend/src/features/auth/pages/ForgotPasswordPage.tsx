import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import { EmailStep } from '../components/EmailStep'
import { NewPasswordStep } from '../components/NewPasswordStep'
import { OtpStep } from '../components/OtpStep'
import { useRequestEmailOtp, useResetPassword, useVerifyEmailOtp } from '../api/auth.queries'
import type { ResetPasswordFormValues } from '../schemas/auth.schemas'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

// Any system user with an email on file (JobSeeker/Employer) — email → OTP → new password, the
// same step machine as RegisterJobSeekerPage, with Purpose: "PasswordReset" on the shared OTP
// endpoints (ResetPasswordDto's doc comment on the backend). The otpToken is cached in state once
// verified, so a failed reset submit retries without re-verifying the (single-use) code.
export default function ForgotPasswordPage() {
  const { t } = useTranslation('auth')

  const [step, setStep] = useState<'email' | 'otp' | 'newPassword' | 'done'>('email')
  const [email, setEmail] = useState('')
  const [otpToken, setOtpToken] = useState('')

  const requestEmailOtp = useRequestEmailOtp()
  const verifyEmailOtp = useVerifyEmailOtp()
  const resetPassword = useResetPassword()

  function handleRequestOtp(values: { email: string }) {
    requestEmailOtp.mutate(
      { email: values.email, purpose: 'PasswordReset' },
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
      { email, code: values.code, purpose: 'PasswordReset' },
      {
        onSuccess: (result) => {
          setOtpToken(result.otpToken)
          setStep('newPassword')
        },
      },
    )
  }

  function handleResetPassword(values: ResetPasswordFormValues) {
    resetPassword.mutate(
      { email, otpToken, newPassword: values.newPassword },
      { onSuccess: () => setStep('done') },
    )
  }

  return (
    <AuthScaffold>
      <AuthHeader title={t('forgotPasswordTitle')} />

      {step === 'email' && (
        <>
          <p className="mb-4 text-sm text-content-secondary">{t('forgotPasswordHint')}</p>
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
          onResend={() => requestEmailOtp.mutate({ email, purpose: 'PasswordReset' })}
          onChangeContact={() => setStep('email')}
        />
      )}

      {step === 'newPassword' && (
        <NewPasswordStep
          isSubmitting={resetPassword.isPending}
          submitError={errorMessage(resetPassword.error)}
          onSubmit={handleResetPassword}
        />
      )}

      {step === 'done' && (
        <p className="text-center text-sm text-content-secondary">{t('resetPasswordSuccess')}</p>
      )}

      <p className="mt-6 text-center text-[14px] text-content-secondary">
        <Link to="/login" className="font-bold text-brand hover:text-brand-hover hover:underline">
          {t('backToLogin')}
        </Link>
      </p>
    </AuthScaffold>
  )
}
