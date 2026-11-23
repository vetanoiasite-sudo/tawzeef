import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useDueFollowUps } from '../api/interviewsAndReports.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20

export default function OfficeHiringFollowUpsDuePage() {
  const { t } = useTranslation('interviewsAndReports')
  const [pageIndex, setPageIndex] = useState(0)
  const { data, isLoading } = useDueFollowUps({ pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('dueFollowUpsTitle')} />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && (
        <EmptyState message={t('emptyDueFollowUps')} />
      )}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((row) => (
            <Card
              key={row.id}
              className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between"
            >
              <div className="flex flex-col gap-1">
                <p className="font-semibold text-text">{row.jobSeekerName}</p>
                <p className="text-sm text-text-muted">
                  {row.jobTitle} · {row.employerDisplayName}
                </p>
                <p className="text-xs text-text-muted">
                  {t('columnHiredAt')}: {new Date(row.hiredAt).toLocaleDateString('ar')} ·{' '}
                  {t('columnDueAt')}: {new Date(row.dueAt).toLocaleDateString('ar')}
                </p>
              </div>
              <div className="flex items-center gap-3">
                <StatusTag
                  domain="hiringFollowUp"
                  value={row.status}
                  label={t(`followUpStatus.${row.status}`)}
                />
                <Link to={`/office/applications/${row.jobApplicationId}`}>
                  <Button variant="secondary">{t('viewAction')}</Button>
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
