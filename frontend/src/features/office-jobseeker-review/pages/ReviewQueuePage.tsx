import clsx from 'clsx'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useReviewQueue } from '../api/officeJobSeekerReview.queries'
import type { JobSeekerReviewStatusCode } from '../types/officeJobSeekerReview.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { FilterChips } from '@/shared/ui/FilterChips'
import { StatusTag } from '@/shared/ui/StatusTag'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const COMPLAINT_THRESHOLD = 3

type StatusFilterValue = JobSeekerReviewStatusCode | 'all'

const STATUS_FILTERS: StatusFilterValue[] = [
  'all',
  'Submitted',
  'UnderReview',
  'Approved',
  'Rejected',
  'Suspended',
]

const PAGE_SIZE = 20

export default function ReviewQueuePage() {
  const { t } = useTranslation('officeJobSeekerReview')
  const [reviewStatus, setReviewStatus] = useState<JobSeekerReviewStatusCode | undefined>(undefined)
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useReviewQueue({ reviewStatus, pageIndex, pageSize: PAGE_SIZE })

  function handleFilterChange(next: StatusFilterValue) {
    setReviewStatus(next === 'all' ? undefined : next)
    setPageIndex(0)
  }

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('queueTitle')} />

      <FilterChips
        options={STATUS_FILTERS.map((status) => ({
          value: status,
          label: status === 'all' ? t('filterAll') : t(`status.${status}`),
        }))}
        value={reviewStatus ?? 'all'}
        onChange={handleFilterChange}
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyQueue')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((row) => (
            <Link key={row.id} to={`/office/job-seekers/${row.id}`}>
              <Card className="flex flex-wrap items-center justify-between gap-3 transition-colors duration-150 hover:border-primary">
                <div className="flex flex-col gap-1">
                  <span className="font-semibold text-text">{row.fullName}</span>
                  <span className="text-sm text-text-muted">
                    {row.mobileNumber}
                    {row.city && <span> · {row.city}</span>}
                  </span>
                </div>
                <div className="flex flex-wrap items-center gap-3">
                  <StatusTag
                    domain="jobSeekerReview"
                    value={row.reviewStatus}
                    label={t(`status.${row.reviewStatus}`)}
                  />
                  <span
                    className={clsx(
                      'text-sm font-medium',
                      row.validComplaintCount >= COMPLAINT_THRESHOLD
                        ? 'text-danger'
                        : 'text-text-muted',
                    )}
                  >
                    {t('columnComplaints')}: {row.validComplaintCount}
                  </span>
                  <span className="text-sm text-text-muted">
                    {new Date(row.lastUpdatedAt).toLocaleDateString('ar')}
                  </span>
                </div>
              </Card>
            </Link>
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
