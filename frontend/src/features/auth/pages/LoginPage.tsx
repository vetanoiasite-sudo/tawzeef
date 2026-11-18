import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'
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

      <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
        <div className="flex flex-col gap-1">
          <label htmlFor="identifier" className="text-sm font-medium text-text">
            {t('auth:loginIdentifierLabel')}
          </label>
          <input
            id="identifier"
            type="tel"
            autoComplete="tel"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('identifier')}
          />
          {errors.identifier && (
            <p className="text-sm text-danger">{t(errors.identifier.message ?? '')}</p>
          )}
        </div>

        <div className="flex flex-col gap-1">
          <label htmlFor="password" className="text-sm font-medium text-text">
            {t('auth:loginPasswordLabel')}
          </label>
          <input
            id="password"
            type="password"
            autoComplete="current-password"
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
            {...register('password')}
          />
          {errors.password && (
            <p className="text-sm text-danger">{t(errors.password.message ?? '')}</p>
          )}
        </div>

        {submitError && <p className="text-sm text-danger">{submitError}</p>}

        <button
          type="submit"
          disabled={login.isPending}
          className="rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
        >
          {t('auth:loginSubmit')}
        </button>
      </form>

      <p className="text-center text-sm text-text-muted">
        {t('auth:noAccount')}{' '}
        <Link to="/register/job-seeker" className="text-primary">
          {t('auth:registerLink')}
        </Link>{' '}
        ·{' '}
        <Link to="/register/employer" className="text-primary">
          {t('auth:registerEmployerLink')}
        </Link>
      </p>
    </div>
  )
}
