import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { Card } from '@/shared/ui/Card'
import { env } from '@/shared/config/env'
import type { JobListItem } from '../types/publicJobs.types'

/** UI_UX_GUIDELINES.md §4.2 anatomy — more breathing room than Wuzzuf's dense card, per the
 * "trust before volume" principle (§1.1). */
export function JobCard({ job }: { job: JobListItem }) {
  const { t } = useTranslation('publicJobs')

  return (
    <Link to={`/jobs/${job.id}`} className="block">
      <Card className="flex flex-col gap-3 transition-colors duration-150 hover:border-primary">
        <div className="flex items-start gap-3">
          <div className="flex size-11 shrink-0 items-center justify-center overflow-hidden rounded-200 border border-border-primary bg-surface-muted">
            {job.employerLogoUrl ? (
              <img
                src={`${env.apiBaseUrl}${job.employerLogoUrl}`}
                alt={job.employerDisplayName}
                className="size-full object-contain"
              />
            ) : (
              <BuildingPlaceholderIcon />
            )}
          </div>
          <div className="flex flex-col gap-1">
            <h3 className="text-lg font-bold text-text">{job.title}</h3>
            <p className="text-sm text-text-muted">
              {job.employerDisplayName}
              {job.city && <span> · {job.city}</span>}
            </p>
          </div>
        </div>
        <div className="flex flex-wrap gap-2">
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
          {job.experienceRequirement && (
            <span className="rounded-full bg-surface-muted px-2.5 py-1 text-xs font-medium text-text-muted">
              {job.experienceRequirement}
            </span>
          )}
        </div>
        <p className="text-xs font-medium text-brand">
          {t('officeLabel', { defaultValue: 'مكتب التوظيف' })}: {job.officeName}
        </p>

        {job.publishedAt && (
          <p className="text-xs text-text-muted">
            {t('postedOn')} {new Date(job.publishedAt).toLocaleDateString('ar')}
          </p>
        )}
      </Card>
    </Link>
  )
}

function BuildingPlaceholderIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden className="text-text-muted">
      <rect x="4" y="3" width="16" height="18" rx="1" stroke="currentColor" strokeWidth="1.6" />
      <path
        d="M8 8h2M14 8h2M8 12h2M14 12h2M8 16h2M14 16h2"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
      />
    </svg>
  )
}
