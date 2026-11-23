import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { roleLandingPath } from '@/shared/auth/roleLandingPath'
import { toSessionUser } from '../api/auth.mapper'
import { useAcceptStaffInvite } from '../api/auth.queries'
import { AcceptInvitePasswordStep } from '../components/AcceptInvitePasswordStep'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import type { AcceptInvitePasswordFormValues } from '../schemas/auth.schemas'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

/// <summary>Only reachable after an OfficeAdmin/SuperAdmin invite (OfficeUsersController.Invite)
/// — the invitee enters the phone number their invite was sent to and sets their own password.
/// No phone-OTP verification step — removed 2026-11-23, temporary (AuthDtos.cs LoginDto's doc
/// comment); the OTP request/verify endpoints and components still exist, just unused here for
/// now. Nobody but the invitee ever sees or sets their password.</summary>
export default function AcceptStaffInvitePage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const acceptInvite = useAcceptStaffInvite()

  function handleAccept(values: AcceptInvitePasswordFormValues) {
    acceptInvite.mutate(
      { mobileNumber: values.mobileNumber, password: values.password },
      {
        onSuccess: (result) => {
          setSession(result.accessToken, toSessionUser(result))
          void navigate(roleLandingPath(result.role), { replace: true })
        },
      },
    )
  }

  return (
    <AuthScaffold maxWidth={520}>
      <AuthHeader title={t('acceptInviteTitle')} />

      <AcceptInvitePasswordStep
        isSubmitting={acceptInvite.isPending}
        submitError={errorMessage(acceptInvite.error)}
        onSubmit={handleAccept}
      />
    </AuthScaffold>
  )
}
