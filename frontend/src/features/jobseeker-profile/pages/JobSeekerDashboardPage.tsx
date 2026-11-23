import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { useMyJobSeekerProfile } from '../api/jobSeekerProfile.queries'
import { useMyApplications } from '@/features/job-seeker-applications/api/jobSeekerApplications.queries'
import { useJobSearch } from '@/features/public-jobs/api/publicJobs.queries'
import { JobCard } from '@/features/public-jobs/components/JobCard'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'
import { EmptyState } from '@/shared/ui/EmptyState'

/** The page a Job Seeker lands on right after login — a real dashboard (profile status + an
 * applications count + a jobs section) so logging in visibly changes what's on screen, instead of
 * dropping back onto the same public homepage a logged-out visitor sees. */
export default function JobSeekerDashboardPage() {
  const { t } = useTranslation(['jobSeekerProfile', 'jobSeekerApplications'])
  const user = useSessionStore((s) => s.user)

  const { data: profile, isLoading: profileLoading } = useMyJobSeekerProfile()
  const { data: applications } = useMyApplications({ pageIndex: 0, pageSize: 1 })
  const { data: jobs, isLoading: jobsLoading } = useJobSearch({ pageIndex: 0, pageSize: 6 })

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('jobSeekerProfile:dashboardGreeting', { name: user?.fullName ?? '' })} />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Card className="flex flex-col gap-3">
          <p className="text-sm font-bold text-content-secondary">
            {t('jobSeekerProfile:profileStatusCard')}
          </p>
          {profileLoading || !profile ? (
            <LoadingState />
          ) : (
            <>
              <StatusTag
                domain="jobSeekerReview"
                value={profile.reviewStatus}
                label={t(`jobSeekerProfile:status.${profile.reviewStatus}`)}
              />
              <Link to="/profile" className="self-start">
                <Button variant="secondary" size="sm">
                  {profile.reviewStatus === 'Draft'
                    ? t('jobSeekerProfile:completeProfileAction')
                    : t('jobSeekerProfile:viewProfileAction')}
                </Button>
              </Link>
            </>
          )}
        </Card>

        <Card className="flex flex-col gap-3">
          <p className="text-sm font-bold text-content-secondary">
            {t('jobSeekerProfile:applicationsCountLabel')}
          </p>
          <p className="text-3xl font-extrabold text-brand">
            {applications?.pagination.totalCount ?? 0}
          </p>
          <Link to="/my-applications" className="self-start">
            <Button variant="secondary" size="sm">
              {t('jobSeekerProfile:viewApplicationsAction')}
            </Button>
          </Link>
        </Card>
      </div>

      <section className="flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-bold text-content-primary">
            {t('jobSeekerProfile:latestJobsSection')}
          </h2>
          <Link to="/jobs">
            <Button variant="tertiary" size="sm">
              {t('jobSeekerProfile:browseAllJobsAction')}
            </Button>
          </Link>
        </div>

        {jobsLoading && <LoadingState />}

        {!jobsLoading && jobs && jobs.items.length === 0 && (
          <EmptyState message={t('jobSeekerProfile:noJobsYet')} />
        )}

        {!jobsLoading && jobs && jobs.items.length > 0 && (
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
            {jobs.items.map((job) => (
              <JobCard key={job.id} job={job} />
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
