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
    return <div className="mx-auto max-w-2xl px-4 py-10 text-text-muted">...</div>
  }

  const canDecideProfile = profile.reviewStatus === 'UnderReview'
  const canSuspend = profile.reviewStatus === 'Approved' && hasPermission('jobseekers.suspend')
  const canReinstate = profile.reviewStatus === 'Suspended' && hasPermission('jobseekers.reinstate')
  const overComplaintThreshold = profile.validComplaintCount >= 3

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-8 px-4 py-10">
      <Link to="/office/job-seekers" className="text-sm text-primary">
        {t('backToQueue')}
      </Link>

      <div className="flex flex-col gap-1">
        <h1 className="text-lg font-semibold text-text">{profile.fullName}</h1>
        <p className="text-sm text-text-muted">{profile.mobileNumber}</p>
      </div>

      {overComplaintThreshold && (
        <div className="rounded-md border border-danger bg-danger/10 p-4">
          <p className="text-sm font-medium text-danger">
            {t('complaintThresholdWarning', { count: profile.validComplaintCount })}
          </p>
        </div>
      )}

      <div className="rounded-md bg-surface-muted p-4">
        <p className="text-sm text-text-muted">
          {t('columnStatus')}: <span className="font-medium text-text">{t(`status.${profile.reviewStatus}`)}</span>
        </p>
        {profile.rejectionReason && (
          <p className="mt-1 text-sm text-danger">
            {t('rejectionReasonLabel')}: {profile.rejectionReason}
          </p>
        )}
        {profile.suspensionReason && (
          <p className="mt-1 text-sm text-danger">
            {t('suspensionReasonLabel')}: {profile.suspensionReason}
          </p>
        )}
      </div>

      <section className="grid grid-cols-2 gap-4 text-sm">
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
        <div className="col-span-2">
          <p className="text-text-muted">{t('skillsLabel')}</p>
          <p className="text-text">{profile.skills ?? '—'}</p>
        </div>
        <div className="col-span-2">
          <p className="text-text-muted">{t('careerSummaryLabel')}</p>
          <p className="text-text">{profile.careerSummary ?? '—'}</p>
        </div>
      </section>

      <section className="flex flex-col gap-4">
        <h2 className="font-semibold text-text">{t('documentsSection')}</h2>
        {profile.documents.map((document) => (
          <DocumentReviewRow
            key={document.id}
            document={document}
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
              <button
                type="button"
                disabled={approveProfile.isPending}
                onClick={() => approveProfile.mutate()}
                className="rounded-md bg-success px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
              >
                {t('approveProfile')}
              </button>
              <button
                type="button"
                onClick={() => setShowRejectProfile(true)}
                className="rounded-md border border-danger px-4 py-2 text-sm font-medium text-danger"
              >
                {t('rejectProfile')}
              </button>
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
            <button
              type="button"
              onClick={() => setShowSuspend(true)}
              className="self-start rounded-md border border-danger px-4 py-2 text-sm font-medium text-danger"
            >
              {t('suspendProfile')}
            </button>
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
            <button
              type="button"
              onClick={() => setShowReinstate(true)}
              className="self-start rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface"
            >
              {t('reinstateProfile')}
            </button>
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
