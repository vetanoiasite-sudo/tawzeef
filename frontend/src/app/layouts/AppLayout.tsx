import clsx from 'clsx'
import { useEffect, useState } from 'react'
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
    'officeDashboard',
    'superAdmin',
    'publicJobs',
    'employerJobAds',
    'officeJobAdsReview',
    'jobSeekerApplications',
    'interviewsAndReports',
    'suggestions',
    'officeSuggestionsReview',
    'notifications',
    'auditLog',
    'platformSettings',
  ])
  const { isAuthenticated, hasPermission } = useAccess()
  const user = useSessionStore((s) => s.user)
  const clearSession = useSessionStore((s) => s.clearSession)
  const location = useLocation()
  const scrolled = useScrolled(24)
  const [menuOpen, setMenuOpen] = useState(false)
  // On a hero route (dark image at top) the header starts transparent over the
  // image; after the first scroll it flips to the solid surface treatment.
  const overHero = HERO_ROUTES.has(location.pathname) && !scrolled && !menuOpen

  // Close the mobile drawer on every navigation, and never leave it open across routes.
  useEffect(() => setMenuOpen(false), [location.pathname])

  // Lock background scroll while the drawer is open, and let Escape close it.
  useEffect(() => {
    if (!menuOpen) return
    const previousOverflow = document.body.style.overflow
    document.body.style.overflow = 'hidden'
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') setMenuOpen(false)
    }
    window.addEventListener('keydown', onKeyDown)
    return () => {
      document.body.style.overflow = previousOverflow
      window.removeEventListener('keydown', onKeyDown)
    }
  }, [menuOpen])

  const navLinkClasses = clsx(
    'inline-flex h-10 items-center rounded-200 px-3 text-[14px] font-bold transition-colors duration-150',
    'lg:h-10',
    overHero
      ? 'text-white/90 hover:bg-white/10 hover:text-white'
      : 'text-content-secondary hover:bg-brand-soft hover:text-brand',
  )
  // Same links, full-width rows — used inside the mobile dropdown panel instead.
  const mobileNavLinkClasses =
    'flex h-12 items-center rounded-200 px-3 text-[15px] font-bold text-content-secondary transition-colors duration-150 hover:bg-brand-soft hover:text-brand'

  // Parameterized so the identical link list can render twice — compact inline for the desktop
  // nav, full-width/larger touch targets for the mobile dropdown panel — without duplicating the
  // ~15-link conditional list itself.
  function renderNavLinks(linkClassName: string) {
    return (
      <>
        {/* Public tabs — visible to everyone, drive the three main journeys
         *  the platform supports: browsing open jobs, registering as a
         *  job seeker, registering as an employer. */}
        {!isAuthenticated && (
          <>
            <Link to="/jobs" className={linkClassName}>
              تصفّح الوظائف
            </Link>
            <Link to="/register/job-seeker" className={linkClassName}>
              للباحثين عن عمل
            </Link>
            <Link to="/register/employer" className={linkClassName}>
              للشركات
            </Link>
          </>
        )}

        {isAuthenticated && (
          <>
            {user?.role === 'JobSeeker' && (
              <Link to="/dashboard" className={linkClassName}>
                {t('jobSeekerProfile:dashboardNav')}
              </Link>
            )}
            {user?.role === 'Employer' && (
              <Link to="/employer/dashboard" className={linkClassName}>
                {t('employerProfile:dashboardNav')}
              </Link>
            )}
            {(user?.role === 'OfficeAdmin' || user?.role === 'OfficeStaff') && (
              <Link to="/office/dashboard" className={linkClassName}>
                {t('officeDashboard:dashboardNav')}
              </Link>
            )}
            {user?.role === 'SuperAdmin' && (
              <Link to="/super-admin/dashboard" className={linkClassName}>
                {t('superAdmin:dashboardNav')}
              </Link>
            )}
            <Link to="/jobs" className={linkClassName}>
              الوظائف
            </Link>
            {user?.role === 'JobSeeker' && (
              <Link to="/profile" className={linkClassName}>
                {t('jobSeekerProfile:pageTitle')}
              </Link>
            )}
            {user?.role === 'JobSeeker' && hasPermission('jobapplications.viewown') && (
              <Link to="/my-applications" className={linkClassName}>
                {t('jobSeekerApplications:pageTitle')}
              </Link>
            )}
            {user?.role === 'Employer' && (
              <Link to="/company-profile" className={linkClassName}>
                {t('employerProfile:pageTitle')}
              </Link>
            )}
            {user?.role === 'Employer' && hasPermission('jobadvertisements.view') && (
              <Link to="/employer/job-advertisements" className={linkClassName}>
                {t('employerJobAds:pageTitle')}
              </Link>
            )}
            {hasPermission('jobadvertisements.review') && (
              <Link to="/office/job-advertisements" className={linkClassName}>
                {t('officeJobAdsReview:queueTitle')}
              </Link>
            )}
            {/* hiringfollowupreports.view is also granted to Employer (own ads, per-application
                read — RBAC_PERMISSIONS.md §3), so this office-only queue link needs the role
                check too, not just the permission: an Employer legitimately has the code but
                not an OfficeId, and /api/office/hiring-follow-ups/due requires one. */}
            {user?.role !== 'Employer' && user?.role !== 'JobSeeker' && hasPermission('hiringfollowupreports.view') && (
              <Link to="/office/hiring-follow-ups" className={linkClassName}>
                {t('interviewsAndReports:dueFollowUpsTitle')}
              </Link>
            )}
            {hasPermission('jobseekers.view') && (
              <Link to="/office/job-seekers" className={linkClassName}>
                {t('officeJobSeekerReview:queueTitle')}
              </Link>
            )}
            {hasPermission('employers.view') && (
              <Link to="/office/employers" className={linkClassName}>
                {t('officeEmployerReview:queueTitle')}
              </Link>
            )}
            {hasPermission('complaints.viewown') && (
              <Link to="/complaints" className={linkClassName}>
                {t('complaints:pageTitle')}
              </Link>
            )}
            {hasPermission('complaints.view') && (
              <Link to="/office/complaints" className={linkClassName}>
                {t('officeComplaintReview:queueTitle')}
              </Link>
            )}
            {hasPermission('suggestions.viewown') && (
              <Link to="/suggestions" className={linkClassName}>
                {t('suggestions:pageTitle')}
              </Link>
            )}
            {hasPermission('suggestions.view') && (
              <Link to="/office/suggestions" className={linkClassName}>
                {t('officeSuggestionsReview:queueTitle')}
              </Link>
            )}
            {hasPermission('officeusers.view') && (
              <Link to="/office/users" className={linkClassName}>
                {t('officeUsers:pageTitle')}
              </Link>
            )}
            {hasPermission('auditlogs.view') && (
              <Link to="/audit-log" className={linkClassName}>
                {t('auditLog:pageTitle')}
              </Link>
            )}
            {hasPermission('platformsettings.view') && (
              <Link to="/super-admin/settings" className={linkClassName}>
                {t('platformSettings:nav')}
              </Link>
            )}
          </>
        )}
      </>
    )
  }

  const sessionControls = isAuthenticated ? (
    <>
      <NotificationBell />
      <span
        className={clsx(
          'hidden text-[14px] lg:inline',
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
    </>
  ) : (
    <Link to="/login" className="shrink-0">
      <Button variant="primary" size="sm">
        {t('auth:loginLink')}
      </Button>
    </Link>
  )

  return (
    <div className="flex min-h-screen flex-col bg-surface">
      <header
        className={clsx(
          'fixed inset-x-0 top-0 z-30 backdrop-saturate-150',
          'transition-[background-color,backdrop-filter,border-color,box-shadow] duration-300 ease-out',
          overHero
            ? 'border-b border-transparent bg-transparent backdrop-blur-0'
            : 'border-b border-border-primary bg-white/85 shadow-[0_1px_0_0_rgba(0,0,0,0.02)] backdrop-blur-xl',
          menuOpen && 'bg-surface shadow-[0_1px_0_0_rgba(0,0,0,0.02)]',
        )}
      >
        <div className="flex h-16 w-full items-center px-4 sm:px-8 xl:px-16 2xl:px-24">
          {/* Three-column header on large screens: logo (start) · centered nav · session
           *  controls (end). Below `lg` the nav collapses behind a hamburger button instead —
           *  the full link list (up to ~13 items for office roles) never fits a phone width. */}
          <div className="flex flex-1 items-center">
            <Link to="/" className="shrink-0">
              <TawzeefLogo
                height={28}
                color={overHero ? '#FFFFFF' : '#1A23C3'}
                accentColor="#FFE343"
              />
            </Link>
          </div>

          <nav className="hidden shrink-0 items-center gap-1 lg:flex">{renderNavLinks(navLinkClasses)}</nav>

          <div className="flex flex-1 items-center justify-end gap-2">
            <div
              className={clsx(
                'hidden shrink-0 items-center gap-3 border-s ps-3 lg:flex',
                overHero ? 'border-white/20 text-white' : 'border-border-primary',
              )}
            >
              {sessionControls}
            </div>

            {/* Mobile: notification bell (when signed in) stays visible next to the toggle. */}
            {isAuthenticated && (
              <span className="lg:hidden">
                <NotificationBell />
              </span>
            )}

            <button
              type="button"
              onClick={() => setMenuOpen((v) => !v)}
              aria-label={menuOpen ? 'إغلاق القائمة' : 'فتح القائمة'}
              aria-expanded={menuOpen}
              className={clsx(
                'flex size-10 shrink-0 items-center justify-center rounded-200 transition-colors duration-150 lg:hidden',
                overHero ? 'text-white hover:bg-white/10' : 'text-content-primary hover:bg-brand-soft',
              )}
            >
              {menuOpen ? <CloseIcon /> : <MenuIcon />}
            </button>
          </div>
        </div>

      </header>

      {/* Mobile nav drawer — slides in from the start edge (the right, in RTL) instead of
       *  dropping down from the header, so it reads as its own panel over the page rather than
       *  an extension of the header bar. Backdrop click, Escape, or a route change all close it. */}
      <div
        aria-hidden={!menuOpen}
        className={clsx(
          'fixed inset-0 z-40 bg-black/40 transition-opacity duration-300 lg:hidden',
          menuOpen ? 'pointer-events-auto opacity-100' : 'pointer-events-none opacity-0',
        )}
        onClick={() => setMenuOpen(false)}
      />
      <div
        role="dialog"
        aria-modal="true"
        aria-label="القائمة"
        className={clsx(
          'fixed inset-y-0 start-0 z-50 flex w-[85%] max-w-[340px] flex-col bg-surface shadow-[0_0_32px_rgba(0,0,0,0.18)]',
          'transition-transform duration-300 ease-out lg:hidden',
          menuOpen ? 'translate-x-0' : 'translate-x-full',
        )}
      >
        <div className="flex h-16 shrink-0 items-center justify-between border-b border-border-primary px-4">
          <Link to="/" className="shrink-0" onClick={() => setMenuOpen(false)}>
            <TawzeefLogo height={26} color="#1A23C3" accentColor="#FFE343" />
          </Link>
          <button
            type="button"
            onClick={() => setMenuOpen(false)}
            aria-label="إغلاق القائمة"
            className="flex size-10 shrink-0 items-center justify-center rounded-200 text-content-primary transition-colors duration-150 hover:bg-brand-soft"
          >
            <CloseIcon />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto px-4 py-3">
          <nav className="flex flex-col gap-0.5">{renderNavLinks(mobileNavLinkClasses)}</nav>
        </div>

        <div className="flex shrink-0 flex-col gap-3 border-t border-border-primary px-4 py-3">
          {isAuthenticated && (
            <span className="px-3 text-[14px] text-content-primary">
              {t('common:welcomeUser', { name: user?.fullName })}
            </span>
          )}
          {isAuthenticated ? (
            <Button variant="secondary" size="sm" fullWidth onClick={clearSession}>
              {t('common:logout')}
            </Button>
          ) : (
            <Link to="/login" className="block" onClick={() => setMenuOpen(false)}>
              <Button variant="primary" size="sm" fullWidth>
                {t('auth:loginLink')}
              </Button>
            </Link>
          )}
        </div>
      </div>
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

function MenuIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M4 7h16M4 12h16M4 17h16" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  )
}

function CloseIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M6 6l12 12M18 6L6 18" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  )
}
