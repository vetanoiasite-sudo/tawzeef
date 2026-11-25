import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { employerJobAdsApi } from '../api/employerJobAds.api'
import { useApplicantProfile } from '../api/employerJobAds.queries'
import { useFilePreview } from '@/shared/hooks/useFilePreview'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { GatedField } from '@/shared/ui/GatedField'
import { LoadingState } from '@/shared/ui/LoadingState'
import { StatusTag } from '@/shared/ui/StatusTag'
import { FilePreviewModal } from '@/shared/ui/FilePreviewModal'

/** The employer's "open this applicant's profile" page (2026-11-25 product decision) —
 * job title/skills/career summary/education/photo always show; contact info and the CV stay
 * locked behind GatedField until the advertisement's payment gate passes (same rule already
 * enforced server-side — this page never receives those fields at all before then). No
 * verification documents here, ever — those are office-only. */
export default function ApplicantProfilePage() {
  const { t } = useTranslation('employerJobAds')
  const { adId, applicationId } = useParams<{ adId: string; applicationId: string }>()
  const id = applicationId ?? ''

  const { data: applicant, isLoading } = useApplicantProfile(id)
  const [photoUrl, setPhotoUrl] = useState<string | null>(null)
  const { preview, isLoading: isLoadingCv, open: openCv, close: closeCv } = useFilePreview()

  useEffect(() => {
    let cancelled = false
    let objectUrl: string | null = null
    if (applicant?.hasPhoto) {
      void employerJobAdsApi.getApplicantPhoto(id).then(({ blob }) => {
        if (cancelled) return
        objectUrl = URL.createObjectURL(blob)
        setPhotoUrl(objectUrl)
      })
    } else {
      void Promise.resolve().then(() => {
        if (!cancelled) setPhotoUrl(null)
      })
    }
    return () => {
      cancelled = true
      if (objectUrl) URL.revokeObjectURL(objectUrl)
    }
  }, [applicant?.hasPhoto, id])

  if (isLoading || !applicant) {
    return <LoadingState />
  }

  const isGated = applicant.phone === undefined && applicant.email === undefined

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link
        to={`/employer/job-advertisements/${adId}/applicants`}
        className="text-sm font-medium text-brand hover:underline"
      >
        {t('backToApplicants')}
      </Link>
      <PageHeader title={t('applicantProfileTitle')} />

      <Card className="flex flex-col gap-6">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div className="flex items-center gap-4">
            <div className="flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-full border border-border-primary bg-surface-muted">
              {photoUrl ? (
                <img src={photoUrl} alt="" className="size-full object-cover" />
              ) : (
                <PersonIcon />
              )}
            </div>
            <div>
              <h1 className="text-xl font-bold text-content-primary">{applicant.jobSeekerName}</h1>
              {applicant.jobTitle && (
                <p className="mt-0.5 text-sm font-semibold text-brand">{applicant.jobTitle}</p>
              )}
            </div>
          </div>
          <StatusTag
            domain="applicationEmployerFacing"
            value={applicant.employerFacingStatus}
            label={t(`outcome.${applicant.employerFacingStatus}`)}
          />
        </div>

        {applicant.careerSummary && (
          <section className="flex flex-col gap-1.5">
            <h2 className="text-xs font-bold tracking-wide text-content-tertiary uppercase">
              {t('careerSummaryLabel')}
            </h2>
            <p className="text-sm whitespace-pre-line text-content-primary">{applicant.careerSummary}</p>
          </section>
        )}

        {applicant.skills && (
          <section className="flex flex-col gap-1.5">
            <h2 className="text-xs font-bold tracking-wide text-content-tertiary uppercase">
              {t('skillsLabel')}
            </h2>
            <p className="text-sm whitespace-pre-line text-content-primary">{applicant.skills}</p>
          </section>
        )}

        <div className="grid grid-cols-1 gap-x-6 gap-y-3 border-t border-border-primary pt-4 sm:grid-cols-2">
          {applicant.education && (
            <div className="flex flex-col gap-0.5">
              <span className="text-xs font-medium text-content-tertiary">{t('educationLabel')}</span>
              <span className="text-sm text-content-primary">{applicant.education}</span>
            </div>
          )}
          {applicant.yearsOfExperience != null && (
            <div className="flex flex-col gap-0.5">
              <span className="text-xs font-medium text-content-tertiary">
                {t('yearsOfExperienceLabel')}
              </span>
              <span className="text-sm text-content-primary">{applicant.yearsOfExperience}</span>
            </div>
          )}
          <div className="flex flex-col gap-0.5">
            <span className="text-xs font-medium text-content-tertiary">{t('appliedAtLabel')}</span>
            <span className="text-sm text-content-primary">
              {new Date(applicant.appliedAt).toLocaleDateString('ar')}
            </span>
          </div>
        </div>

        <div className="flex flex-col gap-2 border-t border-border-primary pt-4">
          <h2 className="text-xs font-bold tracking-wide text-content-tertiary uppercase">
            {t('columnContact')}
          </h2>
          {isGated ? (
            <GatedField reason={t('dataLockedUntilPayment')} />
          ) : (
            <div className="flex flex-col gap-1 text-sm text-content-primary">
              {applicant.phone && <span>{applicant.phone}</span>}
              {applicant.email && <span>{applicant.email}</span>}
            </div>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {applicant.cvDownloadUrl ? (
            <Button
              variant="secondary"
              disabled={isLoadingCv}
              onClick={() => applicant.cvDownloadUrl && void openCv(applicant.cvDownloadUrl)}
            >
              {t('viewCv')}
            </Button>
          ) : (
            <GatedField reason={t('dataLockedUntilPayment')} />
          )}
        </div>
      </Card>

      {preview && <FilePreviewModal preview={preview} onClose={closeCv} />}
    </div>
  )
}

function PersonIcon() {
  return (
    <svg width="32" height="32" viewBox="0 0 24 24" fill="none" aria-hidden className="text-content-tertiary">
      <circle cx="12" cy="8" r="4" stroke="currentColor" strokeWidth="1.8" />
      <path d="M4 20c0-3.3 3.6-6 8-6s8 2.7 8 6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}
