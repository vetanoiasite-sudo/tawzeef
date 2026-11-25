import { useRef, useState, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useFilePreview } from '@/shared/hooks/useFilePreview'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { FilePreviewModal } from '@/shared/ui/FilePreviewModal'
import type { JobSeekerDocument, JobSeekerDocumentTypeCode } from '../types/jobSeekerProfile.types'

export function DocumentUploadCard({
  documentType,
  document,
  isUploading,
  error,
  justSucceeded,
  onUpload,
}: {
  documentType: JobSeekerDocumentTypeCode
  document?: JobSeekerDocument
  isUploading: boolean
  error?: string
  /** True once this card's own upload mutation has most recently succeeded (stays true until
   * another upload of this type starts) — drives the "replaced, pending review" confirmation. */
  justSucceeded?: boolean
  onUpload: (file: File) => void
}) {
  const { t } = useTranslation('jobSeekerProfile')
  const inputRef = useRef<HTMLInputElement>(null)
  const { preview, isLoading: isLoadingPreview, open: openPreview, close: closePreview } = useFilePreview()
  // Whether a document was already sitting in this slot right when the file picker opened — set
  // before onUpload fires, so it still reflects "replace vs first upload" after the mutation
  // refetches and `document` becomes the new one.
  const [wasReplace, setWasReplace] = useState(false)

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) {
      setWasReplace(Boolean(document))
      onUpload(file)
    }
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

      <p className="text-sm text-text-muted">
        {document ? document.originalFileName : t('noFileUploaded')}
      </p>
      {document?.rejectionReason && (
        <p className="text-sm text-danger">
          {t('rejectionReasonLabel')}: {document.rejectionReason}
        </p>
      )}

      <input
        ref={inputRef}
        type="file"
        accept=".pdf,.jpg,.jpeg,.png,.xlsx,.xls,.doc,.docx,application/pdf,image/jpeg,image/png,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        className="hidden"
        onChange={handleFileChange}
      />
      <div className="flex flex-wrap gap-2">
        <Button
          variant="secondary"
          disabled={isUploading}
          onClick={() => inputRef.current?.click()}
          className="self-start"
        >
          {document ? t('replace') : t('upload')}
        </Button>
        {document && (
          <Button
            variant="secondary"
            disabled={isLoadingPreview}
            onClick={() => void openPreview(`/api/jobseekers/me/documents/${document.id}`)}
          >
            {t('viewFile')}
          </Button>
        )}
      </div>
      <p className="text-xs text-text-muted">{t('uploadHint')}</p>
      {error && <p className="text-sm text-danger">{error}</p>}
      {justSucceeded && !isUploading && !error && (
        <div className="rounded-md border border-warning/30 bg-warning/10 px-3 py-2">
          <p className="text-sm font-medium text-warning">
            {wasReplace ? t('replacementPendingBadge') : t('uploadPendingBadge')}
          </p>
          <p className="mt-1 text-xs text-text-muted">{t('replacementBlocksApplyNote')}</p>
        </div>
      )}

      {preview && <FilePreviewModal preview={preview} onClose={closePreview} />}
    </Card>
  )
}
