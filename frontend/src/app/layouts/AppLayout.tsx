import clsx from 'clsx'
import { useEffect, useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, Outlet, useLocation } from 'react-router-dom'
import { NotificationBell } from '@/features/notifications'
import { usePublicOffices } from '@/features/auth/api/auth.queries'
import { useMyJobSeekerProfile } from '@/features/jobseeker-profile/api/jobSeekerProfile.queries'
import { jobSeekerProfileApi } from '@/features/jobseeker-profile/api/jobSeekerProfile.api'
import { useMyEmployerProfile } from '@/features/employer-profile/api/employerProfile.queries'
import { useAccess } from '@/shared/auth/useAccess'
import { useSessionStore, type SessionUser } from '@/shared/auth/sessionStore'
import { useScrolled } from '@/shared/hooks/useScrolled'
import { env } from '@/shared/config/env'
import { Button } from '@/shared/ui/Button'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'

const ROLE_LABELS: Record<string, string> = {
  JobSeeker: 'باحث عن عمل',
  Employer: 'صاحب عمل',
  OfficeAdmin: 'مدير مكتب',
  OfficeStaff: 'موظف مكتب',
  SuperAdmin: 'سوبر أدمن',
}

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
    'jobSeekerCourses',
    'officeBanners',
    'officeProfile',
    'officeAccounts',
    'officeEmployedJobSeekers',
  ])
  const { isAuthenticated, hasPermission } = useAccess()
  const user = useSessionStore((s) => s.user)
  const location = useLocation()

  // Confirmed 2026-11-24 (real bug): clearSession alone left the user on whatever protected page
  // they were viewing; RequireAuth then redirected to /login?redirect=<that-same-page>, so logging
  // in again — even with a completely different account/role — landed back on the previous
  // account's page.
  //
  // Neither a plain clearSession() nor clearSession() + SPA navigate('/') actually fixes this
  // (confirmed by live testing, not just reasoning): calling clearSession() flips isAuthenticated
  // synchronously, while a client-side navigate — even with replace: true — does not commit the
  // new location synchronously, so RequireAuth still renders against the OLD location first and
  // wins with its own /login?redirect=<old page>, reproducing the bug even with navigate('/') in
  // the handler. A hard redirect with NO clearSession() call sidesteps the whole race: the token
  // only lives in memory (sessionStore, never localStorage), so the full page unload this triggers
  // wipes it on its own — nothing flips isAuthenticated while the old page (and its RequireAuth)
  // is still mounted, so there's no stale instance left to redirect anywhere.
  function handleLogout() {
    window.location.href = '/'
  }
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

  // Every link lives in the side drawer now, on every screen size — this app's nav list has
  // grown to ~15 items for office roles, which read as a cluttered horizontal bar on desktop
  // long before it would ever wrap on a phone, so a dedicated inline desktop nav isn't kept
  // around for comparison; one drawer behavior to maintain beats two.
  const drawerLinkClasses =
    'flex h-12 items-center rounded-200 px-3 text-[15px] font-bold text-content-secondary transition-colors duration-150 hover:bg-brand-soft hover:text-brand'

  // Parameterized even though only one caller remains below — keeps the ~15-link conditional
  // list itself in one place if a second rendering context ever comes back.
  // Only ever rendered for a signed-in user now — the drawer itself doesn't exist for an
  // anonymous visitor (every one of these links duplicates something already on the home page:
  // jobs via the hero/latest-jobs sections, registration via the bottom CTA cards).
  function renderNavLinks(linkClassName: string) {
    return (
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
        {user?.role === 'JobSeeker' && (
          <Link to="/my-courses" className={linkClassName}>
            {t('jobSeekerCourses:pageTitle')}
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
        {user?.role !== 'Employer' &&
          user?.role !== 'JobSeeker' &&
          hasPermission('hiringfollowupreports.view') && (
            <Link to="/office/hiring-follow-ups" className={linkClassName}>
              {t('interviewsAndReports:dueFollowUpsTitle')}
            </Link>
          )}
        {hasPermission('jobseekers.view') && (
          <Link to="/office/job-seekers" className={linkClassName}>
            {t('officeJobSeekerReview:queueTitle')}
          </Link>
        )}
        {hasPermission('jobseekers.view') && (
          <Link to="/office/job-seekers/employed" className={linkClassName}>
            {t('officeEmployedJobSeekers:pageTitle')}
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
        {hasPermission('offices.view') && (
          <Link to="/office/profile" className={linkClassName}>
            {t('officeProfile:pageTitle')}
          </Link>
        )}
        {hasPermission('banners.view') && (
          <Link to="/office/banners" className={linkClassName}>
            {t('officeBanners:pageTitle')}
          </Link>
        )}
        {hasPermission('advertisementpayments.view') && (
          <Link to="/office/accounts" className={linkClassName}>
            {t('officeAccounts:pageTitle')}
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
    )
  }

  // Welcome text + logout move into the drawer footer below (every screen size now, not just
  // mobile) — the header itself only ever needs the sign-in CTA for an anonymous visitor.
  const sessionControls = isAuthenticated ? null : (
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
          {/* Logo (start) · session controls + hamburger (end) — the nav link list itself
           *  always lives in the side drawer, never inline here (see drawerLinkClasses above). */}
          <div className="flex flex-1 items-center">
            <Link to="/" className="shrink-0">
              <TawzeefLogo
                height={28}
                color={overHero ? '#FFFFFF' : '#1A23C3'}
                accentColor="#FFE343"
              />
            </Link>
          </div>

          <div className="flex flex-1 items-center justify-end gap-2">
            {sessionControls}

            {/* Notification bell (when signed in) stays visible next to the toggle, on every
             *  screen size — the welcome text / logout combo moved into the drawer footer. */}
            {isAuthenticated && <NotificationBell />}

            {/* The drawer itself only exists for a signed-in user (see renderNavLinks' own
             *  comment) — an anonymous visitor gets just the logo and sign-in button above,
             *  no hamburger at all. */}
            {isAuthenticated && (
              <button
                type="button"
                onClick={() => setMenuOpen((v) => !v)}
                aria-label={menuOpen ? 'إغلاق القائمة' : 'فتح القائمة'}
                aria-expanded={menuOpen}
                className={clsx(
                  'flex size-10 shrink-0 items-center justify-center rounded-200 transition-colors duration-150',
                  overHero
                    ? 'text-white hover:bg-white/10'
                    : 'text-content-primary hover:bg-brand-soft',
                )}
              >
                {menuOpen ? <CloseIcon /> : <MenuIcon />}
              </button>
            )}
          </div>
        </div>
      </header>

      {isAuthenticated && (
        <>
          {/* Nav drawer — slides in from the end edge (the left, in RTL) matching the menu icon
           *  position, on every screen size (see drawerLinkClasses above). Backdrop click, Escape,
           *  or a route change all close it. */}
          <div
            aria-hidden={!menuOpen}
            className={clsx(
              'fixed inset-0 z-40 bg-black/40 transition-opacity duration-300',
              menuOpen ? 'pointer-events-auto opacity-100' : 'pointer-events-none opacity-0',
            )}
            onClick={() => setMenuOpen(false)}
          />
          <div
            role="dialog"
            aria-modal="true"
            aria-label="القائمة"
            className={clsx(
              'fixed inset-y-0 end-0 z-50 flex w-[85%] max-w-[340px] flex-col bg-surface shadow-[0_0_32px_rgba(0,0,0,0.18)]',
              'transition-transform duration-300 ease-out',
              menuOpen
                ? 'translate-x-0'
                : '-translate-x-full rtl:-translate-x-full ltr:translate-x-full',
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

            {/* Profile header — the account's own photo (job seeker photo / company logo / office
             *  photo), name and role, always first, with the link list below it. */}
            {user && (
              <div className="flex shrink-0 items-center gap-3 border-b border-border-primary bg-gradient-to-b from-brand-soft/70 to-transparent px-4 py-5">
                <DrawerAvatar user={user} />
                <div className="flex min-w-0 flex-col">
                  <p className="truncate text-[16px] font-bold text-content-primary">{user.fullName}</p>
                  <p className="text-[13px] font-medium text-brand">{ROLE_LABELS[user.role] ?? user.role}</p>
                </div>
              </div>
            )}

            <div className="flex-1 overflow-y-auto px-4 py-3">
              <nav className="flex flex-col gap-0.5">{renderNavLinks(drawerLinkClasses)}</nav>
            </div>

            <div className="flex shrink-0 flex-col gap-3 border-t border-border-primary px-4 py-3">
              <Button variant="secondary" size="sm" fullWidth onClick={handleLogout}>
                {t('common:logout')}
              </Button>
            </div>
          </div>
        </>
      )}
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

function AvatarFrame({ children }: { children: ReactNode }) {
  return (
    <div className="flex size-14 shrink-0 items-center justify-center overflow-hidden rounded-full border-2 border-white bg-surface text-brand shadow-[0_2px_8px_rgba(0,0,0,0.1)]">
      {children}
    </div>
  )
}

/** Dispatches to the right photo source per role — a job seeker's own uploaded photo (private,
 * fetched as an authenticated blob), an employer's company logo or an office's photo (both public
 * URLs, served straight from `<img src>`), or a generic icon for SuperAdmin (no photo concept). */
function DrawerAvatar({ user }: { user: SessionUser }) {
  switch (user.role) {
    case 'JobSeeker':
      return <JobSeekerAvatar />
    case 'Employer':
      return <EmployerAvatar />
    case 'OfficeAdmin':
    case 'OfficeStaff':
      return <OfficeAvatar officeId={user.officeId} />
    default:
      return (
        <AvatarFrame>
          <ShieldIcon />
        </AvatarFrame>
      )
  }
}

function JobSeekerAvatar() {
  const { data: profile } = useMyJobSeekerProfile()
  const [photoUrl, setPhotoUrl] = useState<string | null>(null)

  useEffect(() => {
    if (!profile?.hasPhoto) {
      setPhotoUrl(null)
      return
    }
    let objectUrl: string | null = null
    let cancelled = false
    void jobSeekerProfileApi.getPhoto().then(({ blob }) => {
      if (cancelled) return
      objectUrl = URL.createObjectURL(blob)
      setPhotoUrl(objectUrl)
    })
    return () => {
      cancelled = true
      if (objectUrl) URL.revokeObjectURL(objectUrl)
    }
  }, [profile?.hasPhoto])

  return (
    <AvatarFrame>
      {photoUrl ? <img src={photoUrl} alt="" className="size-full object-cover" /> : <PersonIcon />}
    </AvatarFrame>
  )
}

function EmployerAvatar() {
  const { data: profile } = useMyEmployerProfile()
  return (
    <AvatarFrame>
      {profile?.logoUrl ? (
        <img src={`${env.apiBaseUrl}${profile.logoUrl}`} alt="" className="size-full object-cover" />
      ) : (
        <BuildingIcon />
      )}
    </AvatarFrame>
  )
}

function OfficeAvatar({ officeId }: { officeId: string | null }) {
  const { data: offices } = usePublicOffices()
  const office = offices?.find((o) => o.id === officeId)
  return (
    <AvatarFrame>
      {office?.logoUrl ? (
        <img src={`${env.apiBaseUrl}${office.logoUrl}`} alt="" className="size-full object-cover" />
      ) : (
        <BuildingIcon />
      )}
    </AvatarFrame>
  )
}

function PersonIcon() {
  return (
    <svg width="26" height="26" viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="8" r="4" stroke="currentColor" strokeWidth="1.8" />
      <path d="M4 20c0-3.3 3.6-6 8-6s8 2.7 8 6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}

function BuildingIcon() {
  return (
    <svg width="26" height="26" viewBox="0 0 24 24" fill="none" aria-hidden>
      <rect x="4" y="3" width="16" height="18" rx="1" stroke="currentColor" strokeWidth="1.8" />
      <path
        d="M8 8h2M14 8h2M8 12h2M14 12h2M8 16h2M14 16h2"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </svg>
  )
}

function ShieldIcon() {
  return (
    <svg width="26" height="26" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M12 3l7 3v6c0 4.5-3 7.8-7 9-4-1.2-7-4.5-7-9V6l7-3Z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
      <path d="M9 12l2 2 4-4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  )
}

function MenuIcon() {
  return (
    <svg width="22" height="22" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M4 7h16M4 12h16M4 17h16"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
      />
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
