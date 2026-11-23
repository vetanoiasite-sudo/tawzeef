import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { jobAdFormSchema, type JobAdFormValues } from '../schemas/jobAd.schema'
import type { JobAdDetail } from '../types/employerJobAds.types'
import { TextInput, TextArea } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'
import { EGYPT_GOVERNORATES } from '@/shared/constants/governorates'

export function JobAdForm({
  ad,
  isSubmitting,
  submitError,
  onSubmit,
}: {
  ad?: JobAdDetail
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: JobAdFormValues) => void
}) {
  const { t } = useTranslation(['employerJobAds', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<z.input<typeof jobAdFormSchema>, unknown, JobAdFormValues>({
    resolver: zodResolver(jobAdFormSchema),
    defaultValues: {
      title: ad?.title ?? '',
      description: ad?.description ?? '',
      experienceRequirement: ad?.experienceRequirement ?? '',
      workingHours: ad?.workingHours ?? '',
      address: ad?.address ?? '',
      category: ad?.category ?? '',
      city: ad?.city ?? '',
      employmentType: ad?.employmentType ?? '',
      salaryMin: ad?.salaryMin ?? '',
      salaryMax: ad?.salaryMax ?? '',
      vacancyCount: ad?.vacancyCount ?? 1,
    },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-5">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <TextInput
          id="title"
          label={t('employerJobAds:titleLabel')}
          className="sm:col-span-2"
          error={errors.title ? t(errors.title.message ?? '') : undefined}
          {...register('title')}
        />

        <TextArea
          id="description"
          label={t('employerJobAds:descriptionLabel')}
          rows={4}
          className="sm:col-span-2"
          error={errors.description ? t(errors.description.message ?? '') : undefined}
          {...register('description')}
        />

        <TextInput
          id="category"
          label={t('employerJobAds:categoryLabel')}
          {...register('category')}
        />

        {/* A governorate dropdown, not free text — public search filters by this same fixed
         *  list (frontend/src/shared/constants/governorates.ts), so a typed city name here
         *  would silently never match a search. */}
        <Select id="city" label={t('employerJobAds:cityLabel')} {...register('city')}>
          <option value="">—</option>
          {EGYPT_GOVERNORATES.map((g) => (
            <option key={g} value={g}>
              {g}
            </option>
          ))}
        </Select>

        <Select
          id="employmentType"
          label={t('employerJobAds:employmentTypeLabel')}
          {...register('employmentType')}
        >
          <option value="">—</option>
          <option value="FullTime">{t('employerJobAds:employmentType.FullTime')}</option>
          <option value="PartTime">{t('employerJobAds:employmentType.PartTime')}</option>
          <option value="Contract">{t('employerJobAds:employmentType.Contract')}</option>
        </Select>

        <TextInput
          id="experienceRequirement"
          label={t('employerJobAds:experienceRequirementLabel')}
          {...register('experienceRequirement')}
        />

        <TextInput
          id="workingHours"
          label={t('employerJobAds:workingHoursLabel')}
          {...register('workingHours')}
        />

        <TextInput
          id="address"
          label={t('employerJobAds:addressLabel')}
          className="sm:col-span-2"
          {...register('address')}
        />

        <TextInput
          id="salaryMin"
          type="number"
          label={t('employerJobAds:salaryMinLabel')}
          {...register('salaryMin')}
        />

        <TextInput
          id="salaryMax"
          type="number"
          label={t('employerJobAds:salaryMaxLabel')}
          {...register('salaryMax')}
        />

        <TextInput
          id="vacancyCount"
          type="number"
          min={1}
          label={t('employerJobAds:vacancyCountLabel')}
          error={errors.vacancyCount ? t(errors.vacancyCount.message ?? '') : undefined}
          {...register('vacancyCount')}
        />
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" variant="primary" disabled={isSubmitting} className="self-start">
        {t('employerJobAds:save')}
      </Button>
    </form>
  )
}
