import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { useAccess } from '@/shared/auth/useAccess'
import { useReviewQueue as useJobSeekerReviewQueue } from '@/features/office-jobseeker-review/api/officeJobSeekerReview.queries'
import { useReviewQueue as useEmployerReviewQueue } from '@/features/office-employer-review/api/officeEmployerReview.queries'
import { useReviewQueue as useJobAdReviewQueue } from '@/features/office-job-ads-review/api/officeJobAdsReview.queries'
import { useComplaintQueue } from '@/features/office-complaints-review/api/officeComplaintReview.queries'
import { useSuggestionQueue } from '@/features/office-suggestions-review/api/officeSuggestionsReview.queries'
import { useDueFollowUps } from '@/features/interviews-and-reports/api/interviewsAndReports.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'

/** The page any office member (OfficeAdmin or OfficeStaff) lands on right after login — an
 * overview of what needs their attention, instead of dropping straight into one review queue
 * with no context. Each tile only fetches (and only renders) if the signed-in member actually
 * holds that queue's permission, mirroring the exact same gates AppLayout's nav already uses —
 * an OfficeStaff member with a narrow grant set sees fewer tiles, not a 403. */
export default function OfficeDashboardPage() {
  const { t } = useTranslation([
    'officeDashboard',
    'officeJobSeekerReview',
    'officeEmployerReview',
    'officeJobAdsReview',
    'officeComplaintReview',
    'officeSuggestionsReview',
    'interviewsAndReports',
  ])
  const user = useSessionStore((s) => s.user)
  const { hasPermission } = useAccess()

  const canReviewJobSeekers = hasPermission('jobseekers.view')
  const canReviewEmployers = hasPermission('employers.view')
  const canReviewJobAds = hasPermission('jobadvertisements.review')
  const canReviewComplaints = hasPermission('complaints.view')
  const canReviewSuggestions = hasPermission('suggestions.view')
  const canViewFollowUps = user?.role !== 'Employer' && user?.role !== 'JobSeeker' && hasPermission('hiringfollowupreports.view')

  // Hooks can't be called conditionally, so every queue is always queried with pageSize: 1 (just
  // enough for the totalCount header) — a tile whose permission check above is false never uses
  // its result (filtered out of `tiles` below), so its request either never fires in practice
  // (React Query dedupes against the same queue page already cached from that feature's own list
  // page) or quietly 403s in the background with nothing rendered from it.
  const pendingJobSeekers = useJobSeekerReviewQueue({ reviewStatus: 'Submitted', pageIndex: 0, pageSize: 1 })
  const pendingEmployers = useEmployerReviewQueue({ verificationStatus: 'Submitted', pageIndex: 0, pageSize: 1 })
  const pendingJobAds = useJobAdReviewQueue({ status: 'SubmittedForReview', pageIndex: 0, pageSize: 1 })
  const awaitingPayment = useJobAdReviewQueue({ status: 'ApprovedAwaitingPayment', pageIndex: 0, pageSize: 1 })
  const newComplaints = useComplaintQueue({ status: 'New', pageIndex: 0, pageSize: 1 })
  const newSuggestions = useSuggestionQueue({ status: 'New', pageIndex: 0, pageSize: 1 })
  const dueFollowUps = useDueFollowUps({ pageIndex: 0, pageSize: 1 })

  const tiles = [
    canReviewJobSeekers && {
      label: t('officeDashboard:pendingJobSeekersLabel'),
      value: pendingJobSeekers.data?.pagination.totalCount,
      to: '/office/job-seekers',
    },
    canReviewEmployers && {
      label: t('officeDashboard:pendingEmployersLabel'),
      value: pendingEmployers.data?.pagination.totalCount,
      to: '/office/employers',
    },
    canReviewJobAds && {
      label: t('officeDashboard:pendingJobAdsLabel'),
      value: pendingJobAds.data?.pagination.totalCount,
      to: '/office/job-advertisements',
    },
    canReviewJobAds && {
      label: t('officeDashboard:awaitingPaymentLabel'),
      value: awaitingPayment.data?.pagination.totalCount,
      to: '/office/job-advertisements',
    },
    canReviewComplaints && {
      label: t('officeDashboard:newComplaintsLabel'),
      value: newComplaints.data?.pagination.totalCount,
      to: '/office/complaints',
    },
    canReviewSuggestions && {
      label: t('officeDashboard:newSuggestionsLabel'),
      value: newSuggestions.data?.pagination.totalCount,
      to: '/office/suggestions',
    },
    canViewFollowUps && {
      label: t('officeDashboard:dueFollowUpsLabel'),
      value: dueFollowUps.data?.pagination.totalCount,
      to: '/office/hiring-follow-ups',
    },
  ].filter(Boolean) as { label: string; value: number | undefined; to: string }[]

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('officeDashboard:dashboardGreeting', { name: user?.fullName ?? '' })} />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {tiles.map((tile) => (
          <Link key={tile.label} to={tile.to}>
            <Card className="flex h-full flex-col gap-2 transition-[transform,box-shadow] duration-200 hover:-translate-y-1 hover:border-brand hover:shadow-200">
              <p className="text-sm font-bold text-content-secondary">{tile.label}</p>
              <p className="text-3xl font-extrabold text-brand tabular-nums">{tile.value ?? '—'}</p>
            </Card>
          </Link>
        ))}
      </div>

      {tiles.length === 0 && (
        <Card>
          <p className="text-sm text-content-secondary">{t('officeDashboard:quickLinksSection')}</p>
        </Card>
      )}
    </div>
  )
}
