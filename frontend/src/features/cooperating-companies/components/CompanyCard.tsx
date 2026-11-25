import { Link } from 'react-router-dom'
import type { CooperatingCompany } from '../types/cooperatingCompanies.types'
import { env } from '@/shared/config/env'

export function CompanyCard({ company }: { company: CooperatingCompany }) {
  return (
    <Link
      to={`/companies/${company.id}`}
      className="group flex h-28 flex-col items-center justify-center gap-2 rounded-300 border border-border-primary bg-surface p-3 text-center transition-[transform,box-shadow,border-color] duration-200 hover:-translate-y-1 hover:border-brand hover:shadow-200"
    >
      <div className="flex size-10 shrink-0 items-center justify-center overflow-hidden rounded-200 bg-surface-muted">
        {company.logoUrl ? (
          <img
            src={`${env.apiBaseUrl}${company.logoUrl}`}
            alt={company.displayName}
            className="size-full object-contain"
          />
        ) : (
          <BuildingPlaceholderIcon />
        )}
      </div>
      <span className="line-clamp-1 text-[13px] font-bold text-content-primary group-hover:text-brand">
        {company.displayName}
      </span>
    </Link>
  )
}

function BuildingPlaceholderIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden className="text-content-tertiary">
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
