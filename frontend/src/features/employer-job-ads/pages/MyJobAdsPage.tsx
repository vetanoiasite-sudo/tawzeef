import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useMyJobAds } from '../api/employerJobAds.queries'

const PAGE_SIZE = 20

export default function MyJobAdsPage() {
  const { t } = useTranslation('employerJobAds')
  const [pageIndex, setPageIndex] = useState(0)
  const { data, isLoading } = useMyJobAds({ pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-10">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-text">{t('pageTitle')}</h1>
        <Link to="/employer/job-advertisements/new" className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface">
          {t('createAction')}
        </Link>
      </div>

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && <p className="text-sm text-text-muted">{t('emptyList')}</p>}

      {!isLoading && data && data.items.length > 0 && (
        <div className="overflow-x-auto rounded-md border border-surface-muted">
          <table className="w-full text-start text-sm">
            <thead className="bg-surface-muted text-text-muted">
              <tr>
                <th className="px-3 py-2 text-start">{t('columnTitle')}</th>
                <th className="px-3 py-2 text-start">{t('columnCity')}</th>
                <th className="px-3 py-2 text-start">{t('columnStatus')}</th>
                <th className="px-3 py-2 text-start">{t('columnVacancies')}</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((row) => (
                <tr key={row.id} className="border-t border-surface-muted">
                  <td className="px-3 py-2 text-text">{row.title}</td>
                  <td className="px-3 py-2 text-text">{row.city ?? '—'}</td>
                  <td className="px-3 py-2 text-text">{t(`status.${row.status}`)}</td>
                  <td className="px-3 py-2 text-text">{row.vacancyCount}</td>
                  <td className="px-3 py-2">
                    <Link to={`/employer/job-advertisements/${row.id}`} className="text-primary">
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
