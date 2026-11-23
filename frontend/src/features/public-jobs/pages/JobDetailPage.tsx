import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { useApplyToJob } from '@/features/job-seeker-applications'
import { useJobDetails } from '../api/publicJobs.queries'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { LoadingState } from '@/shared/ui/LoadingState'
import { EmptyState } from '@/shared/ui/EmptyState'

export default function JobDetailPage() {
  const { t } = useTranslation('publicJobs')
  const { id } = useParams<{ id: string }>()
  const jobId = id ?? ''
  const user = useSessionStore((s) => s.user)
  const [applied, setApplied] = useState(false)

  const { data: job, isLoading } = useJobDetails(jobId)
  const apply = useApplyToJob()

  if (isLoading) return <LoadingState />
  if (!job)
    return (
      <div className="px-4 py-8 sm:px-6">
        <EmptyState message={t('notFound')} />
      </div>
    )

  const applyError = apply.error instanceof ApiError ? apply.error.messages[0] : undefined

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-5 px-4 py-8 sm:px-6">
      <Link to="/jobs" className="text-sm font-medium text-primary hover:underline">
        {t('backToList')}
      </Link>

      <Card className="flex flex-col gap-5">
        <div className="flex flex-col gap-1">
          <h1 className="text-xl font-bold text-text">{job.title}</h1>
          <p className="text-sm text-text-muted">{job.employerDisplayName}</p>
        </div>

        <div className="flex flex-wrap gap-2">
          {job.city && (
            <span className="rounded-full bg-surface-muted px-2.5 py-1 text-xs font-medium text-text-muted">
              {job.city}
            </span>
          )}
          {job.category && (
            <span className="rounded-full bg-surface-muted px-2.5 py-1 text-xs font-medium text-text-muted">
              {job.category}
            </span>
          )}
          {job.employmentType && (
            <span className="rounded-full bg-surface-muted px-2.5 py-1 text-xs font-medium text-text-muted">
              {t(`employmentType.${job.employmentType}`)}
            </span>
          )}
        </div>

        <section className="grid grid-cols-1 gap-4 border-t border-surface-muted pt-5 text-sm sm:grid-cols-2">
          {job.description && (
            <div className="col-span-1 sm:col-span-2">
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
            <div className="col-span-1 sm:col-span-2">
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

        {job.contactNote && (
          <p className="border-t border-surface-muted pt-5 text-sm text-text-muted">
            {job.contactNote}
          </p>
        )}

        {!user && <p className="text-sm text-text-muted">{t('loginToApply')}</p>}

        {user?.role === 'JobSeeker' && !applied && (
          <div className="flex flex-col gap-2">
            <Button
              variant="primary"
              disabled={apply.isPending}
              onClick={() =>
                apply.mutate(job.id, {
                  onSuccess: () => setApplied(true),
                })
              }
              className="self-start"
            >
              {t('applyAction')}
            </Button>
            {applyError && <p className="text-sm text-danger">{applyError}</p>}
          </div>
        )}

        {user?.role === 'JobSeeker' && applied && (
          <p className="text-sm font-medium text-success">{t('applySuccess')}</p>
        )}
      </Card>
    </div>
  )
}
