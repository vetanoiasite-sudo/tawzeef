import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { verifyOtpSchema, type VerifyOtpFormValues } from '../schemas/auth.schemas'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

// UI_UX_GUIDELINES.md §13: "a short 6-digit code input with a resend timer." Currently wired only
// for the email-OTP registration step (2026-11-23) — "change email" text is hardcoded accordingly;
// generalize it again if a phone-OTP flow starts reusing this component.
const RESEND_SECONDS = 60

export function OtpStep({
  contact,
  isSubmitting,
  isResending,
  submitError,
  onSubmit,
  onResend,
  onChangeContact,
}: {
  /** The phone number or email address the code was sent to. */
  contact: string
  isSubmitting: boolean
  isResending: boolean
  submitError?: string
  onSubmit: (values: VerifyOtpFormValues) => void
  onResend: () => void
  onChangeContact: () => void
}) {
  const { t } = useTranslation(['auth', 'validation'])
  const [secondsLeft, setSecondsLeft] = useState(RESEND_SECONDS)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<VerifyOtpFormValues>({ resolver: zodResolver(verifyOtpSchema) })

  useEffect(() => {
    if (secondsLeft <= 0) return
    const timer = window.setInterval(() => setSecondsLeft((s) => s - 1), 1000)
    return () => window.clearInterval(timer)
  }, [secondsLeft])

  function handleResend() {
    onResend()
    setSecondsLeft(RESEND_SECONDS)
  }

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <p className="text-sm text-text-muted">{t('auth:otpSentTo', { contact })}</p>

      <TextInput
        id="code"
        type="text"
        inputMode="numeric"
        maxLength={6}
        autoComplete="one-time-code"
        label={t('auth:otpLabel')}
        className="text-center text-lg tracking-[0.5em]"
        error={errors.code ? t(errors.code.message ?? '') : undefined}
        {...register('code')}
      />

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" variant="primary" disabled={isSubmitting}>
        {t('auth:verifyCode')}
      </Button>

      <div className="flex items-center justify-between text-sm">
        <Button type="button" variant="ghost" className="min-h-0 px-0" onClick={onChangeContact}>
          {t('auth:changeEmail')}
        </Button>
        <Button
          type="button"
          variant="ghost"
          className="min-h-0 px-0 disabled:text-text-muted"
          onClick={handleResend}
          disabled={secondsLeft > 0 || isResending}
        >
          {secondsLeft > 0 ? t('auth:resendIn', { seconds: secondsLeft }) : t('auth:resendCode')}
        </Button>
      </div>
    </form>
  )
}
