import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { jobAdFormSchema, type JobAdFormValues } from '../schemas/jobAd.schema'
import type { JobAdDetail } from '../types/employerJobAds.types'

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
  } = useForm<JobAdFormValues>({
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
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="title" className="text-sm font-medium text-text">
            {t('employerJobAds:titleLabel')}
          </label>
          <input
            id="title"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('title')}
          />
          {errors.title && <p className="text-sm text-danger">{t(errors.title.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="description" className="text-sm font-medium text-text">
            {t('employerJobAds:descriptionLabel')}
          </label>
          <textarea
            id="description"
            rows={4}
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('description')}
          />
          {errors.description && <p className="text-sm text-danger">{t(errors.description.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="category" className="text-sm font-medium text-text">
            {t('employerJobAds:categoryLabel')}
          </label>
          <input
            id="category"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('category')}
          />
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="city" className="text-sm font-medium text-text">
            {t('employerJobAds:cityLabel')}
          </label>
          <input
            id="city"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('city')}
          />
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="employmentType" className="text-sm font-medium text-text">
            {t('employerJobAds:employmentTypeLabel')}
          </label>
          <select
            id="employmentType"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('employmentType')}
          >
            <option value="">—</option>
            <option value="FullTime">{t('employerJobAds:employmentType.FullTime')}</option>
            <option value="PartTime">{t('employerJobAds:employmentType.PartTime')}</option>
            <option value="Contract">{t('employerJobAds:employmentType.Contract')}</option>
          </select>
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="experienceRequirement" className="text-sm font-medium text-text">
            {t('employerJobAds:experienceRequirementLabel')}
          </label>
          <input
            id="experienceRequirement"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('experienceRequirement')}
          />
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="workingHours" className="text-sm font-medium text-text">
            {t('employerJobAds:workingHoursLabel')}
          </label>
          <input
            id="workingHours"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('workingHours')}
          />
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="address" className="text-sm font-medium text-text">
            {t('employerJobAds:addressLabel')}
          </label>
          <input
            id="address"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('address')}
          />
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="salaryMin" className="text-sm font-medium text-text">
            {t('employerJobAds:salaryMinLabel')}
          </label>
          <input
            id="salaryMin"
            type="number"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('salaryMin')}
          />
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="salaryMax" className="text-sm font-medium text-text">
            {t('employerJobAds:salaryMaxLabel')}
          </label>
          <input
            id="salaryMax"
            type="number"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('salaryMax')}
          />
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="vacancyCount" className="text-sm font-medium text-text">
            {t('employerJobAds:vacancyCountLabel')}
          </label>
          <input
            id="vacancyCount"
            type="number"
            min={1}
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('vacancyCount')}
          />
          {errors.vacancyCount && <p className="text-sm text-danger">{t(errors.vacancyCount.message ?? '')}</p>}
        </div>
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
      >
        {t('employerJobAds:save')}
      </button>
    </form>
  )
}
