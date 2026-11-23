import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import {
  acceptInvitePasswordSchema,
  type AcceptInvitePasswordFormValues,
} from '../schemas/auth.schemas'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

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
    defaultValues: { mobileNumber: '', password: '' },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <p className="text-sm text-content-secondary">{t('auth:acceptInviteMobileHint')}</p>

      <TextInput
        id="mobileNumber"
        type="tel"
        dir="ltr"
        autoComplete="tel"
        placeholder={t('auth:phonePlaceholder')}
        label={t('auth:phoneLabel')}
        error={errors.mobileNumber ? t(errors.mobileNumber.message ?? '') : undefined}
        {...register('mobileNumber')}
      />

      <TextInput
        id="password"
        type="password"
        autoComplete="new-password"
        label={t('auth:passwordLabel')}
        error={errors.password ? t(errors.password.message ?? '') : undefined}
        {...register('password')}
      />

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" variant="primary" disabled={isSubmitting}>
        {t('auth:acceptInviteSubmit')}
      </Button>
    </form>
  )
}
