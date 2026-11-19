import { useRef, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import type { JobSeekerDocument, JobSeekerDocumentTypeCode } from '../types/jobSeekerProfile.types'

export function DocumentUploadCard({
  documentType,
  document,
  isUploading,
  error,
  onUpload,
}: {
  documentType: JobSeekerDocumentTypeCode
  document?: JobSeekerDocument
  isUploading: boolean
  error?: string
  onUpload: (file: File) => void
}) {
  const { t } = useTranslation('jobSeekerProfile')
  const inputRef = useRef<HTMLInputElement>(null)

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) onUpload(file)
    e.target.value = ''
  }

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-center justify-between gap-2">
        <span className="font-medium text-text">{t(`documentType.${documentType}`)}</span>
        {document && (
          <StatusTag
            domain="documentReview"
            value={document.reviewStatus}
            label={t(`documentStatus.${document.reviewStatus}`)}
          />
        )}
      </div>

      <p className="text-sm text-text-muted">{document ? document.originalFileName : t('noFileUploaded')}</p>
      {document?.rejectionReason && (
        <p className="text-sm text-danger">
          {t('rejectionReasonLabel')}: {document.rejectionReason}
        </p>
      )}

      <input
        ref={inputRef}
        type="file"
        accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
        className="hidden"
        onChange={handleFileChange}
      />
      <Button variant="secondary" disabled={isUploading} onClick={() => inputRef.current?.click()} className="self-start">
        {document ? t('replace') : t('upload')}
      </Button>
      <p className="text-xs text-text-muted">{t('uploadHint')}</p>
      {error && <p className="text-sm text-danger">{error}</p>}
    </Card>
  )
}
