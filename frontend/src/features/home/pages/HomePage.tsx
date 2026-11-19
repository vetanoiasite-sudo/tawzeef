import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { useJobSearch } from '@/features/public-jobs/api/publicJobs.queries'
import type { JobListItem } from '@/features/public-jobs/types/publicJobs.types'
import { Button } from '@/shared/ui/Button'

/** Egyptian governorates surfaced on the home page for quick browse-by-city.
 * Free-text filter on the backend today (no City lookup table yet — see the
 * known-gaps section in docs/UI_UX_GUIDELINES.md replacement notes). */
const TOP_CITIES = [
  'القاهرة',
  'الجيزة',
  'الإسكندرية',
  'الدقهلية',
  'الشرقية',
  'الغربية',
  'المنوفية',
  'القليوبية',
] as const

/** Hard-coded categories — mirrors the categories used across the ad flow. */
const TOP_CATEGORIES = [
  { key: 'office', label: 'إداري وسكرتارية' },
  { key: 'sales', label: 'مبيعات' },
  { key: 'accounting', label: 'محاسبة ومالية' },
  { key: 'it', label: 'تكنولوجيا المعلومات' },
  { key: 'retail', label: 'خدمة العملاء' },
  { key: 'logistics', label: 'لوجستيات ونقل' },
  { key: 'engineering', label: 'هندسة وتشغيل' },
  { key: 'health', label: 'صحة وتمريض' },
] as const

export default function HomePage() {
  return (
    <div className="flex flex-col">
      <HeroSearch />
      <LatestJobs />
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

  function onSearch(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    const params = new URLSearchParams()
    if (keyword.trim()) params.set('keyword', keyword.trim())
    if (city.trim()) params.set('city', city.trim())
    void navigate(`/jobs${params.toString() ? `?${params.toString()}` : ''}`)
  }

  return (
    <section
      className="relative isolate overflow-hidden"
      style={{
        backgroundImage: 'url(/images/auth-bg.jpg)',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
      }}
    >
      <div
        aria-hidden
        className="absolute inset-0 -z-10"
        style={{ background: 'linear-gradient(180deg, rgba(0,16,24,0.72) 0%, rgba(18,24,135,0.72) 100%)' }}
      />
      <div className="mx-auto flex max-w-6xl flex-col items-center gap-6 px-4 py-20 text-center text-white sm:py-28">
        <h1 className="max-w-3xl text-[36px] font-extrabold leading-[1.2] sm:text-[48px]">
          وظيفتك المناسبة تبدأ من هنا
        </h1>
        <p className="max-w-2xl text-[16px] leading-[26px] text-white/85 sm:text-[18px]">
          منصة توظيف موثوقة مدارة من مكاتب العمل الكنسية — تصفّح، قدِّم، وقابل أصحاب
          العمل عن طريق مكتبك.
        </p>

        <form
          onSubmit={onSearch}
          className="mt-4 flex w-full max-w-3xl flex-col gap-2 rounded-300 bg-white p-2 shadow-200 sm:flex-row"
        >
          <div className="flex min-w-0 flex-1 items-center gap-2 border-border-primary px-3 sm:border-e">
            <SearchIcon />
            <input
              type="search"
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              placeholder="عنوان الوظيفة، شركة، كلمة مفتاحية"
              className="min-w-0 flex-1 bg-transparent py-3 text-[15px] text-content-primary outline-none placeholder:text-content-tertiary"
              aria-label="كلمة البحث"
            />
          </div>
          <div className="flex min-w-0 flex-1 items-center gap-2 px-3">
            <PinIcon />
            <input
              type="search"
              value={city}
              onChange={(e) => setCity(e.target.value)}
              placeholder="المحافظة"
              className="min-w-0 flex-1 bg-transparent py-3 text-[15px] text-content-primary outline-none placeholder:text-content-tertiary"
              aria-label="المحافظة"
            />
          </div>
          <Button type="submit" variant="primary" size="lg" className="sm:min-w-[140px]">
            بحث
          </Button>
        </form>
      </div>
    </section>
  )
}

/* -------------------------------------------------------------------------- */
/* Latest jobs                                                                */
/* -------------------------------------------------------------------------- */

function LatestJobs() {
  const { data, isLoading, isError } = useJobSearch({ pageIndex: 0, pageSize: 8 })
  const jobs: JobListItem[] = data?.items ?? []

  return (
    <section className="mx-auto w-full max-w-6xl px-4 py-16">
      <SectionHeader title="أحدث الوظائف" action={<Link to="/jobs" className="text-[14px] font-bold text-brand hover:text-brand-hover hover:underline">عرض كل الوظائف</Link>} />

      {isLoading && <GridSkeleton />}
      {isError && (
        <p className="text-[14px] text-danger">تعذّر تحميل الوظائف. جرّب مرة أخرى بعد قليل.</p>
      )}
      {!isLoading && !isError && jobs.length === 0 && (
        <p className="text-[14px] text-content-secondary">لا توجد وظائف منشورة حالياً.</p>
      )}
      {!isLoading && jobs.length > 0 && (
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {jobs.map((job) => (
            <JobCard key={job.id} job={job} />
          ))}
        </div>
      )}
    </section>
  )
}

function JobCard({ job }: { job: JobListItem }) {
  return (
    <Link
      to={`/jobs/${job.id}`}
      className="group flex h-full flex-col justify-between rounded-300 border border-border-primary bg-surface p-4 transition-colors hover:border-brand"
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

function employmentTypeLabel(t: JobListItem['employmentType']) {
  switch (t) {
    case 'FullTime': return 'دوام كامل'
    case 'PartTime': return 'دوام جزئي'
    case 'Contract': return 'عقد'
    default: return ''
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
        <div key={i} className="h-28 animate-pulse rounded-300 border border-border-primary bg-surface" />
      ))}
    </div>
  )
}

/* -------------------------------------------------------------------------- */
/* Browse by category / city                                                  */
/* -------------------------------------------------------------------------- */

function BrowseByCategory() {
  return (
    <section className="bg-surface">
      <div className="mx-auto w-full max-w-6xl px-4 py-16">
        <SectionHeader title="تصفّح حسب الفئة" />
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {TOP_CATEGORIES.map((c) => (
            <Link
              key={c.key}
              to={`/jobs?category=${encodeURIComponent(c.label)}`}
              className="flex h-20 items-center justify-center rounded-300 border border-border-primary bg-surface px-4 text-center text-[14px] font-bold text-content-primary transition-colors hover:border-brand hover:text-brand"
            >
              {c.label}
            </Link>
          ))}
        </div>
      </div>
    </section>
  )
}

function BrowseByCity() {
  return (
    <section className="mx-auto w-full max-w-6xl px-4 py-16">
      <SectionHeader title="تصفّح حسب المحافظة" />
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4 lg:grid-cols-8">
        {TOP_CITIES.map((city) => (
          <Link
            key={city}
            to={`/jobs?city=${encodeURIComponent(city)}`}
            className="flex h-16 items-center justify-center rounded-200 border border-border-primary bg-surface px-3 text-[14px] font-bold text-content-primary transition-colors hover:border-brand hover:text-brand"
          >
            {city}
          </Link>
        ))}
      </div>
    </section>
  )
}

/* -------------------------------------------------------------------------- */
/* Final CTA                                                                  */
/* -------------------------------------------------------------------------- */

function JoinTawzeefCTA() {
  const { t } = useTranslation('auth')
  return (
    <section className="bg-brand-soft">
      <div className="mx-auto grid w-full max-w-6xl grid-cols-1 gap-4 px-4 py-16 md:grid-cols-2">
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
      </div>
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
    <div className="flex flex-col items-start gap-3 rounded-300 border border-border-primary bg-surface p-6">
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
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden className="shrink-0 text-content-tertiary">
      <circle cx="11" cy="11" r="7" stroke="currentColor" strokeWidth="1.8" />
      <path d="M20 20l-3-3" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}

function PinIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden className="shrink-0 text-content-tertiary">
      <path
        d="M12 22s7-7.5 7-13a7 7 0 1 0-14 0c0 5.5 7 13 7 13Z"
        stroke="currentColor"
        strokeWidth="1.8"
      />
      <circle cx="12" cy="9" r="2.5" stroke="currentColor" strokeWidth="1.8" />
    </svg>
  )
}
