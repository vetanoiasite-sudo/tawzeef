import { useTranslation } from 'react-i18next'
import { Link, Outlet } from 'react-router-dom'
import { useAccess } from '@/shared/auth/useAccess'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

export function AppLayout() {
  const { t } = useTranslation(['common', 'auth'])
  const { isAuthenticated } = useAccess()
  const user = useSessionStore((s) => s.user)
  const clearSession = useSessionStore((s) => s.clearSession)

  return (
    <div className="flex min-h-screen flex-col">
      <header className="flex items-center justify-between border-b border-surface-muted px-6 py-3">
        <TawzeefLogo height={32} />
        {isAuthenticated ? (
          <div className="flex items-center gap-3 text-sm">
            <span className="text-text">{t('common:welcomeUser', { name: user?.fullName })}</span>
            <button type="button" onClick={clearSession} className="text-primary">
              {t('common:logout')}
            </button>
          </div>
        ) : (
          <Link to="/login" className="text-sm text-primary">
            {t('auth:loginLink')}
          </Link>
        )}
      </header>
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  )
}
