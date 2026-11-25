import { useTranslation } from 'react-i18next'
import type { OfficeProfile } from '../types/officeProfile.types'
import { env } from '@/shared/config/env'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'

/** The read-only "office profile" view (2026-11-25 product decision) — shown to the office's own
 * staff by default (with "تعديل" the one way into the edit form, same pattern as every other
 * profile in this app), and reused as-is for the public-facing view (minus the Edit button and
 * Status tag, passed in as `onEdit: undefined`). */
export function OfficeProfileSummaryCard({
  office,
  onEdit,
}: {
  office: OfficeProfile
  onEdit?: () => void
}) {
  const { t } = useTranslation('officeProfile')

  const infoRows: { label: string; value: string | null }[] = [
    { label: t('cityLabel'), value: office.city },
    { label: t('addressLabel'), value: office.address },
    { label: t('phoneLabel'), value: office.phone },
    { label: t('emailLabel'), value: office.email },
  ]

  return (
    <Card className="flex flex-col gap-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="flex items-center gap-4">
          <div className="flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-200 border border-border-primary bg-surface-muted">
            {office.logoUrl ? (
              <img
                src={`${env.apiBaseUrl}${office.logoUrl}`}
                alt={office.arName ?? office.name}
                className="size-full object-contain"
              />
            ) : (
              <BuildingIcon />
            )}
          </div>
          <div>
            <h1 className="text-xl font-bold text-content-primary">{office.arName ?? office.name}</h1>
            {office.arName && office.name !== office.arName && (
              <p className="mt-0.5 text-sm text-content-tertiary">{office.name}</p>
            )}
          </div>
        </div>
        <div className="flex items-center gap-2">
          {onEdit && (
            <StatusTag
              domain="office"
              value={office.status}
              label={t(`status.${office.status}`)}
            />
          )}
          {onEdit && (
            <Button variant="secondary" onClick={onEdit}>
              {t('editProfile')}
            </Button>
          )}
        </div>
      </div>

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
