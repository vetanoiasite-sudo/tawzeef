import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { PermissionCheckboxList } from './PermissionCheckboxList'
import type { OfficeUser } from '../types/officeUser.types'

export function OfficeUserRow({
  user,
  isSavingPermissions,
  permissionsError,
  isRevoking,
  onSavePermissions,
  onRevoke,
}: {
  user: OfficeUser
  isSavingPermissions: boolean
  permissionsError?: string
  isRevoking: boolean
  onSavePermissions: (permissionCodes: string[]) => void
  onRevoke: () => void
}) {
  const { t } = useTranslation('officeUsers')
  const [editing, setEditing] = useState(false)
  const [draftPermissions, setDraftPermissions] = useState<string[]>(user.permissionCodes)

  return (
    <div className="flex flex-col gap-2 rounded-md border border-surface-muted p-4">
      <div className="flex items-center justify-between gap-2">
        <div>
          <p className="font-medium text-text">{user.fullName}</p>
          <p className="text-sm text-text-muted">{user.mobileNumber}</p>
        </div>
        <div className="text-end text-sm">
          <p className="text-text">{t(`role.${user.role}`)}</p>
          <p className="text-text-muted">{t(`status.${user.membershipStatus}`)}</p>
        </div>
      </div>

      {user.role === 'OfficeStaff' && !editing && (
        <p className="text-sm text-text-muted">
          {user.permissionCodes.length > 0
            ? user.permissionCodes.map((c) => t(`permission.${c}`)).join('، ')
            : '—'}
        </p>
      )}

      {user.membershipStatus === 'Active' && (
        <div className="flex flex-wrap gap-2">
          {user.role === 'OfficeStaff' && !editing && (
            <button
              type="button"
              onClick={() => {
                setDraftPermissions(user.permissionCodes)
                setEditing(true)
              }}
              className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text"
            >
              {t('editPermissions')}
            </button>
          )}
          <button
            type="button"
            disabled={isRevoking}
            onClick={() => {
              if (window.confirm(t('revokeConfirm'))) onRevoke()
            }}
            className="rounded-md border border-danger px-3 py-1.5 text-sm font-medium text-danger disabled:opacity-60"
          >
            {t('revoke')}
          </button>
        </div>
      )}

      {editing && (
        <div className="flex flex-col gap-2">
          <PermissionCheckboxList selected={draftPermissions} onChange={setDraftPermissions} />
          {permissionsError && <p className="text-sm text-danger">{permissionsError}</p>}
          <div className="flex gap-2">
            <button
              type="button"
              disabled={isSavingPermissions}
              onClick={() => {
                onSavePermissions(draftPermissions)
                setEditing(false)
              }}
              className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
            >
              {t('saveChanges')}
            </button>
            <button
              type="button"
              onClick={() => setEditing(false)}
              className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text"
            >
              {t('cancel')}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
