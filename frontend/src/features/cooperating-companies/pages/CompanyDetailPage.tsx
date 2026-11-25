import { useParams } from 'react-router-dom'
import { useCooperatingCompany } from '../api/cooperatingCompanies.queries'
import { env } from '@/shared/config/env'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { LoadingState } from '@/shared/ui/LoadingState'
import { EmptyState } from '@/shared/ui/EmptyState'

/** The public "view this company's profile" page (2026-11-25) — logo/name/city/business
 * activity only. Deliberately never contact info (phone/email) or any verification document —
 * those stay office-only, confirmed when this feature was built. */
export default function CompanyDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: company, isLoading } = useCooperatingCompany(id ?? '')

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title="ملف الشركة" />

      {isLoading && <LoadingState />}
      {!isLoading && !company && <EmptyState message="الشركة غير موجودة" />}

      {!isLoading && company && (
        <Card className="flex flex-col gap-6">
          <div className="flex items-center gap-4">
            <div className="flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-200 border border-border-primary bg-surface-muted">
              {company.logoUrl ? (
                <img
                  src={`${env.apiBaseUrl}${company.logoUrl}`}
                  alt={company.displayName}
                  className="size-full object-contain"
                />
              ) : (
                <BuildingIcon />
              )}
            </div>
            <div>
              <h1 className="text-xl font-bold text-content-primary">{company.displayName}</h1>
              {company.city && <p className="mt-0.5 text-sm text-content-tertiary">{company.city}</p>}
            </div>
          </div>

          {company.businessActivity && (
            <section className="flex flex-col gap-1.5 border-t border-border-primary pt-4">
              <h2 className="text-xs font-bold tracking-wide text-content-tertiary uppercase">
                النشاط التجاري
              </h2>
              <p className="text-sm whitespace-pre-line text-content-primary">
                {company.businessActivity}
              </p>
            </section>
          )}
        </Card>
      )}
    </div>
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
