import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import {
  jobSeekerProfileFormSchema,
  type JobSeekerProfileFormValues,
} from '../schemas/jobSeekerProfile.schema'
import type { JobSeekerProfile } from '../types/jobSeekerProfile.types'
import { TextInput, TextArea } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'
import { useGovernorates } from '@/features/governorates'

export function ProfileCompletionForm({
  profile,
  isSubmitting,
  submitError,
  cvSuggestions,
  onSubmit,
}: {
  profile: JobSeekerProfile
  isSubmitting: boolean
  submitError?: string
  /** Right after a Cv upload returns suggestions — pre-fills jobTitle/yearsOfExperience only when
   * those fields are currently empty, and shows a hint the job seeker can review/edit before
   * saving. Never written to the server by this component itself. */
  cvSuggestions?: { jobTitle?: string | null; yearsOfExperience?: number | null }
  onSubmit: (values: JobSeekerProfileFormValues) => void
}) {
  const { t } = useTranslation(['jobSeekerProfile', 'validation'])
  const { data: governorates } = useGovernorates()
  const {
    register,
    handleSubmit,
    setValue,
    getValues,
    reset,
    formState: { errors, isDirty },
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
      jobTitle: profile.jobTitle ?? '',
    },
  })

  const [suggestedFields, setSuggestedFields] = useState<Set<'jobTitle' | 'yearsOfExperience'>>(new Set())

  useEffect(() => {
    if (!cvSuggestions) return
    const nextSuggested = new Set<'jobTitle' | 'yearsOfExperience'>()

    if (cvSuggestions.jobTitle && getValues('jobTitle') === '') {
      setValue('jobTitle', cvSuggestions.jobTitle)
      nextSuggested.add('jobTitle')
    }
    if (cvSuggestions.yearsOfExperience != null && getValues('yearsOfExperience') === '') {
      setValue('yearsOfExperience', String(cvSuggestions.yearsOfExperience))
      nextSuggested.add('yearsOfExperience')
    }

    if (nextSuggested.size > 0) setSuggestedFields(nextSuggested)
    // Only react to a new cvSuggestions object identity (a fresh upload) — not to getValues/setValue.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [cvSuggestions])

  // Re-baseline the form once a save actually lands, so the Save button goes back to disabled
  // until the next real edit — isSubmitting going true→false with no error is the only signal
  // available here that the save succeeded (the mutation itself lives in the parent page).
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
          id="email"
          type="email"
          label={t('jobSeekerProfile:emailLabel')}
          error={errors.email ? t(errors.email.message ?? '') : undefined}
          {...register('email')}
        />

        <Select
          id="city"
          label={t('jobSeekerProfile:cityLabel')}
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

        <div className="sm:col-span-2">
          <TextInput
            id="address"
            type="text"
            label={t('jobSeekerProfile:addressLabel')}
            error={errors.address ? t(errors.address.message ?? '') : undefined}
            {...register('address')}
          />
        </div>

        <TextInput
          id="dateOfBirth"
          type="date"
          label={t('jobSeekerProfile:dateOfBirthLabel')}
          {...register('dateOfBirth')}
        />

        <div>
          <TextInput
            id="yearsOfExperience"
            type="number"
            min={0}
            max={80}
            label={t('jobSeekerProfile:yearsOfExperienceLabel')}
            error={errors.yearsOfExperience ? t(errors.yearsOfExperience.message ?? '') : undefined}
            {...register('yearsOfExperience', {
              onChange: () =>
                setSuggestedFields((prev) => {
                  if (!prev.has('yearsOfExperience')) return prev
                  const next = new Set(prev)
                  next.delete('yearsOfExperience')
                  return next
                }),
            })}
          />
          {suggestedFields.has('yearsOfExperience') && (
            <p className="mt-1 text-xs text-brand">{t('jobSeekerProfile:cvSuggestionHint')}</p>
          )}
        </div>

        <div className="sm:col-span-2">
          <TextInput
            id="jobTitle"
            type="text"
            label={t('jobSeekerProfile:jobTitleLabel')}
            error={errors.jobTitle ? t(errors.jobTitle.message ?? '') : undefined}
            {...register('jobTitle', {
              onChange: () =>
                setSuggestedFields((prev) => {
                  if (!prev.has('jobTitle')) return prev
                  const next = new Set(prev)
                  next.delete('jobTitle')
                  return next
                }),
            })}
          />
          {suggestedFields.has('jobTitle') && (
            <p className="mt-1 text-xs text-brand">{t('jobSeekerProfile:cvSuggestionHint')}</p>
          )}
        </div>

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

      <Button type="submit" disabled={isSubmitting || !isDirty} className="self-start">
        {t('jobSeekerProfile:save')}
      </Button>
    </form>
  )
}
