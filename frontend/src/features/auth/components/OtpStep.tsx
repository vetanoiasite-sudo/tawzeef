import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { verifyOtpSchema, type VerifyOtpFormValues } from '../schemas/auth.schemas'

// UI_UX_GUIDELINES.md §13: "a short 6-digit code input with a resend timer."
const RESEND_SECONDS = 60

export function OtpStep({
  phoneNumber,
  isSubmitting,
  isResending,
  submitError,
  onSubmit,
  onResend,
  onChangeNumber,
}: {
  phoneNumber: string
  isSubmitting: boolean
  isResending: boolean
  submitError?: string
  onSubmit: (values: VerifyOtpFormValues) => void
  onResend: () => void
  onChangeNumber: () => void
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
      <p className="text-sm text-text-muted">{t('auth:otpSentTo', { phone: phoneNumber })}</p>

      <div className="flex flex-col gap-1">
        <label htmlFor="code" className="text-sm font-medium text-text">
          {t('auth:otpLabel')}
        </label>
        <input
          id="code"
          type="text"
          inputMode="numeric"
          maxLength={6}
          autoComplete="one-time-code"
          className="rounded-md border border-surface-muted px-3 py-2 text-center text-lg tracking-[0.5em] text-text outline-none focus:border-primary"
          {...register('code')}
        />
        {errors.code && <p className="text-sm text-danger">{t(errors.code.message ?? '')}</p>}
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
      >
        {t('auth:verifyCode')}
      </button>

      <div className="flex items-center justify-between text-sm">
        <button
          type="button"
          onClick={onChangeNumber}
          className="text-text-muted underline-offset-2 hover:underline"
        >
          {t('auth:changeNumber')}
        </button>
        <button
          type="button"
          onClick={handleResend}
          disabled={secondsLeft > 0 || isResending}
          className="text-primary disabled:text-text-muted"
        >
          {secondsLeft > 0 ? t('auth:resendIn', { seconds: secondsLeft }) : t('auth:resendCode')}
        </button>
      </div>
    </form>
  )
}
