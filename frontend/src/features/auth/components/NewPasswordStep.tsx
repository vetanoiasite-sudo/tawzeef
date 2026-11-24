import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { resetPasswordSchema, type ResetPasswordFormValues } from '../schemas/auth.schemas'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

// Forgot-password flow's last step — the email was already confirmed by the preceding OtpStep,
// whose opaque token the page already holds, so this only collects the new password.
export function NewPasswordStep({
  isSubmitting,
  submitError,
  onSubmit,
}: {
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: ResetPasswordFormValues) => void
}) {
  const { t } = useTranslation(['auth', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { newPassword: '', confirmPassword: '' },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <TextInput
        id="newPassword"
        type="password"
        autoComplete="new-password"
        label={t('auth:newPasswordLabel')}
        error={errors.newPassword ? t(errors.newPassword.message ?? '') : undefined}
        {...register('newPassword')}
      />

      <TextInput
        id="confirmPassword"
        type="password"
        autoComplete="new-password"
        label={t('auth:confirmPasswordLabel')}
        error={errors.confirmPassword ? t(errors.confirmPassword.message ?? '') : undefined}
        {...register('confirmPassword')}
      />

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" variant="primary" disabled={isSubmitting}>
        {t('auth:resetPasswordSubmit')}
      </Button>
    </form>
  )
}
