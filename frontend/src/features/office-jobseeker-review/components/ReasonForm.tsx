import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { TextArea } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

export function ReasonForm({
  confirmLabel,
  confirmTone = 'danger',
  isSubmitting,
  error,
  onConfirm,
  onCancel,
}: {
  confirmLabel: string
  confirmTone?: 'danger' | 'primary'
  isSubmitting: boolean
  error?: string
  onConfirm: (reason: string) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeJobSeekerReview')
  const [reason, setReason] = useState('')

  return (
    <div className="flex flex-col gap-2 rounded-md border border-danger/40 p-3">
      <TextArea
        value={reason}
        onChange={(e) => setReason(e.target.value)}
        placeholder={t('reasonPlaceholder')}
        rows={2}
      />
      {error && <p className="text-sm text-danger">{error}</p>}
      <div className="flex gap-2">
        <Button
          disabled={isSubmitting || !reason.trim()}
          onClick={() => onConfirm(reason.trim())}
          className={confirmTone === 'danger' ? 'bg-danger text-surface hover:bg-danger/90' : ''}
        >
          {confirmLabel}
        </Button>
        <Button variant="secondary" onClick={onCancel}>
          {t('cancel')}
        </Button>
      </div>
    </div>
  )
}
