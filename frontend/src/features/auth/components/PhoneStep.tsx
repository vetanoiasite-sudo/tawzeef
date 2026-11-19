import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { requestOtpSchema, type RequestOtpFormValues } from '../schemas/auth.schemas'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

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
      <TextInput
        id="phoneNumber"
        type="tel"
        autoComplete="tel"
        label={t('auth:phoneLabel')}
        placeholder={t('auth:phonePlaceholder')}
        error={errors.phoneNumber ? t(errors.phoneNumber.message ?? '') : undefined}
        {...register('phoneNumber')}
      />

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" variant="primary" disabled={isSubmitting}>
        {t('auth:sendCode')}
      </Button>
    </form>
  )
}
