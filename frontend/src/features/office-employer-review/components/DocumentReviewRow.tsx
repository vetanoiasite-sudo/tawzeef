import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ReasonForm } from './ReasonForm'
import type { OfficeEmployerDocument } from '../types/officeEmployerReview.types'
import { httpClient } from '@/shared/api/httpClient'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'

export function DocumentReviewRow({
  document,
  employerId,
  isApproving,
  isRejecting,
  rejectError,
  onApprove,
  onReject,
}: {
  document: OfficeEmployerDocument
  employerId: string
  isApproving: boolean
  isRejecting: boolean
  rejectError?: string
  onApprove: () => void
  onReject: (reason: string) => void
}) {
  const { t } = useTranslation('officeEmployerReview')
  const [showReject, setShowReject] = useState(false)
  const [downloading, setDownloading] = useState(false)

  async function handleDownload() {
    setDownloading(true)
    try {
      const { blob, fileName } = await httpClient.getBlob(
        `/api/office/employers/${employerId}/documents/${document.id}/download`,
      )
      const url = URL.createObjectURL(blob)
      const link = window.document.createElement('a')
      link.href = url
      link.download = fileName
      link.click()
      URL.revokeObjectURL(url)
    } finally {
      setDownloading(false)
    }
  }

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

      <Button variant="secondary" disabled={downloading} onClick={() => void handleDownload()} className="self-start">
        {t('downloadAction')}
      </Button>

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
