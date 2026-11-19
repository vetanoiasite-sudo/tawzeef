import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useSuggestionQueue } from '../api/officeSuggestionsReview.queries'
import type { SuggestionStatusCode } from '../types/officeSuggestionsReview.types'

const STATUS_FILTERS: (SuggestionStatusCode | undefined)[] = [undefined, 'New', 'UnderReview', 'Responded']
const PAGE_SIZE = 20

export default function SuggestionQueuePage() {
  const { t } = useTranslation('officeSuggestionsReview')
  const [status, setStatus] = useState<SuggestionStatusCode | undefined>(undefined)
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useSuggestionQueue({ status, pageIndex, pageSize: PAGE_SIZE })

  function handleFilterChange(next: SuggestionStatusCode | undefined) {
    setStatus(next)
    setPageIndex(0)
  }

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-10">
      <h1 className="text-lg font-semibold text-text">{t('queueTitle')}</h1>

      <div className="flex flex-wrap gap-2">
        {STATUS_FILTERS.map((s) => (
          <button
            key={s ?? 'all'}
            type="button"
            onClick={() => handleFilterChange(s)}
            className={
              status === s
                ? 'rounded-full bg-primary px-3 py-1 text-sm font-medium text-surface'
                : 'rounded-full border border-surface-muted px-3 py-1 text-sm text-text'
            }
          >
            {s ? t(`status.${s}`) : t('filterAll')}
          </button>
        ))}
      </div>

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && <p className="text-sm text-text-muted">{t('emptyQueue')}</p>}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((row) => (
            <Link
              key={row.id}
              to={`/office/suggestions/${row.id}`}
              className="flex flex-col gap-1 rounded-md border border-surface-muted p-4 transition hover:border-primary"
            >
              <div className="flex items-center justify-between">
                <span className="text-sm text-text-muted">{t(`status.${row.status}`)}</span>
                <span className="text-xs text-text-muted">{new Date(row.creationDate).toLocaleDateString('ar')}</span>
              </div>
              <p className="text-sm text-text">{row.content}</p>
            </Link>
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
