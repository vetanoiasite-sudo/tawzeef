import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { acceptInvitePasswordSchema, type AcceptInvitePasswordFormValues } from '../schemas/auth.schemas'

export function AcceptInvitePasswordStep({
  isSubmitting,
  submitError,
  onSubmit,
}: {
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: AcceptInvitePasswordFormValues) => void
}) {
  const { t } = useTranslation(['auth', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AcceptInvitePasswordFormValues>({
    resolver: zodResolver(acceptInvitePasswordSchema),
    defaultValues: { password: '' },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="password" className="text-sm font-medium text-text">
          {t('auth:passwordLabel')}
        </label>
        <input
          id="password"
          type="password"
          autoComplete="new-password"
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          {...register('password')}
        />
        {errors.password && <p className="text-sm text-danger">{t(errors.password.message ?? '')}</p>}
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
      >
        {t('auth:acceptInviteSubmit')}
      </button>
    </form>
  )
}
