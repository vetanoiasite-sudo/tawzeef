import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { InviteOfficeUserPayload } from '@/features/office-users/types/officeUser.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput } from '@/shared/ui/TextInput'

/** SuperAdmin-only — invites the first OfficeAdmin into a given office (there's no OfficeAdmin
 * yet to do it themselves, OfficeUsersController.Invite's dual-mode doc comment). The regular
 * "invite office staff" form (frontend/src/features/office-users/components/InviteOfficeUserForm)
 * is for an already-logged-in OfficeAdmin inviting into their own office and isn't reused here —
 * this one is fixed to role OfficeAdmin, has no permission-codes picker, and carries officeId
 * explicitly in the payload. */
export function InviteOfficeAdminForm({
  officeId,
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: {
  officeId: string
  isSubmitting: boolean
  error?: string
  onSubmit: (payload: InviteOfficeUserPayload) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('superAdmin')
  const [fullName, setFullName] = useState('')
  const [mobileNumber, setMobileNumber] = useState('')
  const [email, setEmail] = useState('')

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    onSubmit({
      fullName: fullName.trim(),
      mobileNumber: mobileNumber.trim(),
      email: email.trim(),
      role: 'OfficeAdmin',
      permissionCodes: [],
      officeId,
    })
  }

  return (
    <form onSubmit={handleSubmit}>
      <Card className="flex flex-col gap-4">
        <p className="text-sm text-content-secondary">{t('inviteOfficeAdminHint')}</p>

        <TextInput
          id="inviteFullName"
          label={t('inviteFullNameLabel')}
          value={fullName}
          onChange={(e) => setFullName(e.target.value)}
        />

        <TextInput
          id="inviteMobileNumber"
          type="tel"
          dir="ltr"
          label={t('inviteMobileNumberLabel')}
          value={mobileNumber}
          onChange={(e) => setMobileNumber(e.target.value)}
        />

        <TextInput
          id="inviteEmail"
          type="email"
          label={t('inviteEmailLabel')}
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />

        {error && <p className="text-sm text-danger">{error}</p>}

        <div className="flex gap-2">
          <Button
            type="submit"
            variant="primary"
            disabled={isSubmitting || !fullName.trim() || !mobileNumber.trim() || !email.trim()}
          >
            {t('sendInviteAction')}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel}>
            {t('cancel')}
          </Button>
        </div>
      </Card>
    </form>
  )
}
