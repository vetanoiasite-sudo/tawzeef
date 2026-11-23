import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/shared/ui/Button'
import { TextArea } from '@/shared/ui/TextInput'
import { Card } from '@/shared/ui/Card'

export function ResolveControl({
  isSubmitting,
  error,
  onResolve,
}: {
  isSubmitting: boolean
  error?: string
  onResolve: (outcome: 'Resolved' | 'Rejected', resolutionNotes: string) => void
}) {
  const { t } = useTranslation('officeComplaintReview')
  const [outcome, setOutcome] = useState<'Resolved' | 'Rejected' | null>(null)
  const [notes, setNotes] = useState('')

  return (
    <div className="flex flex-col gap-3">
      <h3 className="text-sm font-semibold text-text">{t('resolveSection')}</h3>

      {!outcome && (
        <div className="flex gap-2">
          <Button variant="success" onClick={() => setOutcome('Resolved')}>
            {t('resolveAsResolved')}
          </Button>
          <Button variant="danger" onClick={() => setOutcome('Rejected')}>
            {t('resolveAsRejected')}
          </Button>
        </div>
      )}

      {outcome && (
        <Card className="flex flex-col gap-3">
          <p className="text-sm text-text">
            {outcome === 'Resolved' ? t('resolveAsResolved') : t('resolveAsRejected')}
          </p>
          <TextArea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder={t('resolveNotesPlaceholder')}
            rows={2}
          />
          {error && <p className="text-sm text-danger">{error}</p>}
          <div className="flex gap-2">
            <Button
              variant="primary"
              disabled={isSubmitting || !notes.trim()}
              onClick={() => onResolve(outcome, notes.trim())}
            >
              {t('confirm')}
            </Button>
            <Button variant="secondary" onClick={() => setOutcome(null)}>
              {t('cancel')}
            </Button>
          </div>
        </Card>
      )}
    </div>
  )
}
