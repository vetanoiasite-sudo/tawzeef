import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { useOfficeJobAdApplicants } from '../api/officeJobAdsReview.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20

export default function OfficeJobAdApplicantsPage() {
  const { t } = useTranslation('officeJobAdsReview')
  const { id } = useParams<{ id: string }>()
  const adId = id ?? ''
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useOfficeJobAdApplicants(adId, { pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link
        to={`/office/job-advertisements/${adId}`}
        className="text-sm font-medium text-primary hover:underline"
      >
        {t('backToQueue')}
      </Link>
      <PageHeader title={t('applicantsTitle')} />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && (
        <EmptyState message={t('emptyApplicants')} />
      )}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((applicant) => (
            <Card key={applicant.applicationId} className="flex flex-col gap-3">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-semibold text-text">
                    {applicant.jobSeekerName}
                    {applicant.withdrawnAt && (
                      <span className="ms-2 text-xs text-danger">({t('withdrawn')})</span>
                    )}
                  </p>
                  <p className="text-xs text-text-muted">
                    {new Date(applicant.appliedAt).toLocaleDateString('ar')}
                  </p>
                </div>
                <StatusTag
                  domain="applicationEmployerFacing"
                  value={applicant.employerFacingStatus}
                  label={t(`outcome.${applicant.employerFacingStatus}`)}
                />
              </div>

              <div className="flex flex-col gap-0.5 text-sm text-text">
                {applicant.phone && <span>{applicant.phone}</span>}
                {applicant.email && <span>{applicant.email}</span>}
              </div>

              <Link to={`/office/applications/${applicant.applicationId}`} className="self-start">
                <Button variant="ghost">{t('interviewAndReports')}</Button>
              </Link>
            </Card>
          ))}
        </div>
      )}

      {data && (
        <Pagination
          pageIndex={pageIndex}
          totalPages={data.pagination.totalPages}
          onChange={setPageIndex}
        />
      )}
    </div>
  )
}
