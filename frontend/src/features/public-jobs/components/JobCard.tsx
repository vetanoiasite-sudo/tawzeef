import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import type { JobListItem } from '../types/publicJobs.types'

export function JobCard({ job }: { job: JobListItem }) {
  const { t } = useTranslation('publicJobs')

  return (
    <Link
      to={`/jobs/${job.id}`}
      className="flex flex-col gap-2 rounded-md border border-surface-muted p-4 transition hover:border-primary"
    >
      <h3 className="font-semibold text-text">{job.title}</h3>
      <p className="text-sm text-text-muted">{job.employerDisplayName}</p>
      <div className="flex flex-wrap gap-2 text-xs text-text-muted">
        {job.city && <span className="rounded-full bg-surface-muted px-2 py-0.5">{job.city}</span>}
        {job.category && <span className="rounded-full bg-surface-muted px-2 py-0.5">{job.category}</span>}
        {job.employmentType && (
          <span className="rounded-full bg-surface-muted px-2 py-0.5">{t(`employmentType.${job.employmentType}`)}</span>
        )}
      </div>
      {job.experienceRequirement && <p className="text-xs text-text-muted">{job.experienceRequirement}</p>}
    </Link>
  )
}
