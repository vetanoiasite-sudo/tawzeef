import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { useMyEmployerProfile } from '../api/employerProfile.queries'
import { useMyJobAds } from '@/features/employer-job-ads/api/employerJobAds.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'
import { EmptyState } from '@/shared/ui/EmptyState'

/** The page an Employer lands on right after login — a real dashboard (company verification
 * status + an active-ads count + a jobs section listing their own advertisements) so logging in
 * visibly changes what's on screen, instead of dropping back onto the public homepage. */
export default function EmployerDashboardPage() {
  const { t } = useTranslation(['employerProfile', 'employerJobAds'])
  const user = useSessionStore((s) => s.user)

  const { data: profile, isLoading: profileLoading } = useMyEmployerProfile()
  const { data: ads, isLoading: adsLoading } = useMyJobAds({ pageIndex: 0, pageSize: 5 })
  const { data: publishedAds } = useMyJobAds({ status: 'Published', pageIndex: 0, pageSize: 1 })

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('employerProfile:dashboardGreeting', { name: user?.fullName ?? '' })} />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <Card className="flex flex-col gap-3">
          <p className="text-sm font-bold text-content-secondary">
            {t('employerProfile:profileStatusCard')}
          </p>
          {profileLoading || !profile ? (
            <LoadingState />
          ) : (
            <>
              <StatusTag
                domain="employerVerification"
                value={profile.verificationStatus}
                label={t(`employerProfile:status.${profile.verificationStatus}`)}
              />
              <Link to="/company-profile" className="self-start">
                <Button variant="secondary" size="sm">
                  {profile.verificationStatus === 'Draft'
                    ? t('employerProfile:completeProfileAction')
                    : t('employerProfile:viewProfileAction')}
                </Button>
              </Link>
            </>
          )}
        </Card>

        <Card className="flex flex-col gap-3">
          <p className="text-sm font-bold text-content-secondary">
            {t('employerProfile:activeAdsCountLabel')}
          </p>
          <p className="text-3xl font-extrabold text-brand">
            {publishedAds?.pagination.totalCount ?? 0}
          </p>
          <Link to="/employer/job-advertisements/new" className="self-start">
            <Button variant="secondary" size="sm">
              {t('employerProfile:createJobAdAction')}
            </Button>
          </Link>
        </Card>
      </div>

      <section className="flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-bold text-content-primary">
            {t('employerProfile:myJobAdsSection')}
          </h2>
          <Link to="/employer/job-advertisements">
            <Button variant="tertiary" size="sm">
              {t('employerProfile:viewAllJobAdsAction')}
            </Button>
          </Link>
        </div>

        {adsLoading && <LoadingState />}

        {!adsLoading && ads && ads.items.length === 0 && (
          <EmptyState
            message={t('employerProfile:noJobAdsYet')}
            action={
              <Link to="/employer/job-advertisements/new">
                <Button variant="primary" size="sm">
                  {t('employerProfile:createJobAdAction')}
                </Button>
              </Link>
            }
          />
        )}

        {!adsLoading && ads && ads.items.length > 0 && (
          <div className="flex flex-col gap-3">
            {ads.items.map((ad) => (
              <Link key={ad.id} to={`/employer/job-advertisements/${ad.id}`}>
                <Card className="flex flex-col gap-2 transition-colors duration-150 hover:border-brand sm:flex-row sm:items-center sm:justify-between">
                  <div className="flex flex-col gap-1">
                    <h3 className="font-bold text-content-primary">{ad.title}</h3>
                    {ad.city && <p className="text-sm text-content-secondary">{ad.city}</p>}
                  </div>
                  <StatusTag
                    domain="advertisement"
                    value={ad.status}
                    label={t(`employerJobAds:status.${ad.status}`)}
                  />
                </Card>
              </Link>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
