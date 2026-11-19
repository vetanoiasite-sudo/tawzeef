import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useDueFollowUps } from '../api/interviewsAndReports.queries'

const PAGE_SIZE = 20

export default function OfficeHiringFollowUpsDuePage() {
  const { t } = useTranslation('interviewsAndReports')
  const [pageIndex, setPageIndex] = useState(0)
  const { data, isLoading } = useDueFollowUps({ pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-10">
      <h1 className="text-lg font-semibold text-text">{t('dueFollowUpsTitle')}</h1>

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && <p className="text-sm text-text-muted">{t('emptyDueFollowUps')}</p>}

      {!isLoading && data && data.items.length > 0 && (
        <div className="overflow-x-auto rounded-md border border-surface-muted">
          <table className="w-full text-start text-sm">
            <thead className="bg-surface-muted text-text-muted">
              <tr>
                <th className="px-3 py-2 text-start">{t('columnApplicant')}</th>
                <th className="px-3 py-2 text-start">{t('columnEmployer')}</th>
                <th className="px-3 py-2 text-start">{t('columnJob')}</th>
                <th className="px-3 py-2 text-start">{t('columnHiredAt')}</th>
                <th className="px-3 py-2 text-start">{t('columnDueAt')}</th>
                <th className="px-3 py-2 text-start">{t('columnStatus')}</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((row) => (
                <tr key={row.id} className="border-t border-surface-muted">
                  <td className="px-3 py-2 text-text">{row.jobSeekerName}</td>
                  <td className="px-3 py-2 text-text">{row.employerDisplayName}</td>
                  <td className="px-3 py-2 text-text">{row.jobTitle}</td>
                  <td className="px-3 py-2 text-text-muted">{new Date(row.hiredAt).toLocaleDateString('ar')}</td>
                  <td className="px-3 py-2 text-text-muted">{new Date(row.dueAt).toLocaleDateString('ar')}</td>
                  <td className="px-3 py-2 text-text">{t(`followUpStatus.${row.status}`)}</td>
                  <td className="px-3 py-2">
                    <Link to={`/office/applications/${row.jobApplicationId}`} className="text-primary">
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
