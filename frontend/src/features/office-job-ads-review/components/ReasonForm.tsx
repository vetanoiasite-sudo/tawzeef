import { useState } from 'react'
import { useTranslation } from 'react-i18next'

export function ReasonForm({
  confirmLabel,
  isSubmitting,
  error,
  onConfirm,
  onCancel,
}: {
  confirmLabel: string
  isSubmitting: boolean
  error?: string
  onConfirm: (reason: string) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeJobAdsReview')
  const [reason, setReason] = useState('')

  return (
    <div className="flex flex-col gap-2 rounded-md border border-danger/40 p-3">
      <textarea
        value={reason}
        onChange={(e) => setReason(e.target.value)}
        placeholder={t('reasonPlaceholder')}
        rows={2}
        className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text outline-none focus:border-danger"
      />
      {error && <p className="text-sm text-danger">{error}</p>}
      <div className="flex gap-2">
        <button
          type="button"
          disabled={isSubmitting || !reason.trim()}
          onClick={() => onConfirm(reason.trim())}
          className="rounded-md bg-danger px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
        >
          {confirmLabel}
        </button>
        <button type="button" onClick={onCancel} className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text">
          {t('cancel')}
        </button>
      </div>
    </div>
  )
}
