import { useTranslation } from 'react-i18next'
import { Link, Outlet } from 'react-router-dom'
import { useAccess } from '@/shared/auth/useAccess'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

export function AppLayout() {
  const { t } = useTranslation(['common', 'auth', 'jobSeekerProfile', 'officeJobSeekerReview', 'complaints', 'officeComplaintReview', 'officeUsers'])
  const { isAuthenticated, hasPermission } = useAccess()
  const user = useSessionStore((s) => s.user)
  const clearSession = useSessionStore((s) => s.clearSession)

  return (
    <div className="flex min-h-screen flex-col">
      <header className="flex items-center justify-between border-b border-surface-muted px-6 py-3">
        <TawzeefLogo height={32} />
        {isAuthenticated ? (
          <div className="flex items-center gap-3 text-sm">
            {user?.role === 'JobSeeker' && (
              <Link to="/profile" className="text-primary">
                {t('jobSeekerProfile:pageTitle')}
              </Link>
            )}
            {hasPermission('jobseekers.view') && (
              <Link to="/office/job-seekers" className="text-primary">
                {t('officeJobSeekerReview:queueTitle')}
              </Link>
            )}
            {hasPermission('complaints.viewown') && (
              <Link to="/complaints" className="text-primary">
                {t('complaints:pageTitle')}
              </Link>
            )}
            {hasPermission('complaints.view') && (
              <Link to="/office/complaints" className="text-primary">
                {t('officeComplaintReview:queueTitle')}
              </Link>
            )}
            {hasPermission('officeusers.view') && (
              <Link to="/office/users" className="text-primary">
                {t('officeUsers:pageTitle')}
              </Link>
            )}
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
