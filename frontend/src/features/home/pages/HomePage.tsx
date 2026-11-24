import clsx from 'clsx'
import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { useJobSearch, usePublicCities } from '@/features/public-jobs'
import type { JobListItem } from '@/features/public-jobs'
import { useJobCategories } from '@/features/job-categories'
import { useGovernorates } from '@/features/governorates'
import { useCourseSearch, CourseCard } from '@/features/public-courses'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { Button } from '@/shared/ui/Button'
import { Reveal } from '@/shared/ui/Reveal'
import { BannerSlider } from '../components/BannerSlider'

export default function HomePage() {
  return (
    <div className="flex flex-col">
      <HeroSearch />
      <LatestJobs />
      <BrowseByCourses />
      {/* Office-authored promotional banners — a plain in-page block between sections, never a
       *  popup, so a visitor scrolling past it is never interrupted (renders nothing if empty). */}
      <BannerSlider />
      <BrowseByCategory />
      <BrowseByCity />
      <JoinTawzeefCTA />
    </div>
  )
}

/* -------------------------------------------------------------------------- */
/* Hero                                                                       */
/* -------------------------------------------------------------------------- */

function HeroSearch() {
  const navigate = useNavigate()
  const [keyword, setKeyword] = useState('')
  const [city, setCity] = useState('')

  // Read the total open-jobs count for the hero "X open jobs" badge.
  const countQuery = useJobSearch({ pageIndex: 0, pageSize: 1 })
  const totalOpenJobs = countQuery.data?.pagination?.totalCount ?? 0

  function onSearch(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    const params = new URLSearchParams()
    if (keyword.trim()) params.set('keyword', keyword.trim())
    if (city.trim()) params.set('city', city.trim())
    void navigate(`/jobs${params.toString() ? `?${params.toString()}` : ''}`)
  }

  function scrollPastHero() {
    window.scrollTo({ top: window.innerHeight, behavior: 'smooth' })
  }

  return (
    <section
      className="relative isolate -mt-16 flex min-h-screen w-full flex-col overflow-hidden"
      style={{
        backgroundImage: 'url(/images/auth-bg.jpg)',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
      }}
    >
      <div
        aria-hidden
        className="absolute inset-0 -z-10"
        style={{
          background: 'linear-gradient(180deg, rgba(0,16,24,0.72) 0%, rgba(18,24,135,0.72) 100%)',
        }}
      />

      <div className="flex w-full flex-1 flex-col items-center justify-center gap-6 px-4 pb-16 pt-24 text-center text-white sm:px-8 sm:pb-20 sm:pt-32 xl:px-16 2xl:px-24">
        <h1 className="max-w-3xl text-[40px] font-extrabold leading-[1.25] sm:text-[56px] sm:leading-[1.2] xl:text-[64px]">
          وظيفتك المناسبة
          <br />
          تبدأ من{' '}
          <span className="text-[#FFE343] underline decoration-brand decoration-[6px] underline-offset-[14px]">
            هنا
          </span>
        </h1>
        <p className="max-w-[460px] text-[16px] leading-[26px] text-white/85 sm:text-[18px] sm:leading-[30px]">
          منصة توظيف موثوقة من مكاتب العمل الكنسية — تصفّح وقدِّم عن طريق مكتبك.
        </p>

        <form
          onSubmit={onSearch}
          className="mt-6 flex w-full max-w-4xl flex-col gap-2 rounded-300 bg-white p-2 shadow-200 sm:flex-row sm:items-center"
        >
          <div className="flex min-w-0 flex-[2] items-center gap-2 border-border-primary px-3 sm:border-e">
            <SearchIcon />
            <input
              type="search"
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="ابحث بالمسمى الوظيفي أو الكلمة المفتاحية"
              className="min-w-0 flex-1 bg-transparent py-3 text-[15px] text-content-primary outline-none placeholder:text-content-tertiary"
              aria-label="كلمة البحث"
            />
          </div>

          <CityAutocomplete value={city} onChange={setCity} />

          {/* Open-jobs count lives inside the bar, between the city field and the
           *  submit button — like the Wuzzuf-style "X Open Jobs" badge. */}
          <span className="hidden whitespace-nowrap px-3 text-[13px] font-medium text-content-tertiary sm:inline">
            {countQuery.isLoading ? '...' : `${totalOpenJobs.toLocaleString('ar-EG')} وظيفة متاحة`}
          </span>

          <Button type="submit" variant="primary" size="lg" className="sm:min-w-[120px]">
            بحث
          </Button>
        </form>
      </div>

      {/* Scroll-down affordance at the bottom of the hero */}
      <button
        type="button"
        onClick={scrollPastHero}
        aria-label="التمرير لعرض المزيد"
        className="relative z-10 mx-auto mb-10 flex h-11 w-11 animate-bounce items-center justify-center rounded-full border border-white/30 bg-white/10 text-white backdrop-blur-sm transition-colors hover:border-white hover:bg-white/20"
      >
        <ChevronDownIcon />
      </button>
    </section>
  )
}

function ChevronDownIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M6 9l6 6 6-6"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  )
}

/* -------------------------------------------------------------------------- */
/* City autocomplete — type-ahead picker for the hero search                  */
/* -------------------------------------------------------------------------- */

function CityAutocomplete({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const [open, setOpen] = useState(false)
  const [highlight, setHighlight] = useState(0)
  const rootRef = useRef<HTMLDivElement | null>(null)

  const { data: governorates } = useGovernorates()
  const names = (governorates ?? []).map((g) => g.name)
  const query = value.trim()
  const suggestions = query ? names.filter((g) => g.includes(query)).slice(0, 8) : names.slice(0, 8)

  // Close on outside click.
  useEffect(() => {
    if (!open) return
    function onDown(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', onDown)
    return () => document.removeEventListener('mousedown', onDown)
  }, [open])

  function selectSuggestion(s: string) {
    onChange(s)
    setOpen(false)
  }

  function onKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
    if (!open) {
      if (e.key === 'ArrowDown') {
        setOpen(true)
        e.preventDefault()
      }
      return
    }
    if (e.key === 'ArrowDown') {
      setHighlight((h) => Math.min(h + 1, suggestions.length - 1))
      e.preventDefault()
    } else if (e.key === 'ArrowUp') {
      setHighlight((h) => Math.max(h - 1, 0))
      e.preventDefault()
    } else if (e.key === 'Enter') {
      if (suggestions[highlight]) {
        selectSuggestion(suggestions[highlight])
        e.preventDefault()
      }
    } else if (e.key === 'Escape') {
      setOpen(false)
    }
  }

  return (
    <div ref={rootRef} className="relative flex min-w-0 flex-1">
      <div className="flex min-w-0 flex-1 items-center gap-2 px-3">
        <PinIcon />
        <input
          type="text"
          value={value}
          onChange={(e) => {
            onChange(e.target.value)
            setOpen(true)
            setHighlight(0)
          }}
          onFocus={() => setOpen(true)}
          onKeyDown={onKeyDown}
          placeholder="المحافظة"
          aria-label="المحافظة"
          aria-autocomplete="list"
          aria-expanded={open}
          role="combobox"
          className="min-w-0 flex-1 bg-transparent py-3 text-[15px] text-content-primary outline-none placeholder:text-content-tertiary"
        />
      </div>
      {open && suggestions.length > 0 && (
        <ul
          role="listbox"
          className="absolute end-0 start-0 top-full z-20 mt-1 max-h-72 overflow-auto rounded-200 border border-border-primary bg-white py-1 text-start shadow-200"
        >
          {suggestions.map((s, i) => (
            <li
              key={s}
              role="option"
              aria-selected={i === highlight}
              onMouseDown={(e) => {
                e.preventDefault()
                selectSuggestion(s)
              }}
              onMouseEnter={() => setHighlight(i)}
              className={clsx(
                'cursor-pointer px-4 py-2 text-[14px] text-content-primary',
                i === highlight ? 'bg-brand-soft text-brand' : 'hover:bg-surface-muted',
              )}
            >
              {s}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

/* -------------------------------------------------------------------------- */
/* Latest jobs                                                                */
/* -------------------------------------------------------------------------- */

function LatestJobs() {
  const { data, isLoading, isError } = useJobSearch({ pageIndex: 0, pageSize: 8 })
  const jobs: JobListItem[] = data?.items ?? []

  return (
    <section className="w-full px-4 py-16 sm:px-8 xl:px-16 2xl:px-24">
      <Reveal>
        <SectionHeader
          title="أحدث الوظائف"
          action={
            <Link
              to="/jobs"
              className="text-[14px] font-bold text-brand underline underline-offset-4 decoration-2 hover:text-brand-hover hover:decoration-brand-hover"
            >
              عرض كل الوظائف
            </Link>
          }
        />
      </Reveal>

      {isLoading && <GridSkeleton />}
      {isError && (
        <p className="text-[14px] text-danger">تعذّر تحميل الوظائف. جرّب مرة أخرى بعد قليل.</p>
      )}
      {!isLoading && !isError && jobs.length === 0 && (
        <p className="text-[14px] text-content-secondary">لا توجد وظائف منشورة حالياً.</p>
      )}
      {!isLoading && jobs.length > 0 && (
        <Reveal delayMs={100} className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {jobs.map((job) => (
            <JobCard key={job.id} job={job} />
          ))}
        </Reveal>
      )}
    </section>
  )
}

function JobCard({ job }: { job: JobListItem }) {
  return (
    <Link
      to={`/jobs/${job.id}`}
      className="group flex h-full flex-col justify-between rounded-300 border border-border-primary bg-surface p-4 transition-[transform,box-shadow,border-color] duration-200 hover:-translate-y-1 hover:border-brand hover:shadow-200"
    >
      <div className="flex flex-col gap-1.5">
        <h3 className="line-clamp-2 text-[15px] font-bold leading-[20px] text-content-primary group-hover:text-brand">
          {job.title}
        </h3>
        <p className="line-clamp-1 text-[13px] text-content-secondary">{job.employerDisplayName}</p>
      </div>
      <div className="mt-3 flex flex-wrap items-center gap-2 text-[12px]">
        {job.city && <Chip>{job.city}</Chip>}
        {job.employmentType && <Chip>{employmentTypeLabel(job.employmentType)}</Chip>}
      </div>
    </Link>
  )
}

/* -------------------------------------------------------------------------- */
/* Courses                                                                    */
/* -------------------------------------------------------------------------- */

/** Mirrors LatestJobs exactly — a visitor who isn't signed in has nowhere else to discover
 * courses from now (AppLayout's drawer only exists for a signed-in user), so this section is
 * the one place that gets them to /courses. Renders nothing while every office's courses are
 * paused (no "section" with an empty grid underneath it). */
function BrowseByCourses() {
  const { data, isLoading, isError } = useCourseSearch({ pageIndex: 0, pageSize: 8 })
  const courses = data?.items ?? []

  if (!isLoading && !isError && courses.length === 0) return null

  return (
    <section className="w-full px-4 py-16 sm:px-8 xl:px-16 2xl:px-24">
      <Reveal>
        <SectionHeader
          title="الكورسات"
          action={
            <Link
              to="/courses"
              className="text-[14px] font-bold text-brand underline underline-offset-4 decoration-2 hover:text-brand-hover hover:decoration-brand-hover"
            >
              عرض كل الكورسات
            </Link>
          }
        />
      </Reveal>

      {isLoading && <GridSkeleton />}
      {isError && (
        <p className="text-[14px] text-danger">تعذّر تحميل الكورسات. جرّب مرة أخرى بعد قليل.</p>
      )}
      {!isLoading && courses.length > 0 && (
        <Reveal delayMs={100} className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {courses.map((course) => (
            <CourseCard key={course.id} course={course} />
          ))}
        </Reveal>
      )}
    </section>
  )
}

function employmentTypeLabel(t: JobListItem['employmentType']) {
  switch (t) {
    case 'FullTime':
      return 'دوام كامل'
    case 'PartTime':
      return 'دوام جزئي'
    case 'Contract':
      return 'عقد'
    default:
      return ''
  }
}

function Chip({ children }: { children: React.ReactNode }) {
  return (
    <span className="inline-flex items-center rounded-100 bg-brand-soft px-2 py-1 text-[12px] font-medium text-brand">
      {children}
    </span>
  )
}

function GridSkeleton() {
  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
      {Array.from({ length: 8 }).map((_, i) => (
        <div
          key={i}
          className="h-28 animate-pulse rounded-300 border border-border-primary bg-surface"
        />
      ))}
    </div>
  )
}

/* -------------------------------------------------------------------------- */
/* Browse by category / city                                                  */
/* -------------------------------------------------------------------------- */

/** "عرض الكل" — expands the section in place to show every item instead of navigating away
 * (a visitor asking to see all categories/cities wants more tiles, not the job list). Toggles
 * back to "عرض أقل" once expanded. */
function ExpandToggle({ expanded, onToggle }: { expanded: boolean; onToggle: () => void }) {
  return (
    <button
      type="button"
      onClick={onToggle}
      className="text-[14px] font-bold text-brand underline underline-offset-4 decoration-2 hover:text-brand-hover hover:decoration-brand-hover"
    >
      {expanded ? 'عرض أقل' : 'عرض الكل'}
    </button>
  )
}

const COLLAPSED_COUNT = 8

function BrowseByCategory() {
  // Real, office-managed categories now (job-categories feature) — not a hardcoded list that
  // could drift from what ads are actually tagged with. A ad's category only exists once an
  // office assigns one during review, so this section is empty until at least one has been.
  const { data: categories } = useJobCategories()
  const [expanded, setExpanded] = useState(false)
  const all = categories ?? []

  if (all.length === 0) return null
  const shown = expanded ? all : all.slice(0, COLLAPSED_COUNT)

  return (
    <section className="bg-brand-soft">
      <div className="w-full px-4 py-16 sm:px-8 xl:px-16 2xl:px-24">
        <Reveal>
          <SectionHeader
            title="تصفّح حسب الفئة"
            action={
              all.length > COLLAPSED_COUNT && (
                <ExpandToggle expanded={expanded} onToggle={() => setExpanded((e) => !e)} />
              )
            }
          />
        </Reveal>
        <Reveal delayMs={100} className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {shown.map((c) => (
            <Link
              key={c.id}
              to={`/jobs?category=${encodeURIComponent(c.name)}`}
              className="group flex h-24 flex-col items-center justify-center gap-2 rounded-300 border border-border-primary bg-surface px-4 text-center transition-[transform,box-shadow,border-color] duration-200 hover:-translate-y-1 hover:border-brand hover:shadow-200"
            >
              <span className="text-content-tertiary transition-colors group-hover:text-brand">
                <TagIcon />
              </span>
              <span className="text-[14px] font-bold text-content-primary group-hover:text-brand">
                {c.name}
              </span>
            </Link>
          ))}
        </Reveal>
      </div>
    </section>
  )
}

function BrowseByCity() {
  // Real cities carried by a live ad (public-jobs feature's /api/jobs/cities) — not a static
  // governorate list, so this reflects where jobs actually are, ordered by how many.
  const { data: cities } = usePublicCities()
  const [expanded, setExpanded] = useState(false)
  const all = cities ?? []

  if (all.length === 0) return null
  const shown = expanded ? all : all.slice(0, COLLAPSED_COUNT)

  return (
    <section className="w-full px-4 py-16 sm:px-8 xl:px-16 2xl:px-24">
      <Reveal>
        <SectionHeader
          title="تصفّح حسب المحافظة"
          action={
            all.length > COLLAPSED_COUNT && (
              <ExpandToggle expanded={expanded} onToggle={() => setExpanded((e) => !e)} />
            )
          }
        />
      </Reveal>
      <Reveal delayMs={100} className="grid grid-cols-2 gap-3 sm:grid-cols-4 lg:grid-cols-8">
        {shown.map((c) => (
          <Link
            key={c.city}
            to={`/jobs?city=${encodeURIComponent(c.city)}`}
            className="flex h-16 items-center justify-center rounded-200 border border-border-primary bg-surface px-3 text-[14px] font-bold text-content-primary transition-[transform,box-shadow,border-color] duration-200 hover:-translate-y-0.5 hover:border-brand hover:text-brand hover:shadow-100"
          >
            {c.city}
          </Link>
        ))}
      </Reveal>
    </section>
  )
}

/* -------------------------------------------------------------------------- */
/* Final CTA                                                                  */
/* -------------------------------------------------------------------------- */

function JoinTawzeefCTA() {
  const { t } = useTranslation('auth')
  // Only for an anonymous visitor — a signed-in user (job seeker, employer, office, SuperAdmin)
  // already has an account, so "register as X" cards here were a confusing non-sequitur.
  const user = useSessionStore((s) => s.user)
  if (user) return null

  return (
    <section className="bg-brand-soft">
      <Reveal className="grid w-full grid-cols-1 gap-4 px-4 py-16 sm:px-8 md:grid-cols-2 xl:px-16 2xl:px-24">
        <CtaCard
          title="هل تبحث عن عمل؟"
          body="سجّل حسابك وقدّم على وظائف موثوقة من خلال مكتبك."
          actionHref="/register/job-seeker"
          actionLabel={t('registerLink')}
        />
        <CtaCard
          title="هل أنت صاحب عمل؟"
          body="انشر إعلانك وقابل مرشّحين مرشّحين مسبقاً من المكتب."
          actionHref="/register/employer"
          actionLabel={t('registerEmployerLink')}
        />
      </Reveal>
    </section>
  )
}

function CtaCard({
  title,
  body,
  actionHref,
  actionLabel,
}: {
  title: string
  body: string
  actionHref: string
  actionLabel: string
}) {
  return (
    <div className="flex flex-col items-start gap-3 rounded-300 border border-border-primary bg-surface p-6 transition-[transform,box-shadow] duration-200 hover:-translate-y-1 hover:shadow-200">
      <h3 className="text-[20px] font-bold text-content-primary">{title}</h3>
      <p className="text-[14px] leading-[22px] text-content-secondary">{body}</p>
      <Link to={actionHref} className="mt-auto">
        <Button variant="primary" size="md">
          {actionLabel}
        </Button>
      </Link>
    </div>
  )
}

/* -------------------------------------------------------------------------- */
/* Helpers                                                                    */
/* -------------------------------------------------------------------------- */

function SectionHeader({ title, action }: { title: string; action?: React.ReactNode }) {
  return (
    <div className="mb-6 flex items-end justify-between gap-4">
      <h2 className="text-[24px] font-bold text-content-primary sm:text-[28px]">{title}</h2>
      {action}
    </div>
  )
}

function SearchIcon() {
  return (
    <svg
      width="18"
      height="18"
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden
      className="shrink-0 text-content-tertiary"
    >
      <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="1.8" />
      <path d="M20 20l-3-3" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}

function PinIcon() {
  return (
    <svg
      width="18"
      height="18"
      viewBox="0 0 24 24"
      fill="none"
      aria-hidden
      className="shrink-0 text-content-tertiary"
    >
      <path
        d="M12 22s7-7.5 7-13a7 7 0 1 0-14 0c0 5.5 7 13 7 13Z"
        stroke="currentColor"
        strokeWidth="1.8"
      />
      <circle cx="12" cy="9" r="2.5" stroke="currentColor" strokeWidth="1.8" />
    </svg>
  )
}

/* -------------------------------------------------------------------------- */
/* Category tile icon — one generic tag icon now that categories are a real, */
/* open-ended list an office can grow (no fixed set to key custom icons on). */
/* -------------------------------------------------------------------------- */

function TagIcon() {
  return (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M20 12.5 12.5 20a1.5 1.5 0 0 1-2.12 0l-6.38-6.38a1.5 1.5 0 0 1 0-2.12L11.5 4H18a2 2 0 0 1 2 2v6.5Z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
      <circle cx="15" cy="9" r="1.3" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  )
}
