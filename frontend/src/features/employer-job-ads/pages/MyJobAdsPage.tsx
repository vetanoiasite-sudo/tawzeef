import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useMyJobAds } from '../api/employerJobAds.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { Card } from '@/shared/ui/Card'
import { StatusTag } from '@/shared/ui/StatusTag'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20

export default function MyJobAdsPage() {
  const { t } = useTranslation('employerJobAds')
  const [pageIndex, setPageIndex] = useState(0)
  const { data, isLoading } = useMyJobAds({ pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader
        title={t('pageTitle')}
        action={
          <Link to="/employer/job-advertisements/new">
            <Button variant="primary">{t('createAction')}</Button>
          </Link>
        }
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((row) => (
            <Card key={row.id} className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex flex-col gap-1">
                <p className="font-semibold text-text">{row.title}</p>
                <p className="text-xs text-text-muted">
                  {row.officeName ?? '—'} · {row.city ?? '—'} · {t('columnVacancies')}:{' '}
                  {row.vacancyCount}
                </p>
              </div>
              <div className="flex items-center gap-3">
                <StatusTag
                  domain="advertisement"
                  value={row.status}
                  label={t(`status.${row.status}`)}
                />
                <Link to={`/employer/job-advertisements/${row.id}`}>
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
