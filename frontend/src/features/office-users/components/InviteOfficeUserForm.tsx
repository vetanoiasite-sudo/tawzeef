import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { PermissionCheckboxList } from './PermissionCheckboxList'
import type { InviteOfficeUserPayload, OfficeUserRoleCode } from '../types/officeUser.types'

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
    <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-md border border-surface-muted p-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="fullName" className="text-sm font-medium text-text">
          {t('fullNameLabel')}
        </label>
        <input
          id="fullName"
          value={fullName}
          onChange={(e) => setFullName(e.target.value)}
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
        />
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="mobileNumber" className="text-sm font-medium text-text">
          {t('mobileNumberLabel')}
        </label>
        <input
          id="mobileNumber"
          type="tel"
          value={mobileNumber}
          onChange={(e) => setMobileNumber(e.target.value)}
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
        />
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="role" className="text-sm font-medium text-text">
          {t('roleLabel')}
        </label>
        <select
          id="role"
          value={role}
          onChange={(e) => setRole(e.target.value as OfficeUserRoleCode)}
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
        >
          <option value="OfficeStaff">{t('role.OfficeStaff')}</option>
          <option value="OfficeAdmin">{t('role.OfficeAdmin')}</option>
        </select>
      </div>

      {role === 'OfficeStaff' && (
        <div className="flex flex-col gap-2">
          <span className="text-sm font-medium text-text">{t('permissionsLabel')}</span>
          <PermissionCheckboxList selected={permissionCodes} onChange={setPermissionCodes} />
        </div>
      )}

      {error && <p className="text-sm text-danger">{error}</p>}

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={isSubmitting || !fullName.trim() || !mobileNumber.trim()}
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
        >
          {t('send')}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="rounded-md border border-surface-muted px-4 py-2 text-sm text-text"
        >
          {t('cancel')}
        </button>
      </div>
    </form>
  )
}
