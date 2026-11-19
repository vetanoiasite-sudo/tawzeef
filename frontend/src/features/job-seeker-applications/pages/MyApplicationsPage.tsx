import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useMyApplications, useWithdrawApplication } from '../api/jobSeekerApplications.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20

export default function MyApplicationsPage() {
  const { t } = useTranslation('jobSeekerApplications')
  const [pageIndex, setPageIndex] = useState(0)
  const { data, isLoading } = useMyApplications({ pageIndex, pageSize: PAGE_SIZE })
  const withdraw = useWithdrawApplication()

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((row) => (
            <Card key={row.id} className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div className="flex flex-col gap-1">
                <Link to={`/jobs/${row.jobAdvertisementId}`} className="font-semibold text-primary hover:underline">
                  {row.jobTitle}
                </Link>
                <p className="text-sm text-text-muted">{row.employerDisplayName}</p>
                <p className="text-xs text-text-muted">
                  {t('columnAppliedAt')}: {new Date(row.appliedAt).toLocaleDateString('ar')}
                </p>
              </div>
              <div className="flex items-center gap-3">
                {row.withdrawnAt ? (
                  <StatusTag domain="applicationJobSeekerFacing" value="NotContacted" label={t('withdrawn')} />
                ) : (
                  <StatusTag
                    domain="applicationJobSeekerFacing"
                    value={row.jobSeekerFacingStatus}
                    label={t(`status.${row.jobSeekerFacingStatus}`)}
                  />
                )}
                {!row.withdrawnAt && (
                  <Button
                    variant="danger"
                    disabled={withdraw.isPending}
                    onClick={() => {
                      if (window.confirm(t('withdrawConfirm'))) withdraw.mutate(row.id)
                    }}
                  >
                    {t('withdrawAction')}
                  </Button>
                )}
              </div>
            </Card>
          ))}
        </div>
      )}

      {data && <Pagination pageIndex={pageIndex} totalPages={data.pagination.totalPages} onChange={setPageIndex} />}
    </div>
  )
}
