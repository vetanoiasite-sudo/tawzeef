import { useTranslation } from 'react-i18next'
import type { EmployerProfile } from '../types/employerProfile.types'
import { env } from '@/shared/config/env'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'

/** The read-only "company profile" view every edit used to skip straight past (confirmed
 * 2026-11-25 user report — same gap as the job seeker's own profile). This is what opens by
 * default; "تعديل" is the one way into the actual edit form. */
export function EmployerProfileSummaryCard({
  profile,
  onEdit,
}: {
  profile: EmployerProfile
  onEdit: () => void
}) {
  const { t } = useTranslation('employerProfile')

  const infoRows: { label: string; value: string | null }[] = [
    { label: t('representativeNameLabel'), value: profile.representativeName },
    { label: t('representativeRoleLabel'), value: profile.representativeRole },
    { label: t('companyEmailLabel'), value: profile.companyEmail },
    { label: t('companyPhoneLabel'), value: profile.companyPhone },
    { label: t('cityLabel'), value: profile.city },
    { label: t('addressLabel'), value: profile.address },
  ]

  return (
    <Card className="flex flex-col gap-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="flex items-center gap-4">
          <div className="flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-200 border border-border-primary bg-surface-muted">
            {profile.logoUrl ? (
              <img
                src={`${env.apiBaseUrl}${profile.logoUrl}`}
                alt={profile.displayName}
                className="size-full object-contain"
              />
            ) : (
              <BuildingIcon />
            )}
          </div>
          <div>
            <h1 className="text-xl font-bold text-content-primary">{profile.displayName}</h1>
            {profile.legalName && profile.legalName !== profile.displayName && (
              <p className="mt-0.5 text-sm text-content-tertiary">{profile.legalName}</p>
            )}
          </div>
        </div>
        <Button variant="secondary" onClick={onEdit}>
          {t('editProfile')}
        </Button>
      </div>

      {profile.businessActivity && (
        <section className="flex flex-col gap-1.5">
          <h2 className="text-xs font-bold tracking-wide text-content-tertiary uppercase">
            {t('businessActivityLabel')}
          </h2>
          <p className="text-sm whitespace-pre-line text-content-primary">{profile.businessActivity}</p>
        </section>
      )}

      <div className="grid grid-cols-1 gap-x-6 gap-y-3 border-t border-border-primary pt-4 sm:grid-cols-2">
        {infoRows.map(
          (row) =>
            row.value && (
              <div key={row.label} className="flex flex-col gap-0.5">
                <span className="text-xs font-medium text-content-tertiary">{row.label}</span>
                <span className="text-sm text-content-primary">{row.value}</span>
              </div>
            ),
        )}
      </div>
    </Card>
  )
}

function BuildingIcon() {
  return (
    <svg width="28" height="28" viewBox="0 0 24 24" fill="none" aria-hidden className="text-content-tertiary">
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
