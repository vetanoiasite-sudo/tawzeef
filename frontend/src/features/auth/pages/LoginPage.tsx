import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'
import { Card } from '@/shared/ui/Card'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'
import { toSessionUser } from '../api/auth.mapper'
import { useLogin } from '../api/auth.queries'
import { loginSchema, type LoginFormValues } from '../schemas/auth.schemas'

export default function LoginPage() {
  const { t } = useTranslation(['auth', 'validation'])
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const setSession = useSessionStore((s) => s.setSession)
  const login = useLogin()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })

  function onSubmit(values: LoginFormValues) {
    login.mutate(values, {
      onSuccess: (result) => {
        setSession(result.accessToken, toSessionUser(result))
        // Same-origin relative path only (PROJECT_ARCHITECTURE_FRONTEND.md §13.1) — never follow
        // an absolute/external redirect target from a query param.
        const redirect = searchParams.get('redirect')
        void navigate(redirect?.startsWith('/') ? redirect : '/', { replace: true })
      },
    })
  }

  const submitError = login.error instanceof ApiError ? login.error.messages[0] : undefined

  return (
    <div className="mx-auto flex max-w-md flex-col gap-6 px-4 py-10">
      <div className="flex flex-col items-center gap-2">
        <TawzeefLogo height={48} />
        <h1 className="text-lg font-semibold text-text">{t('auth:loginTitle')}</h1>
      </div>

      <Card>
        <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
          <TextInput
            id="identifier"
            type="tel"
            autoComplete="tel"
            label={t('auth:loginIdentifierLabel')}
            error={errors.identifier ? t(errors.identifier.message ?? '') : undefined}
            {...register('identifier')}
          />

          <TextInput
            id="password"
            type="password"
            autoComplete="current-password"
            label={t('auth:loginPasswordLabel')}
            error={errors.password ? t(errors.password.message ?? '') : undefined}
            {...register('password')}
          />

          {submitError && <p className="text-sm text-danger">{submitError}</p>}

          <Button type="submit" variant="primary" disabled={login.isPending}>
            {t('auth:loginSubmit')}
          </Button>
        </form>
      </Card>

      <p className="text-center text-sm text-text-muted">
        {t('auth:noAccount')}{' '}
        <Link to="/register/job-seeker" className="font-medium text-primary hover:underline">
          {t('auth:registerLink')}
        </Link>{' '}
        ·{' '}
        <Link to="/register/employer" className="font-medium text-primary hover:underline">
          {t('auth:registerEmployerLink')}
        </Link>
      </p>
      <p className="text-center text-sm text-text-muted">
        <Link to="/accept-invite" className="font-medium text-primary hover:underline">
          {t('auth:acceptInviteLink')}
        </Link>
      </p>
    </div>
  )
}
