import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { jobSeekerProfileFormSchema, type JobSeekerProfileFormValues } from '../schemas/jobSeekerProfile.schema'
import type { JobSeekerProfile } from '../types/jobSeekerProfile.types'
import { TextInput, TextArea } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

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
        <TextInput
          id="email"
          type="email"
          label={t('jobSeekerProfile:emailLabel')}
          error={errors.email ? t(errors.email.message ?? '') : undefined}
          {...register('email')}
        />

        <TextInput
          id="city"
          type="text"
          label={t('jobSeekerProfile:cityLabel')}
          error={errors.city ? t(errors.city.message ?? '') : undefined}
          {...register('city')}
        />

        <div className="sm:col-span-2">
          <TextInput
            id="address"
            type="text"
            label={t('jobSeekerProfile:addressLabel')}
            error={errors.address ? t(errors.address.message ?? '') : undefined}
            {...register('address')}
          />
        </div>

        <TextInput id="dateOfBirth" type="date" label={t('jobSeekerProfile:dateOfBirthLabel')} {...register('dateOfBirth')} />

        <TextInput
          id="yearsOfExperience"
          type="number"
          min={0}
          max={80}
          label={t('jobSeekerProfile:yearsOfExperienceLabel')}
          error={errors.yearsOfExperience ? t(errors.yearsOfExperience.message ?? '') : undefined}
          {...register('yearsOfExperience')}
        />

        <div className="sm:col-span-2">
          <TextInput
            id="education"
            type="text"
            label={t('jobSeekerProfile:educationLabel')}
            error={errors.education ? t(errors.education.message ?? '') : undefined}
            {...register('education')}
          />
        </div>

        <div className="sm:col-span-2">
          <TextArea
            id="skills"
            rows={2}
            label={t('jobSeekerProfile:skillsLabel')}
            error={errors.skills ? t(errors.skills.message ?? '') : undefined}
            {...register('skills')}
          />
        </div>

        <div className="sm:col-span-2">
          <TextArea
            id="careerSummary"
            rows={3}
            label={t('jobSeekerProfile:careerSummaryLabel')}
            error={errors.careerSummary ? t(errors.careerSummary.message ?? '') : undefined}
            {...register('careerSummary')}
          />
        </div>
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <Button type="submit" disabled={isSubmitting} className="self-start">
        {t('jobSeekerProfile:save')}
      </Button>
    </form>
  )
}
