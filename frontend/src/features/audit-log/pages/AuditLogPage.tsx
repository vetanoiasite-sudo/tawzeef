import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useAuditLog } from '../api/auditLog.queries'
import type { AuditLogDecisionTypeCode, AuditLogTargetTypeCode } from '../types/auditLog.types'
import { useAccess } from '@/shared/auth/useAccess'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'
import { Select } from '@/shared/ui/Select'

const PAGE_SIZE = 20
const TARGET_TYPES: AuditLogTargetTypeCode[] = ['JobSeeker', 'Employer']
const DECISION_TYPES: AuditLogDecisionTypeCode[] = [
  'Suspend',
  'Reinstate',
  'StopCooperation',
  'ResumeCooperation',
]

/** سجل التدقيق — a read-only viewer over OfficeDecision rows (suspend/reinstate/stop-cooperation/
 * resume-cooperation). Office staff implicitly see only their own office (the backend scopes it
 * server-side); SuperAdmin sees every office, so the office name column stays visible for them. */
export default function AuditLogPage() {
  const { t } = useTranslation('auditLog')
  const { isSuperAdmin } = useAccess()
  const [targetType, setTargetType] = useState<AuditLogTargetTypeCode | undefined>(undefined)
  const [decisionType, setDecisionType] = useState<AuditLogDecisionTypeCode | undefined>(undefined)
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useAuditLog({
    targetType,
    decisionType,
    pageIndex,
    pageSize: PAGE_SIZE,
  })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Select
          label={t('targetTypeFilterLabel')}
          value={targetType ?? ''}
          onChange={(e) => {
            setTargetType((e.target.value || undefined) as AuditLogTargetTypeCode | undefined)
            setPageIndex(0)
          }}
        >
          <option value="">{t('filterAll')}</option>
          {TARGET_TYPES.map((type) => (
            <option key={type} value={type}>
              {t(`targetType.${type}`)}
            </option>
          ))}
        </Select>

        <Select
          label={t('decisionTypeFilterLabel')}
          value={decisionType ?? ''}
          onChange={(e) => {
            setDecisionType((e.target.value || undefined) as AuditLogDecisionTypeCode | undefined)
            setPageIndex(0)
          }}
        >
          <option value="">{t('filterAll')}</option>
          {DECISION_TYPES.map((type) => (
            <option key={type} value={type}>
              {t(`decisionType.${type}`)}
            </option>
          ))}
        </Select>
      </div>

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyLog')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((entry) => (
            <Card key={entry.id} className="flex flex-col gap-2">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <span className="font-semibold text-content-primary">
                  {t(`decisionType.${entry.decisionType}`)}
                  {' — '}
                  {entry.targetName ?? t(`targetType.${entry.targetType}`)}
                </span>
                <span className="text-sm text-content-tertiary">
                  {new Date(entry.decidedAt).toLocaleString('ar')}
                </span>
              </div>

              <div className="flex flex-wrap gap-3 text-sm text-content-secondary">
                <span>
                  {t('columnBy')}: {entry.decidedByName ?? '—'}
                </span>
                {isSuperAdmin && entry.officeName && (
                  <span>
                    {t('columnOffice')}: {entry.officeName}
                  </span>
                )}
              </div>

              {entry.reason && (
                <p className="text-sm text-content-secondary">
                  {t('columnReason')}: {entry.reason}
                </p>
              )}
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
