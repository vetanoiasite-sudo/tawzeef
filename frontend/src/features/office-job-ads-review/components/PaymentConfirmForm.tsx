import { useState } from 'react'
import { useTranslation } from 'react-i18next'

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
    <div className="flex flex-col gap-2 rounded-md border border-primary/40 p-3">
      <label htmlFor="paymentAmount" className="text-sm font-medium text-text">
        {t('paymentAmountLabel')}
      </label>
      <input
        id="paymentAmount"
        type="number"
        min={0}
        step="0.01"
        value={amount}
        onChange={(e) => setAmount(e.target.value)}
        className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text outline-none focus:border-primary"
      />
      {error && <p className="text-sm text-danger">{error}</p>}
      <div className="flex gap-2">
        <button
          type="button"
          disabled={isSubmitting || !amount || Number(amount) <= 0}
          onClick={() => onConfirm(Number(amount), 'EGP')}
          className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
        >
          {t('confirmPaymentAction')}
        </button>
        <button type="button" onClick={onCancel} className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text">
          {t('cancel')}
        </button>
      </div>
    </div>
  )
}
