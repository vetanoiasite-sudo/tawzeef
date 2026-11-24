import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { OfficeEmployerDetail, UpdateEmployerPayload } from '../types/officeEmployerReview.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { useGovernorates } from '@/features/governorates'

/** Office-side correction of a company profile's own fields — a quick-fix tool for a typo the
 * employer got wrong, plain useState like OfficeJobAdEditForm.tsx's sibling pattern. */
export function OfficeEmployerEditForm({
  employer,
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: {
  employer: OfficeEmployerDetail
  isSubmitting: boolean
  error?: string
  onSubmit: (payload: UpdateEmployerPayload) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeEmployerReview')
  const { data: governorates } = useGovernorates()
  const [businessActivity, setBusinessActivity] = useState(employer.businessActivity ?? '')
  const [address, setAddress] = useState(employer.address ?? '')
  const [city, setCity] = useState(employer.city ?? '')
  const [companyEmail, setCompanyEmail] = useState(employer.companyEmail ?? '')
  const [companyPhone, setCompanyPhone] = useState(employer.companyPhone ?? '')
  const [representativeName, setRepresentativeName] = useState(employer.representativeName ?? '')
  const [representativeRole, setRepresentativeRole] = useState(employer.representativeRole ?? '')

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    onSubmit({
      businessActivity: businessActivity.trim() || undefined,
      address: address.trim() || undefined,
      city: city || undefined,
      companyEmail: companyEmail.trim() || undefined,
      companyPhone: companyPhone.trim() || undefined,
      representativeName: representativeName.trim() || undefined,
      representativeRole: representativeRole.trim() || undefined,
    })
  }

  return (
    <form onSubmit={handleSubmit}>
      <Card className="flex flex-col gap-4">
        <p className="text-sm text-content-secondary">{t('editProfileHint')}</p>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <TextInput
            id="editRepresentativeName"
            label={t('representativeLabel')}
            value={representativeName}
            onChange={(e) => setRepresentativeName(e.target.value)}
          />

          <TextInput
            id="editRepresentativeRole"
            label={t('representativeRoleLabel')}
            value={representativeRole}
            onChange={(e) => setRepresentativeRole(e.target.value)}
          />

          <Select id="editCity" label={t('cityLabel')} value={city} onChange={(e) => setCity(e.target.value)}>
            <option value="">—</option>
            {(governorates ?? []).map((g) => (
              <option key={g.id} value={g.name}>
                {g.name}
              </option>
            ))}
          </Select>

          <TextInput
            id="editCompanyPhone"
            type="tel"
            label={t('phoneLabel')}
            value={companyPhone}
            onChange={(e) => setCompanyPhone(e.target.value)}
          />

          <TextInput
            id="editCompanyEmail"
            type="email"
            label={t('emailLabel')}
            value={companyEmail}
            onChange={(e) => setCompanyEmail(e.target.value)}
          />

          <TextInput
            id="editAddress"
            label={t('addressLabel')}
            value={address}
            onChange={(e) => setAddress(e.target.value)}
          />

          <div className="sm:col-span-2">
            <TextInput
              id="editBusinessActivity"
              label={t('businessActivityLabel')}
              value={businessActivity}
              onChange={(e) => setBusinessActivity(e.target.value)}
            />
          </div>
        </div>

        {error && <p className="text-sm text-danger">{error}</p>}

        <div className="flex gap-2">
          <Button type="submit" variant="primary" disabled={isSubmitting}>
            {t('saveChanges')}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel}>
            {t('cancel')}
          </Button>
        </div>
      </Card>
    </form>
  )
}
