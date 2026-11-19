import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useSuggestionQueue } from '../api/officeSuggestionsReview.queries'
import type { SuggestionStatusCode } from '../types/officeSuggestionsReview.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { FilterChips } from '@/shared/ui/FilterChips'
import { Card } from '@/shared/ui/Card'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'
import { StatusTag } from '@/shared/ui/StatusTag'

const STATUS_FILTERS: (SuggestionStatusCode | 'all')[] = ['all', 'New', 'UnderReview', 'Responded']
const PAGE_SIZE = 20

export default function SuggestionQueuePage() {
  const { t } = useTranslation('officeSuggestionsReview')
  const [status, setStatus] = useState<SuggestionStatusCode | 'all'>('all')
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useSuggestionQueue({
    status: status === 'all' ? undefined : status,
    pageIndex,
    pageSize: PAGE_SIZE,
  })

  function handleFilterChange(next: SuggestionStatusCode | 'all') {
    setStatus(next)
    setPageIndex(0)
  }

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8">
      <PageHeader title={t('queueTitle')} />

      <FilterChips
        options={STATUS_FILTERS.map((s) => ({ value: s, label: s === 'all' ? t('filterAll') : t(`status.${s}`) }))}
        value={status}
        onChange={handleFilterChange}
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyQueue')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((row) => (
            <Link key={row.id} to={`/office/suggestions/${row.id}`} className="block">
              <Card className="flex flex-col gap-2 transition-colors duration-150 hover:border-primary">
                <div className="flex items-center justify-between gap-2">
                  <StatusTag domain="suggestion" value={row.status} label={t(`status.${row.status}`)} />
                  <span className="text-xs text-text-muted">{new Date(row.creationDate).toLocaleDateString('ar')}</span>
                </div>
                <p className="text-sm text-text">{row.content}</p>
              </Card>
            </Link>
          ))}
        </div>
      )}

      {data && <Pagination pageIndex={pageIndex} totalPages={data.pagination.totalPages} onChange={setPageIndex} />}
    </div>
  )
}
