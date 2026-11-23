import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useReviewQueue } from '../api/officeJobAdsReview.queries'
import type { JobAdvertisementStatusCode } from '../types/officeJobAdsReview.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { FilterChips } from '@/shared/ui/FilterChips'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20
type StatusFilter = JobAdvertisementStatusCode | 'all'
const STATUS_FILTERS: JobAdvertisementStatusCode[] = [
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
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('queueTitle')} />

      <FilterChips<StatusFilter>
        options={[
          { value: 'all', label: t('filterAll') },
          ...STATUS_FILTERS.map((s) => ({ value: s, label: t(`status.${s}`) })),
        ]}
        value={status ?? 'all'}
        onChange={(v) => {
          setStatus(v === 'all' ? undefined : v)
          setPageIndex(0)
        }}
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyQueue')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((row) => (
            <Card key={row.id} className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex flex-col gap-1">
                <p className="font-semibold text-text">{row.title}</p>
                <p className="text-xs text-text-muted">
                  {row.employerDisplayName ?? '—'} · {row.city ?? '—'}
                </p>
              </div>
              <div className="flex items-center gap-3">
                <StatusTag
                  domain="advertisement"
                  value={row.status}
                  label={t(`status.${row.status}`)}
                />
                <Link to={`/office/job-advertisements/${row.id}`}>
                  <Button variant="ghost">{t('viewAction')}</Button>
                </Link>
              </div>
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
