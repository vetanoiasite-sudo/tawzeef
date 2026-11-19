import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useComplaintQueue } from '../api/officeComplaintReview.queries'
import type { ComplaintStatusCode } from '../types/officeComplaintReview.types'

const STATUS_FILTERS: (ComplaintStatusCode | undefined)[] = [
  undefined,
  'New',
  'UnderReview',
  'Resolved',
  'Rejected',
]

const PAGE_SIZE = 20

export default function ComplaintQueuePage() {
  const { t } = useTranslation('officeComplaintReview')
  const [status, setStatus] = useState<ComplaintStatusCode | undefined>(undefined)
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useComplaintQueue({ status, pageIndex, pageSize: PAGE_SIZE })

  function handleFilterChange(next: ComplaintStatusCode | undefined) {
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
        <div className="overflow-x-auto rounded-md border border-surface-muted">
          <table className="w-full text-start text-sm">
            <thead className="bg-surface-muted text-text-muted">
              <tr>
                <th className="px-3 py-2 text-start">{t('columnSubject')}</th>
                <th className="px-3 py-2 text-start">{t('columnStatus')}</th>
                <th className="px-3 py-2 text-start">{t('columnValidity')}</th>
                <th className="px-3 py-2 text-start">{t('columnDate')}</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((row) => (
                <tr key={row.id} className="border-t border-surface-muted">
                  <td className="px-3 py-2 text-text">{row.subjectName}</td>
                  <td className="px-3 py-2 text-text">{t(`status.${row.status}`)}</td>
                  <td className="px-3 py-2 text-text">{t(`validity.${row.validityDecision}`)}</td>
                  <td className="px-3 py-2 text-text-muted">{new Date(row.createdAt).toLocaleDateString('ar')}</td>
                  <td className="px-3 py-2">
                    <Link to={`/office/complaints/${row.id}`} className="text-primary">
                      {t('viewAction')}
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
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
