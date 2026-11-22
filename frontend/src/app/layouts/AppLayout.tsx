import clsx from 'clsx'
import { useTranslation } from 'react-i18next'
import { Link, Outlet, useLocation } from 'react-router-dom'
import { NotificationBell } from '@/features/notifications'
import { useAccess } from '@/shared/auth/useAccess'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { useScrolled } from '@/shared/hooks/useScrolled'
import { Button } from '@/shared/ui/Button'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

// Auth screens are full-canvas (own headline, hero, chrome) — the app header
// would compete with them, so we skip it on these routes.
const BARE_ROUTES = ['/login', '/register/job-seeker', '/register/employer', '/accept-invite']

// Routes whose top section is a dark hero image — the header starts transparent
// over the image and solidifies once the user scrolls past it.
const HERO_ROUTES = new Set(['/'])

export function AppLayout() {
  const location = useLocation()
  if (BARE_ROUTES.includes(location.pathname)) {
    return <Outlet />
  }
  return <AppLayoutChrome />
}

function AppLayoutChrome() {
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
  const location = useLocation()
  const scrolled = useScrolled(24)
  // On a hero route (dark image at top) the header starts transparent over the
  // image; after the first scroll it flips to the solid surface treatment.
  const overHero = HERO_ROUTES.has(location.pathname) && !scrolled

  const navLinkClasses = clsx(
    'inline-flex h-10 items-center rounded-200 px-3 text-[14px] font-bold transition-colors duration-150',
    overHero
      ? 'text-white/90 hover:bg-white/10 hover:text-white'
      : 'text-content-secondary hover:bg-brand-soft hover:text-brand',
  )

  return (
    <div className="flex min-h-screen flex-col bg-surface">
      <header
        className={clsx(
          'fixed inset-x-0 top-0 z-30 h-16 backdrop-saturate-150',
          'transition-[background-color,backdrop-filter,border-color,box-shadow] duration-300 ease-out',
          overHero
            ? 'border-b border-transparent bg-transparent backdrop-blur-0'
            : 'border-b border-border-primary bg-white/85 shadow-[0_1px_0_0_rgba(0,0,0,0.02)] backdrop-blur-xl',
        )}
      >
        <div className="flex h-full w-full items-center px-4 sm:px-8 xl:px-16 2xl:px-24">
          {/* Three-column header: logo (start) · centered nav · session controls (end). */}
          <div className="flex flex-1 items-center">
            <Link to="/" className="shrink-0">
              <TawzeefLogo
                height={28}
                color={overHero ? '#FFFFFF' : '#1A23C3'}
                accentColor="#FFE343"
              />
            </Link>
          </div>

          <nav className="flex shrink-0 items-center gap-1">
            {/* Public tabs — visible to everyone, drive the three main journeys
             *  the platform supports: browsing open jobs, registering as a
             *  job seeker, registering as an employer. */}
            {!isAuthenticated && (
              <>
                <Link to="/jobs" className={navLinkClasses}>
                  تصفّح الوظائف
                </Link>
                <Link to="/register/job-seeker" className={navLinkClasses}>
                  للباحثين عن عمل
                </Link>
                <Link to="/register/employer" className={navLinkClasses}>
                  للشركات
                </Link>
              </>
            )}

            {isAuthenticated && (
              <>
                {user?.role === 'JobSeeker' && (
                  <Link to="/dashboard" className={navLinkClasses}>
                    {t('jobSeekerProfile:dashboardNav')}
                  </Link>
                )}
                {user?.role === 'Employer' && (
                  <Link to="/employer/dashboard" className={navLinkClasses}>
                    {t('employerProfile:dashboardNav')}
                  </Link>
                )}
                <Link to="/jobs" className={navLinkClasses}>
                  الوظائف
                </Link>
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

          <div className="flex flex-1 items-center justify-end">
          {isAuthenticated ? (
            <div
              className={clsx(
                'flex shrink-0 items-center gap-3 border-s ps-3',
                overHero ? 'border-white/20 text-white' : 'border-border-primary',
              )}
            >
              <NotificationBell />
              <span
                className={clsx(
                  'hidden text-[14px] sm:inline',
                  overHero ? 'text-white' : 'text-content-primary',
                )}
              >
                {t('common:welcomeUser', { name: user?.fullName })}
              </span>
              <Button
                variant="secondary"
                size="sm"
                onClick={clearSession}
                className={clsx(overHero && '!border-white/40 !bg-transparent !text-white hover:!border-white hover:!bg-white/10 hover:!text-white')}
              >
                {t('common:logout')}
              </Button>
            </div>
          ) : (
            <Link to="/login" className="shrink-0">
              <Button variant="primary" size="sm">
                {t('auth:loginLink')}
              </Button>
            </Link>
          )}
          </div>
        </div>
      </header>
      {/* `pt-16` offsets the fixed header so page content doesn't hide under it.
       *  Hero sections on HERO_ROUTES use `-mt-16` to extend under it instead. */}
      <main className="flex-1 pt-16">
        <div className="w-full">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
