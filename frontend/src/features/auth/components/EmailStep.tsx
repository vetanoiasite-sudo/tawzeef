import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { requestEmailOtpSchema, type RequestEmailOtpFormValues } from '../schemas/auth.schemas'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

export function EmailStep({
  defaultEmail,
  isSubmitting,
  submitError,
  onSubmit,
}: {
  defaultEmail: string
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: RequestEmailOtpFormValues) => void
}) {
  const { t } = useTranslation(['auth', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RequestEmailOtpFormValues>({
    resolver: zodResolver(requestEmailOtpSchema),
    defaultValues: { email: defaultEmail },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <TextInput
        id="email"
        type="email"
        autoComplete="email"
        label={t('auth:emailLabel')}
        error={errors.email ? t(errors.email.message ?? '') : undefined}
        {...register('email')}
      />

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" variant="primary" disabled={isSubmitting}>
        {t('auth:sendCode')}
      </Button>
    </form>
  )
}
