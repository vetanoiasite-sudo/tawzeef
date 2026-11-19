import { useTranslation } from 'react-i18next'
import { Button } from '@/shared/ui/Button'

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
        <Button variant="success" disabled={isSubmitting} onClick={() => onDecide('Valid')}>
          {t('markValid')}
        </Button>
        <Button variant="danger" disabled={isSubmitting} onClick={() => onDecide('Invalid')}>
          {t('markInvalid')}
        </Button>
      </div>
      {error && <p className="text-sm text-danger">{error}</p>}
    </div>
  )
}
