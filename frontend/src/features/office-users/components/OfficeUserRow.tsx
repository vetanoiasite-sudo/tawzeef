import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { PermissionCheckboxList } from './PermissionCheckboxList'
import type { OfficeUser } from '../types/officeUser.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'

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
    <Card className="flex flex-col gap-3">
      <div className="flex items-center justify-between gap-2">
        <div>
          <p className="font-medium text-text">{user.fullName}</p>
          <p className="text-sm text-text-muted">{user.mobileNumber}</p>
        </div>
        <div className="flex flex-col items-end gap-1 text-sm">
          <p className="text-text">{t(`role.${user.role}`)}</p>
          <StatusTag
            domain="officeMembership"
            value={user.membershipStatus}
            label={t(`status.${user.membershipStatus}`)}
          />
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
            <Button
              variant="secondary"
              onClick={() => {
                setDraftPermissions(user.permissionCodes)
                setEditing(true)
              }}
            >
              {t('editPermissions')}
            </Button>
          )}
          <Button
            variant="danger"
            disabled={isRevoking}
            onClick={() => {
              if (window.confirm(t('revokeConfirm'))) onRevoke()
            }}
          >
            {t('revoke')}
          </Button>
        </div>
      )}

      {editing && (
        <div className="flex flex-col gap-2 border-t border-surface-muted pt-3">
          <PermissionCheckboxList selected={draftPermissions} onChange={setDraftPermissions} />
          {permissionsError && <p className="text-sm text-danger">{permissionsError}</p>}
          <div className="flex gap-2">
            <Button
              variant="primary"
              disabled={isSavingPermissions}
              onClick={() => {
                onSavePermissions(draftPermissions)
                setEditing(false)
              }}
            >
              {t('saveChanges')}
            </Button>
            <Button variant="secondary" onClick={() => setEditing(false)}>
              {t('cancel')}
            </Button>
          </div>
        </div>
      )}
    </Card>
  )
}
