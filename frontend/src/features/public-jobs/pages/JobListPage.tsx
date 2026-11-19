import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { JobCard } from '../components/JobCard'
import { useJobSearch } from '../api/publicJobs.queries'
import type { EmploymentTypeCode } from '../types/publicJobs.types'

const PAGE_SIZE = 20
const EMPLOYMENT_TYPES: EmploymentTypeCode[] = ['FullTime', 'PartTime', 'Contract']

export default function JobListPage() {
  const { t } = useTranslation('publicJobs')
  const [keyword, setKeyword] = useState('')
  const [city, setCity] = useState('')
  const [category, setCategory] = useState('')
  const [employmentType, setEmploymentType] = useState<EmploymentTypeCode | ''>('')
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useJobSearch({
    keyword: keyword.trim() || undefined,
    city: city.trim() || undefined,
    category: category.trim() || undefined,
    employmentType: employmentType || undefined,
    pageIndex,
    pageSize: PAGE_SIZE,
  })

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-10">
      <h1 className="text-lg font-semibold text-text">{t('pageTitle')}</h1>

      <div className="flex flex-wrap gap-2">
        <input
          value={keyword}
          onChange={(e) => {
            setKeyword(e.target.value)
            setPageIndex(0)
          }}
          placeholder={t('searchPlaceholder')}
          className="flex-1 min-w-[180px] rounded-md border border-surface-muted px-3 py-2 text-sm text-text outline-none focus:border-primary"
        />
        <input
          value={city}
          onChange={(e) => {
            setCity(e.target.value)
            setPageIndex(0)
          }}
          placeholder={t('cityPlaceholder')}
          className="min-w-[140px] rounded-md border border-surface-muted px-3 py-2 text-sm text-text outline-none focus:border-primary"
        />
        <input
          value={category}
          onChange={(e) => {
            setCategory(e.target.value)
            setPageIndex(0)
          }}
          placeholder={t('categoryPlaceholder')}
          className="min-w-[140px] rounded-md border border-surface-muted px-3 py-2 text-sm text-text outline-none focus:border-primary"
        />
        <select
          value={employmentType}
          onChange={(e) => {
            setEmploymentType(e.target.value as EmploymentTypeCode | '')
            setPageIndex(0)
          }}
          className="rounded-md border border-surface-muted px-3 py-2 text-sm text-text outline-none focus:border-primary"
        >
          <option value="">{t('employmentTypeAll')}</option>
          {EMPLOYMENT_TYPES.map((et) => (
            <option key={et} value={et}>
              {t(`employmentType.${et}`)}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && (
        <p className="text-sm text-text-muted">{t('emptyResults')}</p>
      )}

      {!isLoading && data && data.items.length > 0 && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {data.items.map((job) => (
            <JobCard key={job.id} job={job} />
          ))}
        </div>
      )}

      {data && data.pagination.totalPages > 1 && (
        <div className="flex items-center justify-center gap-4">
          <button
            type="button"
            disabled={pageIndex === 0}
            onClick={() => setPageIndex((p) => p - 1)}
            className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text disabled:opacity-60"
          >
            {t('prevPage')}
          </button>
          <span className="text-sm text-text-muted">
            {t('pageOf', { current: pageIndex + 1, total: data.pagination.totalPages })}
          </span>
          <button
            type="button"
            disabled={pageIndex + 1 >= data.pagination.totalPages}
            onClick={() => setPageIndex((p) => p + 1)}
            className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text disabled:opacity-60"
          >
            {t('nextPage')}
          </button>
        </div>
      )}
    </div>
  )
}
