import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { Button } from '@/shared/ui/Button'
import { TextInput } from '@/shared/ui/TextInput'
import { AuthHeader } from '../components/AuthHeader'
import { AuthScaffold } from '../components/AuthScaffold'
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
    <AuthScaffold>
      <AuthHeader
        title={t('auth:loginTitle')}
        subtitle={t('auth:loginSubtitle', {
          defaultValue: 'ادخل إلى حسابك للوصول إلى المكاتب والوظائف والطلبات.',
        })}
      />

      <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-5">
        <TextInput
          id="identifier"
          type="tel"
          autoComplete="tel"
          dir="ltr"
          placeholder="+20 100 123 4567"
          label={t('auth:loginIdentifierLabel')}
          error={errors.identifier ? t(errors.identifier.message ?? '') : undefined}
          {...register('identifier')}
        />

        <TextInput
          id="password"
          type="password"
          autoComplete="current-password"
          placeholder="••••••••"
          label={t('auth:loginPasswordLabel')}
          error={errors.password ? t(errors.password.message ?? '') : undefined}
          {...register('password')}
        />

        {submitError && (
          <div
            role="alert"
            className="rounded-200 border border-danger/30 bg-negative-soft px-4 py-3 text-[14px] font-medium text-danger"
          >
            {submitError}
          </div>
        )}

        <Button type="submit" variant="primary" size="lg" fullWidth disabled={login.isPending}>
          {login.isPending
            ? t('auth:loginSubmitting', { defaultValue: 'جارٍ تسجيل الدخول...' })
            : t('auth:loginSubmit')}
        </Button>
      </form>

      <div className="my-6 flex items-center gap-3">
        <span className="h-px flex-1 bg-border-primary" aria-hidden />
        <span className="text-[12px] font-bold uppercase tracking-[0.12em] text-content-tertiary">
          {t('auth:orDivider', { defaultValue: 'أو' })}
        </span>
        <span className="h-px flex-1 bg-border-primary" aria-hidden />
      </div>

      <div className="flex flex-col gap-3">
        <p className="text-center text-[14px] text-content-secondary">{t('auth:noAccount')}</p>
        <div className="grid grid-cols-1 gap-2">
          <AuthLinkButton to="/register/job-seeker" icon={<SeekerIcon />}>
            {t('auth:registerLink')}
          </AuthLinkButton>
          <AuthLinkButton to="/register/employer" icon={<EmployerIcon />}>
            {t('auth:registerEmployerLink')}
          </AuthLinkButton>
        </div>
        <Link
          to="/accept-invite"
          className="mt-1 inline-flex items-center justify-center gap-1.5 text-[14px] font-bold text-brand hover:text-brand-hover hover:underline"
        >
          <InviteIcon />
          <span>{t('auth:acceptInviteLink')}</span>
        </Link>
      </div>
    </AuthScaffold>
  )
}

function AuthLinkButton({
  to,
  icon,
  children,
}: {
  to: string
  icon: React.ReactNode
  children: React.ReactNode
}) {
  return (
    <Link
      to={to}
      className="group flex h-12 items-center justify-center gap-2 whitespace-nowrap rounded-200 border border-border-primary bg-surface px-3 text-[14px] font-bold text-content-primary transition-colors hover:border-brand hover:text-brand"
    >
      {icon}
      <span>{children}</span>
    </Link>
  )
}

function SeekerIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="8" r="4" stroke="currentColor" strokeWidth="1.8" />
      <path
        d="M4 20c0-3.3 3.6-6 8-6s8 2.7 8 6"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </svg>
  )
}

function EmployerIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <rect x="3" y="7" width="18" height="13" rx="2" stroke="currentColor" strokeWidth="1.8" />
      <path d="M9 7V5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2v2" stroke="currentColor" strokeWidth="1.8" />
      <path d="M3 12h18" stroke="currentColor" strokeWidth="1.8" />
    </svg>
  )
}

function InviteIcon() {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M4 8l8 5 8-5M4 8v10a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8M4 8l2-3h12l2 3"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}
