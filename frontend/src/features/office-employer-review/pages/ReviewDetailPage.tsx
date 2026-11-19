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
  useOfficeEmployerDetail,
  useRejectDocument,
  useRejectProfile,
  useResumeCooperation,
  useStopCooperation,
} from '../api/officeEmployerReview.queries'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function ReviewDetailPage() {
  const { t } = useTranslation('officeEmployerReview')
  const { hasPermission } = useAccess()
  const { id } = useParams<{ id: string }>()
  const employerId = id ?? ''
  const [showRejectProfile, setShowRejectProfile] = useState(false)
  const [showStopCooperation, setShowStopCooperation] = useState(false)
  const [showResumeCooperation, setShowResumeCooperation] = useState(false)

  const { data: employer, isLoading } = useOfficeEmployerDetail(employerId)
  const approveDocument = useApproveDocument(employerId)
  const rejectDocument = useRejectDocument(employerId)
  const approveProfile = useApproveProfile(employerId)
  const rejectProfile = useRejectProfile(employerId)
  const stopCooperation = useStopCooperation(employerId)
  const resumeCooperation = useResumeCooperation(employerId)

  if (isLoading || !employer) {
    return <div className="mx-auto max-w-2xl px-4 py-10 text-text-muted">...</div>
  }

  const canDecideProfile = employer.verificationStatus === 'UnderReview'
  // API_DOCUMENTATION.md §3.5: stop/resume-cooperation is gated by complaints.decide, not
  // employers.suspend — OfficeAdmin only (RBAC_PERMISSIONS.md footnote 5).
  const canStopCooperation = employer.cooperationStatus === 'Active' && hasPermission('complaints.decide')
  const canResumeCooperation = employer.cooperationStatus === 'Stopped' && hasPermission('complaints.decide')
  const overComplaintThreshold = employer.validComplaintCount >= 3

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-8 px-4 py-10">
      <Link to="/office/employers" className="text-sm text-primary">
        {t('backToQueue')}
      </Link>

      <div className="flex flex-col gap-1">
        <h1 className="text-lg font-semibold text-text">{employer.displayName}</h1>
        <p className="text-sm text-text-muted">{employer.legalName}</p>
      </div>

      {overComplaintThreshold && (
        <div className="rounded-md border border-danger bg-danger/10 p-4">
          <p className="text-sm font-medium text-danger">
            {t('complaintThresholdWarning', { count: employer.validComplaintCount })}
          </p>
        </div>
      )}

      <div className="rounded-md bg-surface-muted p-4">
        <p className="text-sm text-text-muted">
          {t('columnStatus')}: <span className="font-medium text-text">{t(`status.${employer.verificationStatus}`)}</span>
        </p>
        <p className="mt-1 text-sm text-text-muted">
          {t('columnCooperation')}:{' '}
          <span className="font-medium text-text">{t(`cooperationStatus.${employer.cooperationStatus}`)}</span>
        </p>
        {employer.rejectionReason && (
          <p className="mt-1 text-sm text-danger">
            {t('rejectionReasonLabel')}: {employer.rejectionReason}
          </p>
        )}
        {employer.cooperationStoppedReason && (
          <p className="mt-1 text-sm text-danger">
            {t('cooperationStoppedReasonLabel')}: {employer.cooperationStoppedReason}
          </p>
        )}
      </div>

      <section className="grid grid-cols-2 gap-4 text-sm">
        <div>
          <p className="text-text-muted">{t('representativeLabel')}</p>
          <p className="text-text">{employer.representativeName ?? '—'}</p>
        </div>
        <div>
          <p className="text-text-muted">{t('cityLabel')}</p>
          <p className="text-text">{employer.city ?? '—'}</p>
        </div>
        <div>
          <p className="text-text-muted">{t('emailLabel')}</p>
          <p className="text-text">{employer.companyEmail ?? '—'}</p>
        </div>
        <div>
          <p className="text-text-muted">{t('phoneLabel')}</p>
          <p className="text-text">{employer.companyPhone ?? '—'}</p>
        </div>
        <div className="col-span-2">
          <p className="text-text-muted">{t('addressLabel')}</p>
          <p className="text-text">{employer.address ?? '—'}</p>
        </div>
        <div className="col-span-2">
          <p className="text-text-muted">{t('businessActivityLabel')}</p>
          <p className="text-text">{employer.businessActivity ?? '—'}</p>
        </div>
      </section>

      <section className="flex flex-col gap-4">
        <h2 className="font-semibold text-text">{t('documentsSection')}</h2>
        {employer.documents.map((document) => (
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

      {canStopCooperation && (
        <section className="flex flex-col gap-2">
          {!showStopCooperation ? (
            <button
              type="button"
              onClick={() => setShowStopCooperation(true)}
              className="self-start rounded-md border border-danger px-4 py-2 text-sm font-medium text-danger"
            >
              {t('stopCooperation')}
            </button>
          ) : (
            <ReasonForm
              confirmLabel={t('stopCooperation')}
              isSubmitting={stopCooperation.isPending}
              error={errorMessage(stopCooperation.error)}
              onConfirm={(reason) => {
                stopCooperation.mutate(reason)
                setShowStopCooperation(false)
              }}
              onCancel={() => setShowStopCooperation(false)}
            />
          )}
        </section>
      )}

      {canResumeCooperation && (
        <section className="flex flex-col gap-2">
          {!showResumeCooperation ? (
            <button
              type="button"
              onClick={() => setShowResumeCooperation(true)}
              className="self-start rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface"
            >
              {t('resumeCooperation')}
            </button>
          ) : (
            <ReasonForm
              confirmLabel={t('resumeCooperation')}
              confirmTone="primary"
              isSubmitting={resumeCooperation.isPending}
              error={errorMessage(resumeCooperation.error)}
              onConfirm={(reason) => {
                resumeCooperation.mutate(reason)
                setShowResumeCooperation(false)
              }}
              onCancel={() => setShowResumeCooperation(false)}
            />
          )}
        </section>
      )}
    </div>
  )
}
