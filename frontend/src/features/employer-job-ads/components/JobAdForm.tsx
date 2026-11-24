import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { jobAdFormSchema, type JobAdFormValues } from '../schemas/jobAd.schema'
import type { JobAdDetail } from '../types/employerJobAds.types'
import { useMyEmployerOffices } from '@/features/employer-profile/api/employerProfile.queries'
import { TextInput, TextArea } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'
import { useGovernorates } from '@/features/governorates'

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
  const { data: governorates } = useGovernorates()
  const { data: offices } = useMyEmployerOffices()
  const approvedOffices = (offices ?? []).filter((o) => o.verificationStatus === 'Approved')
  const {
    register,
    handleSubmit,
    setValue,
    getValues,
    formState: { errors },
  } = useForm<z.input<typeof jobAdFormSchema>, unknown, JobAdFormValues>({
    resolver: zodResolver(jobAdFormSchema),
    defaultValues: {
      officeId: ad?.officeId ?? '',
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

  // Auto-select the employer's only Approved office once it loads — on create only, and only
  // while nothing has been chosen yet (never overrides an explicit pick or an existing ad's fixed
  // office).
  useEffect(() => {
    if (ad || approvedOffices.length !== 1) return
    if (getValues('officeId')) return
    setValue('officeId', approvedOffices[0].officeId)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [approvedOffices.length])

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-5">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {!ad && (
          <Select
            id="officeId"
            label={t('employerJobAds:officeLabel', { defaultValue: 'مكتب التوظيف' })}
            className="sm:col-span-2"
            error={errors.officeId ? t(errors.officeId.message ?? '') : undefined}
            {...register('officeId')}
          >
            <option value="" disabled>
              {t('employerJobAds:officePlaceholder', { defaultValue: 'اختر مكتب التوظيف' })}
            </option>
            {approvedOffices.map((office) => (
              <option key={office.officeId} value={office.officeId}>
                {office.city ? `${office.officeName} — ${office.city}` : office.officeName}
              </option>
            ))}
          </Select>
        )}

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

        {/* A governorate dropdown, not free text — the real Governorates table (api/governorates),
         *  so a typed city name here can never silently fail to match a search. */}
        <Select id="city" label={t('employerJobAds:cityLabel')} {...register('city')}>
          <option value="">—</option>
          {(governorates ?? []).map((g) => (
            <option key={g.id} value={g.name}>
              {g.name}
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
