import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useMyApplications, useWithdrawApplication } from '../api/jobSeekerApplications.queries'

const PAGE_SIZE = 20

export default function MyApplicationsPage() {
  const { t } = useTranslation('jobSeekerApplications')
  const [pageIndex, setPageIndex] = useState(0)
  const { data, isLoading } = useMyApplications({ pageIndex, pageSize: PAGE_SIZE })
  const withdraw = useWithdrawApplication()

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6 px-4 py-10">
      <h1 className="text-lg font-semibold text-text">{t('pageTitle')}</h1>

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && (
        <p className="text-sm text-text-muted">{t('emptyList')}</p>
      )}

      {!isLoading && data && data.items.length > 0 && (
        <div className="overflow-x-auto rounded-md border border-surface-muted">
          <table className="w-full text-start text-sm">
            <thead className="bg-surface-muted text-text-muted">
              <tr>
                <th className="px-3 py-2 text-start">{t('columnJob')}</th>
                <th className="px-3 py-2 text-start">{t('columnEmployer')}</th>
                <th className="px-3 py-2 text-start">{t('columnAppliedAt')}</th>
                <th className="px-3 py-2 text-start">{t('columnStatus')}</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((row) => (
                <tr key={row.id} className="border-t border-surface-muted">
                  <td className="px-3 py-2 text-text">
                    <Link to={`/jobs/${row.jobAdvertisementId}`} className="text-primary">
                      {row.jobTitle}
                    </Link>
                  </td>
                  <td className="px-3 py-2 text-text">{row.employerDisplayName}</td>
                  <td className="px-3 py-2 text-text-muted">{new Date(row.appliedAt).toLocaleDateString('ar')}</td>
                  <td className="px-3 py-2 text-text">
                    {row.withdrawnAt ? t('withdrawn') : t(`status.${row.jobSeekerFacingStatus}`)}
                  </td>
                  <td className="px-3 py-2">
                    {!row.withdrawnAt && (
                      <button
                        type="button"
                        disabled={withdraw.isPending}
                        onClick={() => {
                          if (window.confirm(t('withdrawConfirm'))) withdraw.mutate(row.id)
                        }}
                        className="text-sm text-danger disabled:opacity-60"
                      >
                        {t('withdrawAction')}
                      </button>
                    )}
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
