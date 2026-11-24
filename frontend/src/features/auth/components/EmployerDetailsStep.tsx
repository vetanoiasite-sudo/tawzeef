import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { employerDetailsSchema, type EmployerDetailsFormValues } from '../schemas/auth.schemas'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

export function EmployerDetailsStep({
  email,
  isSubmitting,
  submitError,
  onSubmit,
}: {
  /** Already verified in the earlier email-OTP step — shown read-only, not re-collected here. */
  email: string
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: EmployerDetailsFormValues) => void
}) {
  const { t } = useTranslation(['auth', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<EmployerDetailsFormValues>({
    resolver: zodResolver(employerDetailsSchema),
    defaultValues: {
      representativeName: '',
      mobileNumber: '',
      legalName: '',
      displayName: '',
      password: '',
      confirmPassword: '',
    },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <TextInput
        id="confirmedEmail"
        type="email"
        dir="ltr"
        label={t('auth:emailLabel')}
        value={email}
        disabled
        readOnly
      />

      <TextInput
        id="legalName"
        type="text"
        label={t('auth:legalNameLabel')}
        error={errors.legalName ? t(errors.legalName.message ?? '') : undefined}
        {...register('legalName')}
      />

      <TextInput
        id="displayName"
        type="text"
        label={t('auth:displayNameLabel')}
        error={errors.displayName ? t(errors.displayName.message ?? '') : undefined}
        {...register('displayName')}
      />

      <TextInput
        id="representativeName"
        type="text"
        autoComplete="name"
        label={t('auth:representativeNameLabel')}
        error={errors.representativeName ? t(errors.representativeName.message ?? '') : undefined}
        {...register('representativeName')}
      />

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
        {t('auth:createAccount')}
      </Button>
    </form>
  )
}
