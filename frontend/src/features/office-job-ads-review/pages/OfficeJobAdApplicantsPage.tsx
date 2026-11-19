import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { useOfficeJobAdApplicants } from '../api/officeJobAdsReview.queries'

const PAGE_SIZE = 20

export default function OfficeJobAdApplicantsPage() {
  const { t } = useTranslation('officeJobAdsReview')
  const { id } = useParams<{ id: string }>()
  const adId = id ?? ''
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useOfficeJobAdApplicants(adId, { pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-10">
      <Link to={`/office/job-advertisements/${adId}`} className="text-sm text-primary">
        {t('backToQueue')}
      </Link>
      <h1 className="text-lg font-semibold text-text">{t('applicantsTitle')}</h1>

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && <p className="text-sm text-text-muted">{t('emptyApplicants')}</p>}

      {!isLoading && data && data.items.length > 0 && (
        <div className="overflow-x-auto rounded-md border border-surface-muted">
          <table className="w-full text-start text-sm">
            <thead className="bg-surface-muted text-text-muted">
              <tr>
                <th className="px-3 py-2 text-start">{t('columnApplicant')}</th>
                <th className="px-3 py-2 text-start">{t('columnAppliedAt')}</th>
                <th className="px-3 py-2 text-start">{t('columnContact')}</th>
                <th className="px-3 py-2 text-start">{t('columnOutcome')}</th>
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((applicant) => (
                <tr key={applicant.applicationId} className="border-t border-surface-muted">
                  <td className="px-3 py-2 text-text">
                    {applicant.jobSeekerName}
                    {applicant.withdrawnAt && <span className="ms-2 text-xs text-danger">({t('withdrawn')})</span>}
                  </td>
                  <td className="px-3 py-2 text-text-muted">{new Date(applicant.appliedAt).toLocaleDateString('ar')}</td>
                  <td className="px-3 py-2 text-text">
                    <div className="flex flex-col gap-0.5">
                      {applicant.phone && <span>{applicant.phone}</span>}
                      {applicant.email && <span>{applicant.email}</span>}
                    </div>
                  </td>
                  <td className="px-3 py-2 text-text">{t(`outcome.${applicant.employerFacingStatus}`)}</td>
                  <td className="px-3 py-2">
                    <Link to={`/office/applications/${applicant.applicationId}`} className="text-primary">
                      {t('interviewAndReports')}
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
