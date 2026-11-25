import { useRef, useState, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { TextInput } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'
import { useSupportedCurrencies } from '@/features/platform-settings'

export function PaymentConfirmForm({
  isSubmitting,
  error,
  onConfirm,
  onCancel,
}: {
  isSubmitting: boolean
  error?: string
  /** A receipt photo (invoice, InstaPay screenshot, whatever the payer sent) is required
   * alongside the amount/currency — confirmed 2026-11-25 product decision. */
  onConfirm: (amount: number, currency: string, receiptFile: File) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeJobAdsReview')
  const [amount, setAmount] = useState('')
  const [currency, setCurrency] = useState('')
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [receiptFile, setReceiptFile] = useState<File | null>(null)

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    setReceiptFile(e.target.files?.[0] ?? null)
  }
  // A dropdown (not free text) so the value is always one of the real platform.supported_currencies
  // setting, never whatever an office types by hand. No selection yet (still loading, or the
  // office hasn't touched it) falls back to the first real currency, computed at render time
  // rather than synced into state via an effect.
  const { data: currencies } = useSupportedCurrencies()
  const effectiveCurrency = currency || currencies?.[0] || ''

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
        value={effectiveCurrency}
        onChange={(e) => setCurrency(e.target.value)}
      >
        {(currencies ?? []).map((code) => (
          <option key={code} value={code}>
            {code}
          </option>
        ))}
      </Select>
      <div className="flex flex-col gap-1.5">
        <span className="text-[14px] font-bold text-content-primary">{t('paymentReceiptLabel')}</span>
        <input
          ref={fileInputRef}
          type="file"
          accept=".pdf,.jpg,.jpeg,.png,application/pdf,image/jpeg,image/png"
          className="hidden"
          onChange={handleFileChange}
        />
        <Button
          type="button"
          variant="secondary"
          className="self-start"
          onClick={() => fileInputRef.current?.click()}
        >
          {receiptFile ? t('replaceReceipt') : t('uploadReceipt')}
        </Button>
        <p className="text-xs text-text-muted">{receiptFile ? receiptFile.name : t('noReceiptUploaded')}</p>
      </div>
      {error && <p className="text-sm text-danger">{error}</p>}
      <div className="flex gap-2">
        <Button
          variant="primary"
          disabled={isSubmitting || !amount || Number(amount) <= 0 || !receiptFile}
          onClick={() => receiptFile && onConfirm(Number(amount), effectiveCurrency, receiptFile)}
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
