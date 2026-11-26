import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useOfficeEmployedJobSeekers } from '../api/officeEmployedJobSeekers.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20

export default function OfficeEmployedJobSeekersPage() {
  const { t } = useTranslation('officeEmployedJobSeekers')
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useOfficeEmployedJobSeekers({ pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && data && data.items.length > 0 && (
        <>
          <Card className="hidden overflow-x-auto p-0 sm:block">
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="border-b border-border-primary bg-surface-muted text-content-tertiary">
                  <th className="px-4 py-3 text-start font-semibold">{t('columnName')}</th>
                  <th className="px-4 py-3 text-start font-semibold">{t('columnMobile')}</th>
                  <th className="px-4 py-3 text-start font-semibold">{t('columnEmployedAt')}</th>
                  <th className="px-4 py-3 text-start font-semibold">{t('columnStatus')}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.jobSeekerId} className="border-b border-border-primary last:border-b-0">
                    <td className="px-4 py-3 font-semibold text-content-primary">{item.fullName}</td>
                    <td className="px-4 py-3 text-content-secondary">{item.mobileNumber}</td>
                    <td className="px-4 py-3 text-content-tertiary">
                      {new Date(item.employedAt).toLocaleDateString('ar')}
                    </td>
                    <td className="px-4 py-3">
                      <StatusBadge leftJobAt={item.leftJobAt} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex flex-col gap-3 sm:hidden">
            {data.items.map((item) => (
              <Card key={item.jobSeekerId} className="flex flex-col gap-2">
                <div className="flex items-center justify-between gap-2">
                  <span className="font-semibold text-content-primary">{item.fullName}</span>
                  <StatusBadge leftJobAt={item.leftJobAt} />
                </div>
                <div className="flex justify-between text-sm text-content-secondary">
                  <span>{t('columnMobile')}</span>
                  <span>{item.mobileNumber}</span>
                </div>
                <div className="flex justify-between text-sm text-content-tertiary">
                  <span>{t('columnEmployedAt')}</span>
                  <span>{new Date(item.employedAt).toLocaleDateString('ar')}</span>
                </div>
              </Card>
            ))}
          </div>
        </>
      )}

      {data && (
        <Pagination pageIndex={pageIndex} totalPages={data.pagination.totalPages} onChange={setPageIndex} />
      )}
    </div>
  )
}

function StatusBadge({ leftJobAt }: { leftJobAt: string | null }) {
  const { t } = useTranslation('officeEmployedJobSeekers')
  return leftJobAt ? (
    <span className="rounded-100 bg-warning/10 px-2 py-0.5 text-xs font-semibold text-warning">
      {t('statusLeftJob')}
    </span>
  ) : (
    <span className="rounded-100 bg-success/10 px-2 py-0.5 text-xs font-semibold text-success">
      {t('statusEmployed')}
    </span>
  )
}
