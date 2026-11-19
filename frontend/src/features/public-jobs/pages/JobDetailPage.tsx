import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { useApplyToJob } from '@/features/job-seeker-applications'
import { useJobDetails } from '../api/publicJobs.queries'

export default function JobDetailPage() {
  const { t } = useTranslation('publicJobs')
  const { id } = useParams<{ id: string }>()
  const jobId = id ?? ''
  const user = useSessionStore((s) => s.user)
  const [applied, setApplied] = useState(false)

  const { data: job, isLoading } = useJobDetails(jobId)
  const apply = useApplyToJob()

  if (isLoading) return <div className="mx-auto max-w-2xl px-4 py-10 text-text-muted">...</div>
  if (!job) return <div className="mx-auto max-w-2xl px-4 py-10 text-text-muted">{t('notFound')}</div>

  const applyError = apply.error instanceof ApiError ? apply.error.messages[0] : undefined

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-10">
      <Link to="/jobs" className="text-sm text-primary">
        {t('backToList')}
      </Link>

      <div className="flex flex-col gap-1">
        <h1 className="text-lg font-semibold text-text">{job.title}</h1>
        <p className="text-sm text-text-muted">{job.employerDisplayName}</p>
      </div>

      <div className="flex flex-wrap gap-2 text-xs text-text-muted">
        {job.city && <span className="rounded-full bg-surface-muted px-2 py-0.5">{job.city}</span>}
        {job.category && <span className="rounded-full bg-surface-muted px-2 py-0.5">{job.category}</span>}
        {job.employmentType && (
          <span className="rounded-full bg-surface-muted px-2 py-0.5">{t(`employmentType.${job.employmentType}`)}</span>
        )}
      </div>

      <section className="grid grid-cols-2 gap-4 text-sm">
        {job.description && (
          <div className="col-span-2">
            <p className="text-text-muted">{t('descriptionLabel')}</p>
            <p className="whitespace-pre-line text-text">{job.description}</p>
          </div>
        )}
        {job.experienceRequirement && (
          <div>
            <p className="text-text-muted">{t('experienceLabel')}</p>
            <p className="text-text">{job.experienceRequirement}</p>
          </div>
        )}
        {job.workingHours && (
          <div>
            <p className="text-text-muted">{t('workingHoursLabel')}</p>
            <p className="text-text">{job.workingHours}</p>
          </div>
        )}
        {job.address && (
          <div className="col-span-2">
            <p className="text-text-muted">{t('addressLabel')}</p>
            <p className="text-text">{job.address}</p>
          </div>
        )}
        {(job.salaryMin || job.salaryMax) && (
          <div>
            <p className="text-text-muted">{t('salaryLabel')}</p>
            <p className="text-text">
              {job.salaryMin ?? '—'} - {job.salaryMax ?? '—'}
            </p>
          </div>
        )}
        <div>
          <p className="text-text-muted">{t('vacancyCountLabel')}</p>
          <p className="text-text">{job.vacancyCount}</p>
        </div>
      </section>

      <p className="text-sm text-text-muted">{job.contactNote}</p>

      {!user && <p className="text-sm text-text-muted">{t('loginToApply')}</p>}

      {user?.role === 'JobSeeker' && !applied && (
        <div className="flex flex-col gap-2">
          <button
            type="button"
            disabled={apply.isPending}
            onClick={() =>
              apply.mutate(job.id, {
                onSuccess: () => setApplied(true),
              })
            }
            className="self-start rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
          >
            {t('applyAction')}
          </button>
          {applyError && <p className="text-sm text-danger">{applyError}</p>}
        </div>
      )}

      {user?.role === 'JobSeeker' && applied && (
        <p className="text-sm font-medium text-success">{t('applySuccess')}</p>
      )}
    </div>
  )
}
