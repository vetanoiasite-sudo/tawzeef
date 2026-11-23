import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { PermissionCheckboxList } from './PermissionCheckboxList'
import type { InviteOfficeUserPayload, OfficeUserRoleCode } from '../types/officeUser.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'

export function InviteOfficeUserForm({
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: {
  isSubmitting: boolean
  error?: string
  onSubmit: (payload: InviteOfficeUserPayload) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeUsers')
  const [fullName, setFullName] = useState('')
  const [mobileNumber, setMobileNumber] = useState('')
  const [role, setRole] = useState<OfficeUserRoleCode>('OfficeStaff')
  const [permissionCodes, setPermissionCodes] = useState<string[]>([])

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    onSubmit({
      fullName: fullName.trim(),
      mobileNumber: mobileNumber.trim(),
      role,
      permissionCodes: role === 'OfficeStaff' ? permissionCodes : [],
    })
  }

  return (
    <form onSubmit={handleSubmit}>
      <Card className="flex flex-col gap-4">
        <TextInput
          id="fullName"
          label={t('fullNameLabel')}
          value={fullName}
          onChange={(e) => setFullName(e.target.value)}
        />

        <TextInput
          id="mobileNumber"
          type="tel"
          label={t('mobileNumberLabel')}
          value={mobileNumber}
          onChange={(e) => setMobileNumber(e.target.value)}
        />

        <Select
          id="role"
          label={t('roleLabel')}
          value={role}
          onChange={(e) => setRole(e.target.value as OfficeUserRoleCode)}
        >
          <option value="OfficeStaff">{t('role.OfficeStaff')}</option>
          <option value="OfficeAdmin">{t('role.OfficeAdmin')}</option>
        </Select>

        {role === 'OfficeStaff' && (
          <div className="flex flex-col gap-2">
            <span className="text-sm font-medium text-text">{t('permissionsLabel')}</span>
            <PermissionCheckboxList selected={permissionCodes} onChange={setPermissionCodes} />
          </div>
        )}

        {error && <p className="text-sm text-danger">{error}</p>}

        <div className="flex gap-2">
          <Button
            type="submit"
            variant="primary"
            disabled={isSubmitting || !fullName.trim() || !mobileNumber.trim()}
          >
            {t('send')}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel}>
            {t('cancel')}
          </Button>
        </div>
      </Card>
    </form>
  )
}
