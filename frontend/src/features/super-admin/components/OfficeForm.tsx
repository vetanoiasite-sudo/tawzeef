import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { EGYPT_GOVERNORATES } from '@/shared/constants/governorates'
import type { CreateOfficePayload, Office, UpdateOfficePayload } from '../types/superAdmin.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'

/** Shared by both create and edit — edit passes `office` and the code field becomes read-only
 * (it's the tenant key other records link to, so it's immutable once set). */
export function OfficeForm({
  office,
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: {
  office?: Office
  isSubmitting: boolean
  error?: string
  onSubmit: (payload: CreateOfficePayload | UpdateOfficePayload) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('superAdmin')
  const [name, setName] = useState(office?.name ?? '')
  const [arName, setArName] = useState(office?.arName ?? '')
  const [code, setCode] = useState(office?.code ?? '')
  const [address, setAddress] = useState(office?.address ?? '')
  const [city, setCity] = useState(office?.city ?? '')
  const [phone, setPhone] = useState(office?.phone ?? '')
  const [email, setEmail] = useState(office?.email ?? '')

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const base = {
      name: name.trim(),
      arName: arName.trim() || undefined,
      address: address.trim() || undefined,
      city: city || undefined,
      phone: phone.trim() || undefined,
      email: email.trim() || undefined,
    }
    onSubmit(office ? base : { ...base, code: code.trim() })
  }

  return (
    <form onSubmit={handleSubmit}>
      <Card className="flex flex-col gap-4">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <TextInput id="officeName" label={t('nameLabel')} value={name} onChange={(e) => setName(e.target.value)} />
          <TextInput id="officeArName" label={t('arNameLabel')} value={arName} onChange={(e) => setArName(e.target.value)} />
        </div>

        <TextInput
          id="officeCode"
          label={t('codeLabel')}
          helperText={office ? t('codeHelp') : undefined}
          value={code}
          disabled={Boolean(office)}
          onChange={(e) => setCode(e.target.value)}
        />

        <TextInput id="officeAddress" label={t('addressLabel')} value={address} onChange={(e) => setAddress(e.target.value)} />

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Select id="officeCity" label={t('cityLabel')} value={city} onChange={(e) => setCity(e.target.value)}>
            <option value="">—</option>
            {EGYPT_GOVERNORATES.map((governorate) => (
              <option key={governorate} value={governorate}>
                {governorate}
              </option>
            ))}
          </Select>
          <TextInput id="officePhone" type="tel" label={t('phoneLabel')} value={phone} onChange={(e) => setPhone(e.target.value)} />
        </div>

        <TextInput id="officeEmail" type="email" label={t('emailLabel')} value={email} onChange={(e) => setEmail(e.target.value)} />

        {error && <p className="text-sm text-danger">{error}</p>}

        <div className="flex gap-2">
          <Button type="submit" variant="primary" disabled={isSubmitting || !name.trim() || (!office && !code.trim())}>
            {office ? t('saveChanges') : t('createOffice')}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel}>
            {t('cancel')}
          </Button>
        </div>
      </Card>
    </form>
  )
}
