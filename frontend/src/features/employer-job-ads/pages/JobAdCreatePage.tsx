import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { JobAdForm } from '../components/JobAdForm'
import { useCreateJobAd } from '../api/employerJobAds.queries'
import { useMyEmployerProfile } from '@/features/employer-profile/api/employerProfile.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { LoadingState } from '@/shared/ui/LoadingState'

export default function JobAdCreatePage() {
  const { t } = useTranslation('employerJobAds')
  const navigate = useNavigate()
  const create = useCreateJobAd()
  const { data: profile, isLoading: profileLoading } = useMyEmployerProfile()

  const profileIncomplete = profile?.verificationStatus === 'Draft'

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link
        to="/employer/job-advertisements"
        className="text-sm font-medium text-primary hover:underline"
      >
        {t('backToList')}
      </Link>
      <PageHeader title={t('createAction')} />

      {profileLoading && <LoadingState />}

      {!profileLoading && profileIncomplete && (
        <Card className="flex flex-col gap-3 border-danger/30 bg-danger/5">
          <p className="text-sm font-medium text-danger">{t('profileIncompleteWarning')}</p>
          <Link to="/company-profile" className="self-start">
            <Button variant="primary">{t('completeProfileAction')}</Button>
          </Link>
        </Card>
      )}

      {!profileLoading && !profileIncomplete && (
        <Card>
          <JobAdForm
            isSubmitting={create.isPending}
            submitError={create.error instanceof ApiError ? create.error.messages[0] : undefined}
            onSubmit={(values) =>
              create.mutate(values, {
                onSuccess: (ad) => {
                  void navigate(`/employer/job-advertisements/${ad.id}`)
                },
              })
            }
          />
        </Card>
      )}
    </div>
  )
}
