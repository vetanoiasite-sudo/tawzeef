import { useEffect, useRef, useState, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { jobSeekerProfileApi } from '../api/jobSeekerProfile.api'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'

/** Optional, private — never a public URL (unlike a company logo). Fetched through the same
 * authenticated-blob pattern used for CV downloads elsewhere in this app. */
export function PhotoUploadCard({
  hasPhoto,
  isUploading,
  error,
  onUpload,
}: {
  hasPhoto: boolean
  isUploading: boolean
  error?: string
  onUpload: (file: File) => void
}) {
  const { t } = useTranslation('jobSeekerProfile')
  const inputRef = useRef<HTMLInputElement>(null)
  const [photoUrl, setPhotoUrl] = useState<string | null>(null)

  useEffect(() => {
    if (!hasPhoto) {
      setPhotoUrl(null)
      return
    }
    let objectUrl: string | null = null
    let cancelled = false
    void jobSeekerProfileApi.getPhoto().then(({ blob }) => {
      if (cancelled) return
      objectUrl = URL.createObjectURL(blob)
      setPhotoUrl(objectUrl)
    })
    return () => {
      cancelled = true
      if (objectUrl) URL.revokeObjectURL(objectUrl)
    }
  }, [hasPhoto])

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) onUpload(file)
    e.target.value = ''
  }

  return (
    <Card className="flex flex-col gap-3">
      <span className="font-medium text-content-primary">{t('photoSection')}</span>

      <div className="flex items-center gap-4">
        <div className="flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-full border border-border-primary bg-surface-muted">
          {photoUrl ? (
            <img src={photoUrl} alt="" className="size-full object-cover" />
          ) : (
            <PersonIcon />
          )}
        </div>

        <div className="flex flex-col gap-2">
          {!photoUrl && <p className="text-sm text-content-tertiary">{t('noPhotoUploaded')}</p>}
          <input
            ref={inputRef}
            type="file"
            accept=".jpg,.jpeg,.png,image/jpeg,image/png"
            className="hidden"
            onChange={handleFileChange}
          />
          <Button
            variant="secondary"
            size="sm"
            disabled={isUploading}
            onClick={() => inputRef.current?.click()}
            className="self-start"
          >
            {hasPhoto ? t('replacePhoto') : t('uploadPhoto')}
          </Button>
        </div>
      </div>

      <p className="text-xs text-content-tertiary">{t('photoHint')}</p>
      {error && <p className="text-sm text-danger">{error}</p>}
    </Card>
  )
}

function PersonIcon() {
  return (
    <svg width="32" height="32" viewBox="0 0 24 24" fill="none" aria-hidden className="text-content-tertiary">
      <circle cx="12" cy="8" r="4" stroke="currentColor" strokeWidth="1.8" />
      <path d="M4 20c0-3.3 3.6-6 8-6s8 2.7 8 6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}
