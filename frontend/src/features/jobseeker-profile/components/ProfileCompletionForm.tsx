import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { jobSeekerProfileFormSchema, type JobSeekerProfileFormValues } from '../schemas/jobSeekerProfile.schema'
import type { JobSeekerProfile } from '../types/jobSeekerProfile.types'

export function ProfileCompletionForm({
  profile,
  isSubmitting,
  submitError,
  onSubmit,
}: {
  profile: JobSeekerProfile
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: JobSeekerProfileFormValues) => void
}) {
  const { t } = useTranslation(['jobSeekerProfile', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<JobSeekerProfileFormValues>({
    resolver: zodResolver(jobSeekerProfileFormSchema),
    defaultValues: {
      email: profile.email ?? '',
      address: profile.address ?? '',
      city: profile.city ?? '',
      dateOfBirth: profile.dateOfBirth ?? '',
      education: profile.education ?? '',
      yearsOfExperience: profile.yearsOfExperience?.toString() ?? '',
      skills: profile.skills ?? '',
      careerSummary: profile.careerSummary ?? '',
    },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="flex flex-col gap-1">
          <label htmlFor="email" className="text-sm font-medium text-text">
            {t('jobSeekerProfile:emailLabel')}
          </label>
          <input
            id="email"
            type="email"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('email')}
          />
          {errors.email && <p className="text-sm text-danger">{t(errors.email.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="city" className="text-sm font-medium text-text">
            {t('jobSeekerProfile:cityLabel')}
          </label>
          <input
            id="city"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('city')}
          />
          {errors.city && <p className="text-sm text-danger">{t(errors.city.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="address" className="text-sm font-medium text-text">
            {t('jobSeekerProfile:addressLabel')}
          </label>
          <input
            id="address"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('address')}
          />
          {errors.address && <p className="text-sm text-danger">{t(errors.address.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="dateOfBirth" className="text-sm font-medium text-text">
            {t('jobSeekerProfile:dateOfBirthLabel')}
          </label>
          <input
            id="dateOfBirth"
            type="date"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('dateOfBirth')}
          />
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="yearsOfExperience" className="text-sm font-medium text-text">
            {t('jobSeekerProfile:yearsOfExperienceLabel')}
          </label>
          <input
            id="yearsOfExperience"
            type="number"
            min={0}
            max={80}
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('yearsOfExperience')}
          />
          {errors.yearsOfExperience && (
            <p className="text-sm text-danger">{t(errors.yearsOfExperience.message ?? '')}</p>
          )}
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="education" className="text-sm font-medium text-text">
            {t('jobSeekerProfile:educationLabel')}
          </label>
          <input
            id="education"
            type="text"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('education')}
          />
          {errors.education && <p className="text-sm text-danger">{t(errors.education.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="skills" className="text-sm font-medium text-text">
            {t('jobSeekerProfile:skillsLabel')}
          </label>
          <textarea
            id="skills"
            rows={2}
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('skills')}
          />
          {errors.skills && <p className="text-sm text-danger">{t(errors.skills.message ?? '')}</p>}
        </div>

        <div className="flex flex-col gap-1 sm:col-span-2">
          <label htmlFor="careerSummary" className="text-sm font-medium text-text">
            {t('jobSeekerProfile:careerSummaryLabel')}
          </label>
          <textarea
            id="careerSummary"
            rows={3}
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('careerSummary')}
          />
          {errors.careerSummary && (
            <p className="text-sm text-danger">{t(errors.careerSummary.message ?? '')}</p>
          )}
        </div>
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
      >
        {t('jobSeekerProfile:save')}
      </button>
    </form>
  )
}
