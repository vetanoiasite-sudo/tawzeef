import { useTranslation } from 'react-i18next'
import { Link, Outlet } from 'react-router-dom'
import { NotificationBell } from '@/features/notifications'
import { useAccess } from '@/shared/auth/useAccess'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { Button } from '@/shared/ui/Button'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

const navLinkClasses =
  'inline-flex min-h-9 items-center rounded-md px-2.5 text-sm font-medium text-text-muted transition-colors duration-150 hover:bg-primary/10 hover:text-primary'

export function AppLayout() {
  const { t } = useTranslation([
    'common',
    'auth',
    'jobSeekerProfile',
    'employerProfile',
    'officeJobSeekerReview',
    'officeEmployerReview',
    'complaints',
    'officeComplaintReview',
    'officeUsers',
    'publicJobs',
    'employerJobAds',
    'officeJobAdsReview',
    'jobSeekerApplications',
    'interviewsAndReports',
    'suggestions',
    'officeSuggestionsReview',
    'notifications',
  ])
  const { isAuthenticated, hasPermission } = useAccess()
  const user = useSessionStore((s) => s.user)
  const clearSession = useSessionStore((s) => s.clearSession)

  return (
    <div className="flex min-h-screen flex-col bg-surface-muted">
      <header className="sticky top-0 z-10 border-b border-surface-muted bg-surface">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-x-1 gap-y-2 px-4 py-3 sm:px-6">
          <Link to="/" className="me-2 shrink-0">
            <TawzeefLogo height={28} />
          </Link>

          <nav className="flex flex-1 flex-wrap items-center gap-1">
            <Link to="/jobs" className={navLinkClasses}>
              {t('publicJobs:pageTitle')}
            </Link>

            {isAuthenticated && (
              <>
                {user?.role === 'JobSeeker' && (
                  <Link to="/profile" className={navLinkClasses}>
                    {t('jobSeekerProfile:pageTitle')}
                  </Link>
                )}
                {user?.role === 'JobSeeker' && hasPermission('jobapplications.viewown') && (
                  <Link to="/my-applications" className={navLinkClasses}>
                    {t('jobSeekerApplications:pageTitle')}
                  </Link>
                )}
                {user?.role === 'Employer' && (
                  <Link to="/company-profile" className={navLinkClasses}>
                    {t('employerProfile:pageTitle')}
                  </Link>
                )}
                {user?.role === 'Employer' && hasPermission('jobadvertisements.view') && (
                  <Link to="/employer/job-advertisements" className={navLinkClasses}>
                    {t('employerJobAds:pageTitle')}
                  </Link>
                )}
                {hasPermission('jobadvertisements.review') && (
                  <Link to="/office/job-advertisements" className={navLinkClasses}>
                    {t('officeJobAdsReview:queueTitle')}
                  </Link>
                )}
                {/* hiringfollowupreports.view is also granted to Employer (own ads, per-application
                    read — RBAC_PERMISSIONS.md §3), so this office-only queue link needs the role
                    check too, not just the permission: an Employer legitimately has the code but
                    not an OfficeId, and /api/office/hiring-follow-ups/due requires one. */}
                {user?.role !== 'Employer' && user?.role !== 'JobSeeker' && hasPermission('hiringfollowupreports.view') && (
                  <Link to="/office/hiring-follow-ups" className={navLinkClasses}>
                    {t('interviewsAndReports:dueFollowUpsTitle')}
                  </Link>
                )}
                {hasPermission('jobseekers.view') && (
                  <Link to="/office/job-seekers" className={navLinkClasses}>
                    {t('officeJobSeekerReview:queueTitle')}
                  </Link>
                )}
                {hasPermission('employers.view') && (
                  <Link to="/office/employers" className={navLinkClasses}>
                    {t('officeEmployerReview:queueTitle')}
                  </Link>
                )}
                {hasPermission('complaints.viewown') && (
                  <Link to="/complaints" className={navLinkClasses}>
                    {t('complaints:pageTitle')}
                  </Link>
                )}
                {hasPermission('complaints.view') && (
                  <Link to="/office/complaints" className={navLinkClasses}>
                    {t('officeComplaintReview:queueTitle')}
                  </Link>
                )}
                {hasPermission('suggestions.viewown') && (
                  <Link to="/suggestions" className={navLinkClasses}>
                    {t('suggestions:pageTitle')}
                  </Link>
                )}
                {hasPermission('suggestions.view') && (
                  <Link to="/office/suggestions" className={navLinkClasses}>
                    {t('officeSuggestionsReview:queueTitle')}
                  </Link>
                )}
                {hasPermission('officeusers.view') && (
                  <Link to="/office/users" className={navLinkClasses}>
                    {t('officeUsers:pageTitle')}
                  </Link>
                )}
              </>
            )}
          </nav>

          {isAuthenticated ? (
            <div className="flex shrink-0 items-center gap-3 border-s border-surface-muted ps-3">
              <NotificationBell />
              <span className="hidden text-sm text-text sm:inline">{t('common:welcomeUser', { name: user?.fullName })}</span>
              <Button variant="secondary" onClick={clearSession}>
                {t('common:logout')}
              </Button>
            </div>
          ) : (
            <Link to="/login" className="shrink-0">
              <Button variant="primary">{t('auth:loginLink')}</Button>
            </Link>
          )}
        </div>
      </header>
      <main className="flex-1">
        <div className="mx-auto max-w-6xl">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
