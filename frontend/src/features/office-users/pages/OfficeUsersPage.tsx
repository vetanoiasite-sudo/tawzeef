import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { InviteOfficeUserForm } from '../components/InviteOfficeUserForm'
import { OfficeUserRow } from '../components/OfficeUserRow'
import {
  useDeactivateOfficeUser,
  useInviteOfficeUser,
  useOfficeUsers,
  useUpdateOfficeUserPermissions,
} from '../api/officeUser.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function OfficeUsersPage() {
  const { t } = useTranslation('officeUsers')
  const [showInvite, setShowInvite] = useState(false)

  const { data: users, isLoading } = useOfficeUsers()
  const inviteUser = useInviteOfficeUser()
  const updatePermissions = useUpdateOfficeUserPermissions()
  const deactivateUser = useDeactivateOfficeUser()

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8">
      <PageHeader
        title={t('pageTitle')}
        action={
          !showInvite && (
            <Button variant="primary" onClick={() => setShowInvite(true)}>
              {t('inviteNew')}
            </Button>
          )
        }
      />

      {showInvite && (
        <InviteOfficeUserForm
          isSubmitting={inviteUser.isPending}
          error={errorMessage(inviteUser.error)}
          onSubmit={(payload) => inviteUser.mutate(payload, { onSuccess: () => setShowInvite(false) })}
          onCancel={() => setShowInvite(false)}
        />
      )}
      {inviteUser.isSuccess && !showInvite && <p className="text-sm text-success">{t('inviteSuccess')}</p>}

      {isLoading && <LoadingState />}

      {!isLoading && users && users.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && users && users.length > 0 && (
        <div className="flex flex-col gap-3">
          {users.map((user) => (
            <OfficeUserRow
              key={user.membershipId}
              user={user}
              isSavingPermissions={updatePermissions.isPending}
              permissionsError={
                updatePermissions.variables?.membershipId === user.membershipId
                  ? errorMessage(updatePermissions.error)
                  : undefined
              }
              isRevoking={deactivateUser.isPending}
              onSavePermissions={(permissionCodes) =>
                updatePermissions.mutate({ membershipId: user.membershipId, permissionCodes })
              }
              onRevoke={() => deactivateUser.mutate(user.membershipId)}
            />
          ))}
        </div>
      )}
    </div>
  )
}
