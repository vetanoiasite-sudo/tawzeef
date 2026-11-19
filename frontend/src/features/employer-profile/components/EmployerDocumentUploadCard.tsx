import clsx from 'clsx'
import { useRef, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { EmployerDocument, EmployerDocumentTypeCode } from '../types/employerProfile.types'

const STATUS_TONE: Record<string, string> = {
  Pending: 'text-warning',
  Approved: 'text-success',
  Rejected: 'text-danger',
}

export function EmployerDocumentUploadCard({
  documentType,
  document,
  isUploading,
  error,
  onUpload,
}: {
  documentType: EmployerDocumentTypeCode
  document?: EmployerDocument
  isUploading: boolean
  error?: string
  onUpload: (file: File) => void
}) {
  const { t } = useTranslation('employerProfile')
  const inputRef = useRef<HTMLInputElement>(null)

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) onUpload(file)
    e.target.value = ''
  }

  return (
    <div className="flex flex-col gap-2 rounded-md border border-surface-muted p-4">
      <div className="flex items-center justify-between gap-2">
        <span className="font-medium text-text">{t(`documentType.${documentType}`)}</span>
        {document && (
          <span className={clsx('text-sm font-medium', STATUS_TONE[document.reviewStatus])}>
            {t(`documentStatus.${document.reviewStatus}`)}
          </span>
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
        accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
        className="hidden"
        onChange={handleFileChange}
      />
      <button
        type="button"
        disabled={isUploading}
        onClick={() => inputRef.current?.click()}
        className="self-start rounded-md border border-primary px-3 py-1.5 text-sm font-medium text-primary disabled:opacity-60"
      >
        {document ? t('replace') : t('upload')}
      </button>
      <p className="text-xs text-text-muted">{t('uploadHint')}</p>
      {error && <p className="text-sm text-danger">{error}</p>}
    </div>
  )
}
