import { useRef, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { env } from '@/shared/config/env'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'

export function OfficeLogoUploadCard({
  logoUrl,
  isUploading,
  error,
  onUpload,
}: {
  logoUrl: string | null
  isUploading: boolean
  error?: string
  onUpload: (file: File) => void
}) {
  const { t } = useTranslation('officeProfile')
  const inputRef = useRef<HTMLInputElement>(null)

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) onUpload(file)
    e.target.value = ''
  }

  return (
    <Card className="flex flex-col gap-3">
      <span className="font-medium text-text">{t('logoLabel')}</span>

      <div className="flex items-center gap-4">
        <div className="flex size-16 shrink-0 items-center justify-center overflow-hidden rounded-200 border border-border-primary bg-surface-muted">
          {logoUrl ? (
            <img
              src={`${env.apiBaseUrl}${logoUrl}`}
              alt={t('logoLabel')}
              className="size-full object-contain"
            />
          ) : (
            <BuildingPlaceholderIcon />
          )}
        </div>

        <div className="flex flex-col gap-2">
          <input
            ref={inputRef}
            type="file"
            accept="image/jpeg,image/png"
            className="hidden"
            onChange={handleFileChange}
          />
          <Button
            variant="secondary"
            disabled={isUploading}
            onClick={() => inputRef.current?.click()}
            className="self-start"
          >
            {logoUrl ? t('replaceLogo') : t('uploadLogo')}
          </Button>
          <p className="text-xs text-text-muted">{t('logoUploadHint')}</p>
        </div>
      </div>

      {error && <p className="text-sm text-danger">{error}</p>}
    </Card>
  )
}

function BuildingPlaceholderIcon() {
  return (
    <svg width="28" height="28" viewBox="0 0 24 24" fill="none" aria-hidden className="text-text-muted">
      <rect x="4" y="3" width="16" height="18" rx="1" stroke="currentColor" strokeWidth="1.6" />
      <path
        d="M8 8h2M14 8h2M8 12h2M14 12h2M8 16h2M14 16h2"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinecap="round"
      />
    </svg>
  )
}
