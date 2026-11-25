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
    <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      <Card className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2">
          <Button variant="secondary" size="sm" onClick={() => shiftMonth(-1)}>
            {t('previousMonth')}
          </Button>
          <span className="text-center font-semibold text-content-primary">
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
        <>
          {/* Real table from sm (640px) up — below that it reflows into one card per row,
           * each card listing that row's fields (2026-11-25 product decision), instead of a
           * horizontally-scrolling table on a phone screen. */}
          <Card className="hidden overflow-x-auto p-0 sm:block">
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="border-b border-border-primary bg-surface-muted text-content-tertiary">
                  <th className="px-4 py-3 text-start font-semibold">{t('columnEmployer')}</th>
                  <th className="px-4 py-3 text-start font-semibold">{t('columnRecordedBy')}</th>
                  <th className="px-4 py-3 text-start font-semibold">{t('columnDate')}</th>
                  <th className="px-4 py-3 text-start font-semibold">{t('columnAmount')}</th>
                  <th className="px-4 py-3 text-start font-semibold">{t('columnReceipt')}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.id} className="border-b border-border-primary last:border-b-0">
                    <td className="px-4 py-3 font-semibold text-content-primary">{item.employerDisplayName}</td>
                    <td className="px-4 py-3 text-content-secondary">{item.recordedByName || '—'}</td>
                    <td className="px-4 py-3 text-content-tertiary">
                      {item.paidAt ? new Date(item.paidAt).toLocaleString('ar') : '—'}
                    </td>
                    <td className="px-4 py-3 font-bold text-content-primary">
                      {item.amount.toLocaleString('ar')} {item.currency}
                    </td>
                    <td className="px-4 py-3">
                      <Button variant="secondary" size="sm" onClick={() => void preview.open(item.receiptUrl)}>
                        {t('viewReceiptAction')}
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </Card>

          <div className="flex flex-col gap-3 sm:hidden">
            {data.items.map((item) => (
              <Card key={item.id} className="flex flex-col gap-2">
                <div className="flex items-center justify-between gap-2">
                  <span className="font-semibold text-content-primary">{item.employerDisplayName}</span>
                  <span className="font-bold text-content-primary">
                    {item.amount.toLocaleString('ar')} {item.currency}
                  </span>
                </div>
                <div className="flex justify-between text-sm text-content-secondary">
                  <span>{t('columnRecordedBy')}</span>
                  <span>{item.recordedByName || '—'}</span>
                </div>
                <div className="flex justify-between text-sm text-content-tertiary">
                  <span>{t('columnDate')}</span>
                  <span>{item.paidAt ? new Date(item.paidAt).toLocaleString('ar') : '—'}</span>
                </div>
                <Button
                  variant="secondary"
                  size="sm"
                  className="self-start"
                  onClick={() => void preview.open(item.receiptUrl)}
                >
                  {t('viewReceiptAction')}
                </Button>
              </Card>
            ))}
          </div>
        </>
      )}

      {data && (
        <Pagination pageIndex={pageIndex} totalPages={data.pagination.totalPages} onChange={setPageIndex} />
      )}

      {preview.preview && <FilePreviewModal preview={preview.preview} onClose={preview.close} />}
    </div>
  )
}
