import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useAccess } from '@/shared/auth/useAccess'
import { DocumentReviewRow } from '../components/DocumentReviewRow'
import { ReasonForm } from '../components/ReasonForm'
import {
  useApproveDocument,
  useApproveProfile,
  useOfficeJobSeekerDetail,
  useReinstateProfile,
  useRejectDocument,
  useRejectProfile,
  useSuspendProfile,
} from '../api/officeJobSeekerReview.queries'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function ReviewDetailPage() {
  const { t } = useTranslation('officeJobSeekerReview')
  const { hasPermission } = useAccess()
  const { id } = useParams<{ id: string }>()
  const profileId = id ?? ''
  const [showRejectProfile, setShowRejectProfile] = useState(false)
  const [showSuspend, setShowSuspend] = useState(false)
  const [showReinstate, setShowReinstate] = useState(false)

  const { data: profile, isLoading } = useOfficeJobSeekerDetail(profileId)
  const approveDocument = useApproveDocument(profileId)
  const rejectDocument = useRejectDocument(profileId)
  const approveProfile = useApproveProfile(profileId)
  const rejectProfile = useRejectProfile(profileId)
  const suspendProfile = useSuspendProfile(profileId)
  const reinstateProfile = useReinstateProfile(profileId)

  if (isLoading || !profile) {
    return <LoadingState />
  }

  const canDecideProfile = profile.reviewStatus === 'UnderReview'
  const canSuspend = profile.reviewStatus === 'Approved' && hasPermission('jobseekers.suspend')
  const canReinstate = profile.reviewStatus === 'Suspended' && hasPermission('jobseekers.reinstate')
  const overComplaintThreshold = profile.validComplaintCount >= 3

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link to="/office/job-seekers" className="text-sm font-medium text-primary hover:underline">
        {t('backToQueue')}
      </Link>

      <div className="flex flex-col gap-1">
        <h1 className="text-xl font-bold text-text">{profile.fullName}</h1>
        <p className="text-sm text-text-muted">{profile.mobileNumber}</p>
      </div>

      {overComplaintThreshold && (
        <Card className="border-danger bg-danger/10">
          <p className="text-sm font-medium text-danger">
            {t('complaintThresholdWarning', { count: profile.validComplaintCount })}
          </p>
        </Card>
      )}

      <Card className="flex flex-col gap-2">
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('columnStatus')}:
          <StatusTag domain="jobSeekerReview" value={profile.reviewStatus} label={t(`status.${profile.reviewStatus}`)} />
        </div>
        {profile.rejectionReason && (
          <p className="text-sm text-danger">
            {t('rejectionReasonLabel')}: {profile.rejectionReason}
          </p>
        )}
        {profile.suspensionReason && (
          <p className="text-sm text-danger">
            {t('suspensionReasonLabel')}: {profile.suspensionReason}
          </p>
        )}
      </Card>

      <Card className="grid grid-cols-1 gap-4 text-sm sm:grid-cols-2">
        <div>
          <p className="text-text-muted">{t('emailLabel')}</p>
          <p className="text-text">{profile.email ?? '—'}</p>
        </div>
        <div>
          <p className="text-text-muted">{t('cityLabel')}</p>
          <p className="text-text">{profile.city ?? '—'}</p>
        </div>
        <div>
          <p className="text-text-muted">{t('addressLabel')}</p>
          <p className="text-text">{profile.address ?? '—'}</p>
        </div>
        <div>
          <p className="text-text-muted">{t('educationLabel')}</p>
          <p className="text-text">{profile.education ?? '—'}</p>
        </div>
        <div>
          <p className="text-text-muted">{t('yearsOfExperienceLabel')}</p>
          <p className="text-text">{profile.yearsOfExperience ?? '—'}</p>
        </div>
        <div className="sm:col-span-2">
          <p className="text-text-muted">{t('skillsLabel')}</p>
          <p className="text-text">{profile.skills ?? '—'}</p>
        </div>
        <div className="sm:col-span-2">
          <p className="text-text-muted">{t('careerSummaryLabel')}</p>
          <p className="text-text">{profile.careerSummary ?? '—'}</p>
        </div>
      </Card>

      <section className="flex flex-col gap-4">
        <h2 className="text-lg font-semibold text-text">{t('documentsSection')}</h2>
        {profile.documents.map((document) => (
          <DocumentReviewRow
            key={document.id}
            document={document}
            profileId={profileId}
            isApproving={approveDocument.isPending}
            isRejecting={rejectDocument.isPending}
            rejectError={rejectDocument.variables?.documentId === document.id ? errorMessage(rejectDocument.error) : undefined}
            onApprove={() => approveDocument.mutate(document.id)}
            onReject={(reason) => rejectDocument.mutate({ documentId: document.id, reason })}
          />
        ))}
      </section>

      {canDecideProfile && (
        <section className="flex flex-col gap-2">
          {!showRejectProfile && (
            <div className="flex gap-2">
              <Button disabled={approveProfile.isPending} onClick={() => approveProfile.mutate()} className="bg-success hover:bg-success/90">
                {t('approveProfile')}
              </Button>
              <Button variant="danger" onClick={() => setShowRejectProfile(true)}>
                {t('rejectProfile')}
              </Button>
            </div>
          )}
          {showRejectProfile && (
            <ReasonForm
              confirmLabel={t('rejectProfile')}
              isSubmitting={rejectProfile.isPending}
              error={errorMessage(rejectProfile.error)}
              onConfirm={(reason) => {
                rejectProfile.mutate(reason)
                setShowRejectProfile(false)
              }}
              onCancel={() => setShowRejectProfile(false)}
            />
          )}
          {errorMessage(approveProfile.error) && <p className="text-sm text-danger">{errorMessage(approveProfile.error)}</p>}
        </section>
      )}

      {canSuspend && (
        <section className="flex flex-col gap-2">
          {!showSuspend ? (
            <Button variant="danger" onClick={() => setShowSuspend(true)} className="self-start">
              {t('suspendProfile')}
            </Button>
          ) : (
            <ReasonForm
              confirmLabel={t('suspendProfile')}
              isSubmitting={suspendProfile.isPending}
              error={errorMessage(suspendProfile.error)}
              onConfirm={(reason) => {
                suspendProfile.mutate(reason)
                setShowSuspend(false)
              }}
              onCancel={() => setShowSuspend(false)}
            />
          )}
        </section>
      )}

      {canReinstate && (
        <section className="flex flex-col gap-2">
          {!showReinstate ? (
            <Button onClick={() => setShowReinstate(true)} className="self-start">
              {t('reinstateProfile')}
            </Button>
          ) : (
            <ReasonForm
              confirmLabel={t('reinstateProfile')}
              confirmTone="primary"
              isSubmitting={reinstateProfile.isPending}
              error={errorMessage(reinstateProfile.error)}
              onConfirm={(reason) => {
                reinstateProfile.mutate(reason)
                setShowReinstate(false)
              }}
              onCancel={() => setShowReinstate(false)}
            />
          )}
        </section>
      )}
    </div>
  )
}
