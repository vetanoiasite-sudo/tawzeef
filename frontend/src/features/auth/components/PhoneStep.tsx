import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { requestOtpSchema, type RequestOtpFormValues } from '../schemas/auth.schemas'

export function PhoneStep({
  defaultPhoneNumber,
  isSubmitting,
  submitError,
  onSubmit,
}: {
  defaultPhoneNumber: string
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: RequestOtpFormValues) => void
}) {
  const { t } = useTranslation(['auth', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RequestOtpFormValues>({
    resolver: zodResolver(requestOtpSchema),
    defaultValues: { phoneNumber: defaultPhoneNumber },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="phoneNumber" className="text-sm font-medium text-text">
          {t('auth:phoneLabel')}
        </label>
        <input
          id="phoneNumber"
          type="tel"
          autoComplete="tel"
          placeholder={t('auth:phonePlaceholder')}
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          {...register('phoneNumber')}
        />
        {errors.phoneNumber && (
          <p className="text-sm text-danger">{t(errors.phoneNumber.message ?? '')}</p>
        )}
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
      >
        {t('auth:sendCode')}
      </button>
    </form>
  )
}
