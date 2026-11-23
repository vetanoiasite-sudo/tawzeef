import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { TextInput } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'

// Single-currency platform for now — a dropdown (not free text) so the value is always one of a
// known, valid set rather than whatever an office types by hand.
const SUPPORTED_CURRENCIES = ['EGP'] as const

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
  const [currency, setCurrency] = useState<string>(SUPPORTED_CURRENCIES[0])

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
      <Select
        id="paymentCurrency"
        label={t('paymentCurrencyLabel')}
        value={currency}
        onChange={(e) => setCurrency(e.target.value)}
      >
        {SUPPORTED_CURRENCIES.map((code) => (
          <option key={code} value={code}>
            {code}
          </option>
        ))}
      </Select>
      {error && <p className="text-sm text-danger">{error}</p>}
      <div className="flex gap-2">
        <Button
          variant="primary"
          disabled={isSubmitting || !amount || Number(amount) <= 0}
          onClick={() => onConfirm(Number(amount), currency)}
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
