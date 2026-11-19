import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ComplaintSubjectTypeCode, OfficeDecisionTypeCode } from '../types/officeComplaintReview.types'

export function DecideControl({
  subjectType,
  isSubmitting,
  error,
  onDecide,
}: {
  subjectType: ComplaintSubjectTypeCode
  isSubmitting: boolean
  error?: string
  onDecide: (decisionType: OfficeDecisionTypeCode, reason: string) => void
}) {
  const { t } = useTranslation('officeComplaintReview')
  const [decisionType, setDecisionType] = useState<OfficeDecisionTypeCode | null>(null)
  const [reason, setReason] = useState('')

  if (subjectType === 'Employer') {
    return (
      <div className="flex flex-col gap-2">
        <h3 className="text-sm font-semibold text-text">{t('decideSection')}</h3>
        <p className="text-sm text-text-muted">{t('decideNotSupportedForEmployer')}</p>
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-2">
      <h3 className="text-sm font-semibold text-text">{t('decideSection')}</h3>
      <p className="text-xs text-text-muted">{t('decideHint')}</p>

      {!decisionType && (
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => setDecisionType('Suspend')}
            className="rounded-md border border-danger px-3 py-1.5 text-sm font-medium text-danger"
          >
            {t('decideSuspend')}
          </button>
          <button
            type="button"
            onClick={() => setDecisionType('Reinstate')}
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface"
          >
            {t('decideReinstate')}
          </button>
        </div>
      )}

      {decisionType && (
        <div className="flex flex-col gap-2 rounded-md border border-danger/40 p-3">
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder={t('reasonPlaceholder')}
            rows={2}
            className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text outline-none focus:border-danger"
          />
          {error && <p className="text-sm text-danger">{error}</p>}
          <div className="flex gap-2">
            <button
              type="button"
              disabled={isSubmitting || !reason.trim()}
              onClick={() => onDecide(decisionType, reason.trim())}
              className="rounded-md bg-danger px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
            >
              {decisionType === 'Suspend' ? t('decideSuspend') : t('decideReinstate')}
            </button>
            <button
              type="button"
              onClick={() => setDecisionType(null)}
              className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text"
            >
              {t('cancel')}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
