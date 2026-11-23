import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useComplaintQueue } from '../api/officeComplaintReview.queries'
import type { ComplaintStatusCode } from '../types/officeComplaintReview.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { FilterChips } from '@/shared/ui/FilterChips'
import { Card } from '@/shared/ui/Card'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'
import { StatusTag } from '@/shared/ui/StatusTag'

const STATUS_FILTERS: (ComplaintStatusCode | 'all')[] = [
  'all',
  'New',
  'UnderReview',
  'Resolved',
  'Rejected',
]

const PAGE_SIZE = 20

export default function ComplaintQueuePage() {
  const { t } = useTranslation('officeComplaintReview')
  const [status, setStatus] = useState<ComplaintStatusCode | 'all'>('all')
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useComplaintQueue({
    status: status === 'all' ? undefined : status,
    pageIndex,
    pageSize: PAGE_SIZE,
  })

  function handleFilterChange(next: ComplaintStatusCode | 'all') {
    setStatus(next)
    setPageIndex(0)
  }

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8">
      <PageHeader title={t('queueTitle')} />

      <FilterChips
        options={STATUS_FILTERS.map((s) => ({
          value: s,
          label: s === 'all' ? t('filterAll') : t(`status.${s}`),
        }))}
        value={status}
        onChange={handleFilterChange}
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyQueue')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((row) => (
            <Card key={row.id} className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex flex-col gap-1">
                <span className="font-medium text-text">{row.subjectName}</span>
                <span className="text-xs text-text-muted">
                  {new Date(row.createdAt).toLocaleDateString('ar')}
                </span>
              </div>
              <div className="flex flex-wrap items-center gap-3">
                <StatusTag
                  domain="complaint"
                  value={row.status}
                  label={t(`status.${row.status}`)}
                />
                <span className="text-sm text-text-muted">
                  {t(`validity.${row.validityDecision}`)}
                </span>
                <Link
                  to={`/office/complaints/${row.id}`}
                  className="text-sm font-medium text-primary hover:underline"
                >
                  {t('viewAction')}
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
