import clsx from 'clsx'
import { useEffect } from 'react'
import type { FilePreviewState } from '@/shared/hooks/useFilePreview'
import { Button } from './Button'

/** Inline viewer for an already-fetched file (see useFilePreview) — images and PDFs render
 * directly; anything else (Word/Excel) falls back to a download link, since browsers can't
 * natively render those without extra server-side conversion this app doesn't do. */
export function FilePreviewModal({
  preview,
  onClose,
}: {
  preview: FilePreviewState
  onClose: () => void
}) {
  const isImage = preview.contentType.startsWith('image/')
  const isPdf = preview.contentType === 'application/pdf'

  useEffect(() => {
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKeyDown)
    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', onKeyDown)
    }
  }, [onClose])

  return (
    <>
      <button
        type="button"
        aria-label="إغلاق"
        className="fixed inset-0 z-40 cursor-default bg-black/40"
        onClick={onClose}
      />
      <div
        role="dialog"
        aria-modal="true"
        aria-label={preview.fileName}
        className="fixed inset-0 z-50 flex items-center justify-center p-4"
      >
        <div className="flex max-h-full w-full max-w-3xl flex-col gap-3 rounded-300 border border-border-primary bg-surface p-4 shadow-[0_0_32px_rgba(0,0,0,0.18)]">
          <div className="flex items-center justify-between gap-2">
            <span className="truncate text-sm font-medium text-content-primary">
              {preview.fileName}
            </span>
            <div className="flex shrink-0 gap-2">
              <a href={preview.url} download={preview.fileName}>
                <Button type="button" variant="secondary" size="sm">
                  تنزيل
                </Button>
              </a>
              <Button type="button" variant="secondary" size="sm" onClick={onClose}>
                إغلاق
              </Button>
            </div>
          </div>

          <div
            className={clsx(
              'flex min-h-[50vh] flex-1 items-center justify-center overflow-auto rounded-200 bg-surface-muted',
              !isImage && !isPdf && 'p-8 text-center',
            )}
          >
            {isImage && (
              <img src={preview.url} alt={preview.fileName} className="max-h-[75vh] max-w-full object-contain" />
            )}
            {isPdf && <iframe src={preview.url} title={preview.fileName} className="h-[75vh] w-full" />}
            {!isImage && !isPdf && (
              <p className="text-sm text-content-secondary">
                معاينة هذا النوع من الملفات مش متاحة هنا — دوس "تنزيل" عشان تفتحه.
              </p>
            )}
          </div>
        </div>
      </div>
    </>
  )
}
