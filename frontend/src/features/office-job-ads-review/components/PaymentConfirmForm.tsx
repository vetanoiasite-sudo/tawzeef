import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

export function PaymentConfirmForm({
  isSubmitting,
  error,
  onConfirm,
  onCancel,
}: {
  isSubmitting: boolean
  error?: string
  onConfirm: (amount: number, currency: string) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeJobAdsReview')
  const [amount, setAmount] = useState('')

  return (
    <div className="flex flex-col gap-3 rounded-md border border-primary/40 p-3">
      <TextInput
        id="paymentAmount"
        label={t('paymentAmountLabel')}
        type="number"
        min={0}
        step="0.01"
        value={amount}
        onChange={(e) => setAmount(e.target.value)}
      />
      {error && <p className="text-sm text-danger">{error}</p>}
      <div className="flex gap-2">
        <Button
          variant="primary"
          disabled={isSubmitting || !amount || Number(amount) <= 0}
          onClick={() => onConfirm(Number(amount), 'EGP')}
        >
          {t('confirmPaymentAction')}
        </Button>
        <Button variant="secondary" onClick={onCancel}>
          {t('cancel')}
        </Button>
      </div>
    </div>
  )
}
