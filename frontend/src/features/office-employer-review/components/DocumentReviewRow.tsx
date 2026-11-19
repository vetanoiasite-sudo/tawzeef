import clsx from 'clsx'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ReasonForm } from './ReasonForm'
import type { OfficeEmployerDocument } from '../types/officeEmployerReview.types'

const STATUS_TONE: Record<string, string> = {
  Pending: 'text-warning',
  Approved: 'text-success',
  Rejected: 'text-danger',
}

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
    <div className="flex flex-col gap-2 rounded-md border border-surface-muted p-4">
      <div className="flex items-center justify-between gap-2">
        <span className="font-medium text-text">{t(`documentType.${document.documentType}`)}</span>
        <span className={clsx('text-sm font-medium', STATUS_TONE[document.reviewStatus])}>
          {t(`documentStatus.${document.reviewStatus}`)}
        </span>
      </div>

      <p className="text-sm text-text-muted">{document.originalFileName}</p>
      {document.rejectionReason && (
        <p className="text-sm text-danger">
          {t('rejectionReasonLabel')}: {document.rejectionReason}
        </p>
      )}

      {document.reviewStatus === 'Pending' && !showReject && (
        <div className="flex gap-2">
          <button
            type="button"
            disabled={isApproving}
            onClick={onApprove}
            className="rounded-md bg-success px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
          >
            {t('approve')}
          </button>
          <button
            type="button"
            onClick={() => setShowReject(true)}
            className="rounded-md border border-danger px-3 py-1.5 text-sm font-medium text-danger"
          >
            {t('reject')}
          </button>
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
    </div>
  )
}
