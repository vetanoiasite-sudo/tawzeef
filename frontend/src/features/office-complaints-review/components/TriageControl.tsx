import { useTranslation } from 'react-i18next'

export function TriageControl({
  isSubmitting,
  error,
  onDecide,
}: {
  isSubmitting: boolean
  error?: string
  onDecide: (validityDecision: 'Valid' | 'Invalid') => void
}) {
  const { t } = useTranslation('officeComplaintReview')

  return (
    <div className="flex flex-col gap-2">
      <h3 className="text-sm font-semibold text-text">{t('triageSection')}</h3>
      <div className="flex gap-2">
        <button
          type="button"
          disabled={isSubmitting}
          onClick={() => onDecide('Valid')}
          className="rounded-md bg-success px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
        >
          {t('markValid')}
        </button>
        <button
          type="button"
          disabled={isSubmitting}
          onClick={() => onDecide('Invalid')}
          className="rounded-md border border-danger px-3 py-1.5 text-sm font-medium text-danger disabled:opacity-60"
        >
          {t('markInvalid')}
        </button>
      </div>
      {error && <p className="text-sm text-danger">{error}</p>}
    </div>
  )
}
