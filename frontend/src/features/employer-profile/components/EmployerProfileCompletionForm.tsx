import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import {
  employerProfileFormSchema,
  type EmployerProfileFormValues,
} from '../schemas/employerProfile.schema'
import type { EmployerProfile } from '../types/employerProfile.types'
import { TextInput } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'
import { useGovernorates } from '@/features/governorates'

export function EmployerProfileCompletionForm({
  profile,
  isSubmitting,
  submitError,
  onSubmit,
}: {
  profile: EmployerProfile
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: EmployerProfileFormValues) => void
}) {
  const { t } = useTranslation(['employerProfile', 'validation'])
  const { data: governorates } = useGovernorates()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<EmployerProfileFormValues>({
    resolver: zodResolver(employerProfileFormSchema),
    // Full-replace PUT semantics on the backend — every field must be pre-populated with the
    // current value, or an unsent field gets overwritten with null (e.g. representativeName, also
    // set at registration time).
    defaultValues: {
      businessActivity: profile.businessActivity ?? '',
      address: profile.address ?? '',
      city: profile.city ?? '',
      companyEmail: profile.companyEmail ?? '',
      companyPhone: profile.companyPhone ?? '',
      representativeName: profile.representativeName ?? '',
      representativeRole: profile.representativeRole ?? '',
    },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <TextInput
          id="representativeName"
          type="text"
          label={t('employerProfile:representativeNameLabel')}
          error={errors.representativeName ? t(errors.representativeName.message ?? '') : undefined}
          {...register('representativeName')}
        />

        <TextInput
          id="representativeRole"
          type="text"
          label={t('employerProfile:representativeRoleLabel')}
          error={errors.representativeRole ? t(errors.representativeRole.message ?? '') : undefined}
          {...register('representativeRole')}
        />

        <div className="sm:col-span-2">
          <TextInput
            id="businessActivity"
            type="text"
            label={t('employerProfile:businessActivityLabel')}
            error={errors.businessActivity ? t(errors.businessActivity.message ?? '') : undefined}
            {...register('businessActivity')}
          />
        </div>

        <Select
          id="city"
          label={t('employerProfile:cityLabel')}
          error={errors.city ? t(errors.city.message ?? '') : undefined}
          {...register('city')}
        >
          <option value="">—</option>
          {(governorates ?? []).map((g) => (
            <option key={g.id} value={g.name}>
              {g.name}
            </option>
          ))}
        </Select>

        <TextInput
          id="companyPhone"
          type="tel"
          label={t('employerProfile:companyPhoneLabel')}
          error={errors.companyPhone ? t(errors.companyPhone.message ?? '') : undefined}
          {...register('companyPhone')}
        />

        <div className="sm:col-span-2">
          <TextInput
            id="address"
            type="text"
            label={t('employerProfile:addressLabel')}
            error={errors.address ? t(errors.address.message ?? '') : undefined}
            {...register('address')}
          />
        </div>

        <div className="sm:col-span-2">
          <TextInput
            id="companyEmail"
            type="email"
            label={t('employerProfile:companyEmailLabel')}
            error={errors.companyEmail ? t(errors.companyEmail.message ?? '') : undefined}
            {...register('companyEmail')}
          />
        </div>
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" disabled={isSubmitting} className="self-start">
        {t('employerProfile:save')}
      </Button>
    </form>
  )
}
