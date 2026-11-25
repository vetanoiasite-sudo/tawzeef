import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useRef } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import {
  officeProfileFormSchema,
  type OfficeProfileFormValues,
} from '../schemas/officeProfile.schema'
import type { OfficeProfile } from '../types/officeProfile.types'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

export function OfficeProfileCompletionForm({
  office,
  isSubmitting,
  submitError,
  onSubmit,
}: {
  office: OfficeProfile
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: OfficeProfileFormValues) => void
}) {
  const { t } = useTranslation(['officeProfile', 'validation'])
  const {
    register,
    handleSubmit,
    getValues,
    reset,
    formState: { errors, isDirty },
  } = useForm<OfficeProfileFormValues>({
    resolver: zodResolver(officeProfileFormSchema),
    // Full-replace PUT semantics on the backend — every field must be pre-populated with the
    // current value, or an unsent field gets overwritten with null.
    defaultValues: {
      name: office.name,
      arName: office.arName ?? '',
      address: office.address ?? '',
      city: office.city ?? '',
      phone: office.phone ?? '',
      email: office.email ?? '',
    },
  })

  // Save button disabled until a real edit happens, then re-disabled once the save lands
  // (2026-11-25 product decision, applied project-wide to every profile edit form).
  const wasSubmittingRef = useRef(isSubmitting)
  useEffect(() => {
    if (wasSubmittingRef.current && !isSubmitting && !submitError) {
      reset(getValues())
    }
    wasSubmittingRef.current = isSubmitting
  }, [isSubmitting, submitError, reset, getValues])

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <TextInput
          id="name"
          type="text"
          label={t('officeProfile:nameLabel')}
          error={errors.name ? t(errors.name.message ?? '') : undefined}
          {...register('name')}
        />

        <TextInput
          id="arName"
          type="text"
          label={t('officeProfile:arNameLabel')}
          error={errors.arName ? t(errors.arName.message ?? '') : undefined}
          {...register('arName')}
        />

        <TextInput
          id="city"
          type="text"
          label={t('officeProfile:cityLabel')}
          error={errors.city ? t(errors.city.message ?? '') : undefined}
          {...register('city')}
        />

        <TextInput
          id="phone"
          type="tel"
          label={t('officeProfile:phoneLabel')}
          error={errors.phone ? t(errors.phone.message ?? '') : undefined}
          {...register('phone')}
        />

        <div className="sm:col-span-2">
          <TextInput
            id="address"
            type="text"
            label={t('officeProfile:addressLabel')}
            error={errors.address ? t(errors.address.message ?? '') : undefined}
            {...register('address')}
          />
        </div>

        <div className="sm:col-span-2">
          <TextInput
            id="email"
            type="email"
            label={t('officeProfile:emailLabel')}
            error={errors.email ? t(errors.email.message ?? '') : undefined}
            {...register('email')}
          />
        </div>
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" disabled={isSubmitting || !isDirty} className="self-start">
        {t('officeProfile:save')}
      </Button>
    </form>
  )
}
