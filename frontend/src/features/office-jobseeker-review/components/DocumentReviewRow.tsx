import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ReasonForm } from './ReasonForm'
import type { OfficeJobSeekerDocument } from '../types/officeJobSeekerReview.types'
import { useFilePreview } from '@/shared/hooks/useFilePreview'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { FilePreviewModal } from '@/shared/ui/FilePreviewModal'

export function DocumentReviewRow({
  document,
  profileId,
  isApproving,
  isRejecting,
  rejectError,
  onApprove,
  onReject,
}: {
  document: OfficeJobSeekerDocument
  profileId: string
  isApproving: boolean
  isRejecting: boolean
  rejectError?: string
  onApprove: () => void
  onReject: (reason: string) => void
}) {
  const { t } = useTranslation('officeJobSeekerReview')
  const [showReject, setShowReject] = useState(false)
  const { preview, isLoading: isLoadingPreview, open: openPreview, close: closePreview } = useFilePreview()

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

      <Button
        variant="secondary"
        disabled={isLoadingPreview}
        onClick={() =>
          void openPreview(`/api/office/job-seekers/${profileId}/documents/${document.id}/download`)
        }
        className="self-start"
      >
        {t('viewDocumentAction')}
      </Button>

      {!showReject && (
        <div className="flex gap-2">
          <Button
            disabled={isApproving || document.reviewStatus === 'Approved'}
            onClick={onApprove}
            className="bg-success hover:bg-success/90"
          >
            {t('approve')}
          </Button>
          <Button
            variant="danger"
            disabled={document.reviewStatus === 'Rejected'}
            onClick={() => setShowReject(true)}
          >
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

      {preview && <FilePreviewModal preview={preview} onClose={closePreview} />}
    </Card>
  )
}
