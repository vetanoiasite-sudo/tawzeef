import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type {
  ComplaintSubjectTypeCode,
  OfficeDecisionTypeCode,
} from '../types/officeComplaintReview.types'
import { Button } from '@/shared/ui/Button'
import { TextArea } from '@/shared/ui/TextInput'

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
    <div className="flex flex-col gap-3">
      <h3 className="text-sm font-semibold text-text">{t('decideSection')}</h3>
      <p className="text-xs text-text-muted">{t('decideHint')}</p>

      {!decisionType && (
        <div className="flex gap-2">
          <Button variant="danger" onClick={() => setDecisionType('Suspend')}>
            {t('decideSuspend')}
          </Button>
          <Button variant="primary" onClick={() => setDecisionType('Reinstate')}>
            {t('decideReinstate')}
          </Button>
        </div>
      )}

      {decisionType && (
        <div className="flex flex-col gap-3 rounded-md border border-danger/40 p-3">
          <TextArea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder={t('reasonPlaceholder')}
            rows={2}
          />
          {error && <p className="text-sm text-danger">{error}</p>}
          <div className="flex gap-2">
            <Button
              variant="danger"
              disabled={isSubmitting || !reason.trim()}
              onClick={() => onDecide(decisionType, reason.trim())}
            >
              {decisionType === 'Suspend' ? t('decideSuspend') : t('decideReinstate')}
            </Button>
            <Button variant="secondary" onClick={() => setDecisionType(null)}>
              {t('cancel')}
            </Button>
          </div>
        </div>
      )}
    </div>
  )
}
