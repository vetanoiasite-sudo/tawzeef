import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Button } from '@/shared/ui/Button'
import { TextInput } from '@/shared/ui/TextInput'
import type {
  JobAdvertisementPackage,
  JobAdvertisementPackagePayload,
} from '../types/officeJobAdPackages.types'

export function JobAdPackageEditForm({
  package: pkg,
  isSubmitting,
  error,
  onSave,
  onCancel,
}: {
  package: JobAdvertisementPackage
  isSubmitting: boolean
  error?: string
  onSave: (payload: JobAdvertisementPackagePayload) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeJobAdPackages')
  const [name, setName] = useState(pkg.name)
  const [price, setPrice] = useState(String(pkg.price))
  const [currency, setCurrency] = useState(pkg.currency)
  const [cvViewLimit, setCvViewLimit] = useState(pkg.cvViewLimit === null ? '' : String(pkg.cvViewLimit))
  const [durationDays, setDurationDays] = useState(String(pkg.durationDays))

  function handleSave() {
    onSave({
      name: name.trim(),
      level: pkg.level,
      price: Number(price),
      currency: currency.trim().toUpperCase(),
      cvViewLimit: cvViewLimit.trim() === '' ? null : Number(cvViewLimit),
      durationDays: Number(durationDays),
    })
  }

  const isValid =
    name.trim().length > 0 && Number(price) > 0 && currency.trim().length > 0 && Number(durationDays) > 0

  return (
    <div className="flex flex-col gap-3 border-t border-surface-muted pt-3">
      <TextInput id={`name-${pkg.id}`} label={t('nameLabel')} value={name} onChange={(e) => setName(e.target.value)} />
      <div className="grid grid-cols-2 gap-3">
        <TextInput
          id={`price-${pkg.id}`}
          label={t('priceLabel')}
          type="number"
          min={0}
          step="0.01"
          value={price}
          onChange={(e) => setPrice(e.target.value)}
        />
        <TextInput
          id={`currency-${pkg.id}`}
          label={t('currencyLabel')}
          maxLength={3}
          value={currency}
          onChange={(e) => setCurrency(e.target.value)}
        />
        <TextInput
          id={`cvViewLimit-${pkg.id}`}
          label={t('cvViewLimitLabel')}
          type="number"
          min={1}
          placeholder={t('unlimitedCv')}
          value={cvViewLimit}
          onChange={(e) => setCvViewLimit(e.target.value)}
        />
        <TextInput
          id={`durationDays-${pkg.id}`}
          label={t('durationDaysLabel')}
          type="number"
          min={1}
          value={durationDays}
          onChange={(e) => setDurationDays(e.target.value)}
        />
      </div>
      {error && <p className="text-sm text-danger">{error}</p>}
      <div className="flex gap-2">
        <Button variant="primary" disabled={isSubmitting || !isValid} onClick={handleSave}>
          {t('saveChanges')}
        </Button>
        <Button variant="secondary" onClick={onCancel}>
          {t('cancel')}
        </Button>
      </div>
    </div>
  )
}
