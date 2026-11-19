import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApplicantRow } from '../components/ApplicantRow'
import { useJobAdApplicants, useUpdateApplicationOutcome } from '../api/employerJobAds.queries'
import type { EmployerFacingStatusCode } from '../types/employerJobAds.types'

const PAGE_SIZE = 20

export default function JobAdApplicantsPage() {
  const { t } = useTranslation('employerJobAds')
  const { id } = useParams<{ id: string }>()
  const adId = id ?? ''
  const [pageIndex, setPageIndex] = useState(0)
  const filter = { pageIndex, pageSize: PAGE_SIZE }

  const { data, isLoading } = useJobAdApplicants(adId, filter)
  const updateOutcome = useUpdateApplicationOutcome(adId, filter)

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-10">
      <Link to={`/employer/job-advertisements/${adId}`} className="text-sm text-primary">
        {t('backToList')}
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
                <th className="px-3 py-2 text-start">{t('columnWithdrawn')}</th>
                <th className="px-3 py-2 text-start">{t('columnContact')}</th>
                <th className="px-3 py-2 text-start">{t('columnOutcome')}</th>
                <th className="px-3 py-2" />
                <th className="px-3 py-2" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((applicant) => (
                <ApplicantRow
                  key={applicant.applicationId}
                  applicant={applicant}
                  isUpdating={updateOutcome.isPending && updateOutcome.variables?.applicationId === applicant.applicationId}
                  onUpdateOutcome={(outcome: EmployerFacingStatusCode) =>
                    updateOutcome.mutate({ applicationId: applicant.applicationId, outcome })
                  }
                />
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
