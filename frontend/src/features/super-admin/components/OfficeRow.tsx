import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { InviteOfficeUserPayload } from '@/features/office-users/types/officeUser.types'
import { OfficeForm } from './OfficeForm'
import { InviteOfficeAdminForm } from './InviteOfficeAdminForm'
import { OfficeLogoUploadCard } from './OfficeLogoUploadCard'
import type { Office, UpdateOfficePayload } from '../types/superAdmin.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'

export function OfficeRow({
  office,
  isSaving,
  saveError,
  isTogglingStatus,
  isInviting,
  inviteError,
  inviteSuccess,
  isUploadingLogo,
  onSave,
  onToggleStatus,
  onInviteAdmin,
  onUploadLogo,
}: {
  office: Office
  isSaving: boolean
  saveError?: string
  isTogglingStatus: boolean
  isInviting: boolean
  inviteError?: string
  inviteSuccess: boolean
  isUploadingLogo: boolean
  onSave: (payload: UpdateOfficePayload) => void
  onToggleStatus: () => void
  onInviteAdmin: (payload: InviteOfficeUserPayload) => void
  onUploadLogo: (file: File) => void
}) {
  const { t } = useTranslation('superAdmin')
  const [editing, setEditing] = useState(false)
  const [inviting, setInviting] = useState(false)

  if (editing) {
    return (
      <OfficeForm
        office={office}
        isSubmitting={isSaving}
        error={saveError}
        onSubmit={(payload) => onSave(payload as UpdateOfficePayload)}
        onCancel={() => setEditing(false)}
      />
    )
  }

  if (inviting) {
    return (
      <InviteOfficeAdminForm
        officeId={office.id}
        isSubmitting={isInviting}
        error={inviteError}
        onSubmit={(payload) => {
          onInviteAdmin(payload)
          setInviting(false)
        }}
        onCancel={() => setInviting(false)}
      />
    )
  }

  return (
    <Card className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
      <div className="flex items-center gap-3">
        <OfficeLogoUploadCard
          logoUrl={office.logoUrl}
          isUploading={isUploadingLogo}
          onUpload={onUploadLogo}
        />
        <div className="flex flex-col gap-1">
          <div className="flex flex-wrap items-center gap-2">
            <p className="font-medium text-text">{office.name}</p>
            <StatusTag domain="office" value={office.status} label={t(`status.${office.status}`)} />
          </div>
          <p className="text-sm text-text-muted">
            {office.code} · {office.city ?? '—'}
          </p>
          {inviteSuccess && <p className="text-sm text-success">{t('inviteOfficeAdminSuccess')}</p>}
        </div>
      </div>

      <div className="flex flex-wrap gap-2">
        {office.status === 'Active' && (
          <Button variant="secondary" onClick={() => setInviting(true)}>
            {t('inviteOfficeAdminAction')}
          </Button>
        )}
        <Button variant="secondary" onClick={() => setEditing(true)}>
          {t('editAction')}
        </Button>
        <Button
          variant={office.status === 'Active' ? 'danger' : 'success'}
          disabled={isTogglingStatus}
          onClick={() => {
            if (office.status === 'Active' && !window.confirm(t('deactivateConfirm'))) return
            onToggleStatus()
          }}
        >
          {office.status === 'Active' ? t('deactivateAction') : t('activateAction')}
        </Button>
      </div>
    </Card>
  )
}
