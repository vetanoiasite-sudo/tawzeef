import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ReasonForm } from './ReasonForm'
import type { OfficeEmployerDocument } from '../types/officeEmployerReview.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'

export function DocumentReviewRow({
  document,
  isApproving,
  isRejecting,
  rejectError,
  onApprove,
  onReject,
}: {
  document: OfficeEmployerDocument
  isApproving: boolean
  isRejecting: boolean
  rejectError?: string
  onApprove: () => void
  onReject: (reason: string) => void
}) {
  const { t } = useTranslation('officeEmployerReview')
  const [showReject, setShowReject] = useState(false)

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-center justify-between gap-2">
        <span className="font-medium text-text">{t(`documentType.${document.documentType}`)}</span>
        <StatusTag
          domain="documentReview"
          value={document.reviewStatus}
          label={t(`documentStatus.${document.reviewStatus}`)}
        />
      </div>

      <p className="text-sm text-text-muted">{document.originalFileName}</p>
      {document.rejectionReason && (
        <p className="text-sm text-danger">
          {t('rejectionReasonLabel')}: {document.rejectionReason}
        </p>
      )}

      {document.reviewStatus === 'Pending' && !showReject && (
        <div className="flex gap-2">
          <Button disabled={isApproving} onClick={onApprove} className="bg-success hover:bg-success/90">
            {t('approve')}
          </Button>
          <Button variant="danger" onClick={() => setShowReject(true)}>
            {t('reject')}
          </Button>
        </div>
      )}

      {showReject && (
        <ReasonForm
          confirmLabel={t('reject')}
          isSubmitting={isRejecting}
          error={rejectError}
          onConfirm={(reason) => {
            onReject(reason)
            setShowReject(false)
          }}
          onCancel={() => setShowReject(false)}
        />
      )}
    </Card>
  )
}
