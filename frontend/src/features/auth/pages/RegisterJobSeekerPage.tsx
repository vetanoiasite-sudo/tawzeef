import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { toSessionUser } from '../api/auth.mapper'
import { useRegisterJobSeeker } from '../api/auth.queries'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
import { DetailsStep } from '../components/DetailsStep'
import type { JobSeekerDetailsFormValues } from '../schemas/auth.schemas'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

// One-step registration (name/mobile/email/password/office). No phone-OTP verification step —
// removed 2026-11-23, temporary (AuthDtos.cs LoginDto's doc comment); the OTP request/verify
// endpoints and components still exist, just unused here for now.
export default function RegisterJobSeekerPage() {
  const { t } = useTranslation('auth')
  const navigate = useNavigate()
  const setSession = useSessionStore((s) => s.setSession)

  const registerJobSeeker = useRegisterJobSeeker()

  function handleRegister(values: JobSeekerDetailsFormValues) {
    registerJobSeeker.mutate(
      {
        fullName: values.fullName,
        mobileNumber: values.mobileNumber,
        email: values.email,
        password: values.password,
        officeId: values.officeId,
      },
      {
        onSuccess: (result) => {
          setSession(result.accessToken, toSessionUser(result))
          void navigate('/dashboard', { replace: true })
        },
      },
    )
  }

  return (
    <AuthScaffold maxWidth={520}>
      <AuthHeader title={t('registerTitle')} />

      <DetailsStep
        isSubmitting={registerJobSeeker.isPending}
        submitError={errorMessage(registerJobSeeker.error)}
        onSubmit={handleRegister}
      />

      <p className="mt-6 text-center text-[14px] text-content-secondary">
        {t('alreadyHaveAccount')}{' '}
        <Link to="/login" className="font-bold text-brand hover:text-brand-hover hover:underline">
          {t('loginLink')}
        </Link>
        {' · '}
        <Link
          to="/register/employer"
          className="font-bold text-brand hover:text-brand-hover hover:underline"
        >
          {t('registerEmployerLink')}
        </Link>
      </p>
    </AuthScaffold>
  )
}
