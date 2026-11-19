import { useState } from 'react'
import { useTranslation } from 'react-i18next'

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
    <div className="flex flex-col gap-2">
      <h3 className="text-sm font-semibold text-text">{t('resolveSection')}</h3>

      {!outcome && (
        <div className="flex gap-2">
          <button
            type="button"
            onClick={() => setOutcome('Resolved')}
            className="rounded-md bg-success px-3 py-1.5 text-sm font-medium text-surface"
          >
            {t('resolveAsResolved')}
          </button>
          <button
            type="button"
            onClick={() => setOutcome('Rejected')}
            className="rounded-md border border-danger px-3 py-1.5 text-sm font-medium text-danger"
          >
            {t('resolveAsRejected')}
          </button>
        </div>
      )}

      {outcome && (
        <div className="flex flex-col gap-2 rounded-md border border-surface-muted p-3">
          <p className="text-sm text-text">{outcome === 'Resolved' ? t('resolveAsResolved') : t('resolveAsRejected')}</p>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder={t('resolveNotesPlaceholder')}
            rows={2}
            className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text outline-none focus:border-primary"
          />
          {error && <p className="text-sm text-danger">{error}</p>}
          <div className="flex gap-2">
            <button
              type="button"
              disabled={isSubmitting || !notes.trim()}
              onClick={() => onResolve(outcome, notes.trim())}
              className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
            >
              {t('confirm')}
            </button>
            <button
              type="button"
              onClick={() => setOutcome(null)}
              className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text"
            >
              {t('cancel')}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
