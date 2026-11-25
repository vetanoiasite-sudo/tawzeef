import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useOfficeAccountsPayments, useOfficeAccountsSummary } from '../api/officeAccounts.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'
import { FilePreviewModal } from '@/shared/ui/FilePreviewModal'
import { useFilePreview } from '@/shared/hooks/useFilePreview'

const PAGE_SIZE = 20
const MONTH_NAMES_AR = [
  'يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو',
  'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر',
]

const now = new Date()

/** الحسابات — the office's own ledger of confirmed advertisement payments, scoped server-side to
 * the calling office (2026-11-25 product decision: no cross-office/super-admin rollup). */
export default function OfficeAccountsPage() {
  const { t } = useTranslation('officeAccounts')
  const [year, setYear] = useState(now.getFullYear())
  const [month, setMonth] = useState(now.getMonth() + 1)
  const [employerName, setEmployerName] = useState('')
  const [pageIndex, setPageIndex] = useState(0)
  const preview = useFilePreview()

  const { data, isLoading } = useOfficeAccountsPayments({
    year,
    month,
    employerName: employerName || undefined,
    pageIndex,
    pageSize: PAGE_SIZE,
  })
  const { data: summary } = useOfficeAccountsSummary(year, month)

  function shiftMonth(delta: number) {
    const d = new Date(year, month - 1 + delta, 1)
    setYear(d.getFullYear())
    setMonth(d.getMonth() + 1)
    setPageIndex(0)
  }

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      <Card className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Button variant="secondary" size="sm" onClick={() => shiftMonth(-1)}>
            {t('previousMonth')}
          </Button>
          <span className="min-w-[140px] text-center font-semibold text-content-primary">
            {MONTH_NAMES_AR[month - 1]} {year}
          </span>
          <Button variant="secondary" size="sm" onClick={() => shiftMonth(1)}>
            {t('nextMonth')}
          </Button>
        </div>
        <div className="flex flex-col items-end">
          <span className="text-sm text-content-tertiary">{t('monthTotalLabel')}</span>
          <span className="text-lg font-bold text-content-primary">
            {(summary?.totalAmount ?? 0).toLocaleString('ar')} {summary?.currency ?? 'EGP'}
          </span>
        </div>
      </Card>

      <TextInput
        id="employerNameFilter"
        label={t('employerNameFilterLabel')}
        value={employerName}
        onChange={(e) => {
          setEmployerName(e.target.value)
          setPageIndex(0)
        }}
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((item) => (
            <Card key={item.id} className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex flex-col gap-1">
                <span className="font-semibold text-content-primary">{item.employerDisplayName}</span>
                <span className="text-sm text-content-secondary">
                  {t('columnRecordedBy')}: {item.recordedByName || '—'}
                </span>
                {item.paidAt && (
                  <span className="text-sm text-content-tertiary">
                    {new Date(item.paidAt).toLocaleString('ar')}
                  </span>
                )}
              </div>
              <div className="flex items-center gap-3">
                <span className="font-bold text-content-primary">
                  {item.amount.toLocaleString('ar')} {item.currency}
                </span>
                <Button variant="secondary" size="sm" onClick={() => void preview.open(item.receiptUrl)}>
                  {t('viewReceiptAction')}
                </Button>
              </div>
            </Card>
          ))}
        </div>
      )}

      {data && (
        <Pagination pageIndex={pageIndex} totalPages={data.pagination.totalPages} onChange={setPageIndex} />
      )}

      {preview.preview && <FilePreviewModal preview={preview.preview} onClose={preview.close} />}
    </div>
  )
}
