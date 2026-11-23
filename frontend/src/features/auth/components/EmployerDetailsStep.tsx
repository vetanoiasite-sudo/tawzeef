import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { usePublicOffices } from '../api/auth.queries'
import { employerDetailsSchema, type EmployerDetailsFormValues } from '../schemas/auth.schemas'
import { TextInput } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'

export function EmployerDetailsStep({
  isSubmitting,
  submitError,
  onSubmit,
}: {
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: EmployerDetailsFormValues) => void
}) {
  const { t } = useTranslation(['auth', 'validation'])
  const { data: offices, isLoading: officesLoading } = usePublicOffices()
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
      email: '',
      password: '',
      officeId: '',
    },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
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
        id="email"
        type="email"
        autoComplete="email"
        label={t('auth:emailLabel')}
        error={errors.email ? t(errors.email.message ?? '') : undefined}
        {...register('email')}
      />

      <TextInput
        id="password"
        type="password"
        autoComplete="new-password"
        label={t('auth:passwordLabel')}
        error={errors.password ? t(errors.password.message ?? '') : undefined}
        {...register('password')}
      />

      <div className="flex flex-col gap-1.5">
        <Select
          id="officeId"
          disabled={officesLoading}
          defaultValue=""
          label={t('auth:officeLabel')}
          {...register('officeId')}
        >
          <option value="" disabled>
            {t('auth:officePlaceholder')}
          </option>
          {offices?.map((office) => (
            <option key={office.id} value={office.id}>
              {office.city ? `${office.name} — ${office.city}` : office.name}
            </option>
          ))}
        </Select>
        {errors.officeId && (
          <p className="text-sm text-danger">{t(errors.officeId.message ?? '')}</p>
        )}
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" variant="primary" disabled={isSubmitting}>
        {t('auth:createAccount')}
      </Button>
    </form>
  )
}
