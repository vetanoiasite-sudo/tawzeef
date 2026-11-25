import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApplicantRow } from '../components/ApplicantRow'
import { useJobAdApplicants, useUpdateApplicationOutcome } from '../api/employerJobAds.queries'
import type { EmployerFacingStatusCode } from '../types/employerJobAds.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

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
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link
        to={`/employer/job-advertisements/${adId}`}
        className="text-sm font-medium text-primary hover:underline"
      >
        {t('backToList')}
      </Link>
      <PageHeader title={t('applicantsTitle')} />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && (
        <EmptyState message={t('emptyApplicants')} />
      )}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-4">
          {data.items.map((applicant) => (
            <ApplicantRow
              key={applicant.applicationId}
              adId={adId}
              applicant={applicant}
              isUpdating={
                updateOutcome.isPending &&
                updateOutcome.variables?.applicationId === applicant.applicationId
              }
              onUpdateOutcome={(outcome: EmployerFacingStatusCode) =>
                updateOutcome.mutate({ applicationId: applicant.applicationId, outcome })
              }
            />
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
