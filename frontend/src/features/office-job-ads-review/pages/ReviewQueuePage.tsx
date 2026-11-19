import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useReviewQueue } from '../api/officeJobAdsReview.queries'
import type { JobAdvertisementStatusCode } from '../types/officeJobAdsReview.types'

const PAGE_SIZE = 20
const STATUS_FILTERS: (JobAdvertisementStatusCode | undefined)[] = [
  undefined,
  'SubmittedForReview',
  'UnderReview',
  'ApprovedAwaitingPayment',
  'Published',
  'Paused',
  'Rejected',
  'Closed',
  'Expired',
]

export default function ReviewQueuePage() {
  const { t } = useTranslation('officeJobAdsReview')
  const [status, setStatus] = useState<JobAdvertisementStatusCode | undefined>(undefined)
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useReviewQueue({ status, pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-10">
      <h1 className="text-lg font-semibold text-text">{t('queueTitle')}</h1>

      <div className="flex flex-wrap gap-2">
        {STATUS_FILTERS.map((s) => (
          <button
            key={s ?? 'all'}
            type="button"
            onClick={() => {
              setStatus(s)
              setPageIndex(0)
            }}
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
                <th className="px-3 py-2 text-start">{t('columnTitle')}</th>
                <th className="px-3 py-2 text-start">{t('columnEmployer')}</th>
                <th className="px-3 py-2 text-start">{t('columnCity')}</th>
                <th className="px-3 py-2 text-start">{t('columnStatus')}</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((row) => (
                <tr key={row.id} className="border-t border-surface-muted">
                  <td className="px-3 py-2 text-text">{row.title}</td>
                  <td className="px-3 py-2 text-text">{row.employerDisplayName ?? '—'}</td>
                  <td className="px-3 py-2 text-text">{row.city ?? '—'}</td>
                  <td className="px-3 py-2 text-text">{t(`status.${row.status}`)}</td>
                  <td className="px-3 py-2">
                    <Link to={`/office/job-advertisements/${row.id}`} className="text-primary">
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
