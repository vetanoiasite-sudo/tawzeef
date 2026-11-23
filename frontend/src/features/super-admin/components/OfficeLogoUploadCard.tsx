import { useRef, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { env } from '@/shared/config/env'

/** Compact inline logo avatar + upload trigger — sits directly in the office row (unlike the
 * employer's LogoUploadCard, which gets a whole Card of its own on the employer's profile page;
 * the SuperAdmin offices list is already a row-per-office layout, so a full card per row would
 * be too tall). */
export function OfficeLogoUploadCard({
  logoUrl,
  isUploading,
  onUpload,
}: {
  logoUrl: string | null
  isUploading: boolean
  onUpload: (file: File) => void
}) {
  const { t } = useTranslation('superAdmin')
  const inputRef = useRef<HTMLInputElement>(null)

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) onUpload(file)
    e.target.value = ''
  }

  return (
    <button
      type="button"
      disabled={isUploading}
      onClick={() => inputRef.current?.click()}
      title={logoUrl ? t('replaceLogoAction') : t('uploadLogoAction')}
      className="group relative flex size-12 shrink-0 items-center justify-center overflow-hidden rounded-200 border border-border-primary bg-surface-muted transition-colors hover:border-brand disabled:cursor-not-allowed disabled:opacity-60"
    >
      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png"
        className="hidden"
        onChange={handleFileChange}
      />
      {logoUrl ? (
        <img
          src={`${env.apiBaseUrl}${logoUrl}`}
          alt={t('officeLogoLabel')}
          className="size-full object-contain"
        />
      ) : (
        <BuildingPlaceholderIcon />
      )}
      <span className="pointer-events-none absolute inset-0 flex items-center justify-center bg-black/40 text-white opacity-0 transition-opacity group-hover:opacity-100">
        <CameraIcon />
      </span>
    </button>
  )
}

function BuildingPlaceholderIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden className="text-text-muted">
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

function CameraIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M4 8h3l1.5-2h7L17 8h3a1 1 0 0 1 1 1v9a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V9a1 1 0 0 1 1-1Z"
        stroke="currentColor"
        strokeWidth="1.6"
        strokeLinejoin="round"
      />
      <circle cx="12" cy="13" r="3.2" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  )
}
