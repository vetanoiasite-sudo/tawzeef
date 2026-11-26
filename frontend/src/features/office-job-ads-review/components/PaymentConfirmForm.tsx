import { useRef, useState, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'
import { useOfficeJobAdPackages } from '@/features/office-job-ad-packages'

export function PaymentConfirmForm({
  isSubmitting,
  error,
  onConfirm,
  onCancel,
}: {
  isSubmitting: boolean
  error?: string
  /** A receipt photo (invoice, InstaPay screenshot, whatever the payer sent) is required
   * alongside the chosen package — confirmed 2026-11-25 product decision, package replaced the
   * free-typed amount/currency per peppy-greeting-lighthouse.md's plan. */
  onConfirm: (jobAdvertisementPackageId: string, receiptFile: File) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeJobAdsReview')
  const { data: packages } = useOfficeJobAdPackages()
  const activePackages = (packages ?? []).filter((p) => p.isActive)
  const [packageId, setPackageId] = useState('')
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [receiptFile, setReceiptFile] = useState<File | null>(null)

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    setReceiptFile(e.target.files?.[0] ?? null)
  }
  // No selection yet falls back to the first active package, computed at render time rather
  // than synced into state via an effect — same pattern this form used for currency before.
  const effectivePackageId = packageId || activePackages[0]?.id || ''

  return (
    <div className="flex flex-col gap-3 rounded-md border border-primary/40 p-3">
      {activePackages.length === 0 ? (
        <p className="text-sm text-danger">{t('noActivePackages')}</p>
      ) : (
        <Select
          id="paymentPackage"
          label={t('paymentPackageLabel')}
          value={effectivePackageId}
          onChange={(e) => setPackageId(e.target.value)}
        >
          {activePackages.map((pkg) => (
            <option key={pkg.id} value={pkg.id}>
              {pkg.name} — {pkg.price} {pkg.currency} / {pkg.durationDays} يوم /{' '}
              {pkg.cvViewLimit ?? 'غير محدود'} CV
            </option>
          ))}
        </Select>
      )}
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
          disabled={isSubmitting || !effectivePackageId || !receiptFile}
          onClick={() => receiptFile && onConfirm(effectivePackageId, receiptFile)}
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
