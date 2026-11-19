import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { employerProfileFormSchema, type EmployerProfileFormValues } from '../schemas/employerProfile.schema'
import type { EmployerProfile } from '../types/employerProfile.types'

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
        <div className="flex flex-col gap-1">
          <label htmlFor="representativeName" className="text-sm font-medium text-text">
            {t('employerProfile:representativeNameLabel')}
          </label>
          <input
            id="representativeName"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('representativeName')}
          />
          {errors.representativeName && (
            <p className="text-sm text-danger">{t(errors.representativeName.message ?? '')}</p>
          )}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="representativeRole" className="text-sm font-medium text-text">
            {t('employerProfile:representativeRoleLabel')}
          </label>
          <input
            id="representativeRole"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('representativeRole')}
          />
          {errors.representativeRole && (
            <p className="text-sm text-danger">{t(errors.representativeRole.message ?? '')}</p>
          )}
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="businessActivity" className="text-sm font-medium text-text">
            {t('employerProfile:businessActivityLabel')}
          </label>
          <input
            id="businessActivity"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('businessActivity')}
          />
          {errors.businessActivity && (
            <p className="text-sm text-danger">{t(errors.businessActivity.message ?? '')}</p>
          )}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="city" className="text-sm font-medium text-text">
            {t('employerProfile:cityLabel')}
          </label>
          <input
            id="city"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('city')}
          />
          {errors.city && <p className="text-sm text-danger">{t(errors.city.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="companyPhone" className="text-sm font-medium text-text">
            {t('employerProfile:companyPhoneLabel')}
          </label>
          <input
            id="companyPhone"
            type="tel"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('companyPhone')}
          />
          {errors.companyPhone && <p className="text-sm text-danger">{t(errors.companyPhone.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="address" className="text-sm font-medium text-text">
            {t('employerProfile:addressLabel')}
          </label>
          <input
            id="address"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('address')}
          />
          {errors.address && <p className="text-sm text-danger">{t(errors.address.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="companyEmail" className="text-sm font-medium text-text">
            {t('employerProfile:companyEmailLabel')}
          </label>
          <input
            id="companyEmail"
            type="email"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('companyEmail')}
          />
          {errors.companyEmail && <p className="text-sm text-danger">{t(errors.companyEmail.message ?? '')}</p>}
        </div>
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
      >
        {t('employerProfile:save')}
      </button>
    </form>
  )
}
