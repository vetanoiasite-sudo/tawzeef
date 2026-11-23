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
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

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
    return <LoadingState />
  }

  const canDecideProfile = employer.verificationStatus === 'UnderReview'
  // API_DOCUMENTATION.md §3.5: stop/resume-cooperation is gated by complaints.decide, not
  // employers.suspend — OfficeAdmin only (RBAC_PERMISSIONS.md footnote 5).
  const canStopCooperation =
    employer.cooperationStatus === 'Active' && hasPermission('complaints.decide')
  const canResumeCooperation =
    employer.cooperationStatus === 'Stopped' && hasPermission('complaints.decide')
  const overComplaintThreshold = employer.validComplaintCount >= 3

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link to="/office/employers" className="text-sm font-medium text-primary hover:underline">
        {t('backToQueue')}
      </Link>

      <div className="flex flex-col gap-1">
        <h1 className="text-xl font-bold text-text">{employer.displayName}</h1>
        <p className="text-sm text-text-muted">{employer.legalName}</p>
      </div>

      {overComplaintThreshold && (
        <Card className="border-danger bg-danger/10">
          <p className="text-sm font-medium text-danger">
            {t('complaintThresholdWarning', { count: employer.validComplaintCount })}
          </p>
        </Card>
      )}

      <Card className="flex flex-col gap-2">
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('columnStatus')}:
          <StatusTag
            domain="employerVerification"
            value={employer.verificationStatus}
            label={t(`status.${employer.verificationStatus}`)}
          />
        </div>
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('columnCooperation')}:
          <StatusTag
            domain="employerCooperation"
            value={employer.cooperationStatus}
            label={t(`cooperationStatus.${employer.cooperationStatus}`)}
          />
        </div>
        {employer.rejectionReason && (
          <p className="text-sm text-danger">
            {t('rejectionReasonLabel')}: {employer.rejectionReason}
          </p>
        )}
        {employer.cooperationStoppedReason && (
          <p className="text-sm text-danger">
            {t('cooperationStoppedReasonLabel')}: {employer.cooperationStoppedReason}
          </p>
        )}
      </Card>

      <Card className="grid grid-cols-1 gap-4 text-sm sm:grid-cols-2">
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
        <div className="sm:col-span-2">
          <p className="text-text-muted">{t('addressLabel')}</p>
          <p className="text-text">{employer.address ?? '—'}</p>
        </div>
        <div className="sm:col-span-2">
          <p className="text-text-muted">{t('businessActivityLabel')}</p>
          <p className="text-text">{employer.businessActivity ?? '—'}</p>
        </div>
      </Card>

      <section className="flex flex-col gap-4">
        <h2 className="text-lg font-semibold text-text">{t('documentsSection')}</h2>
        {employer.documents.map((document) => (
          <DocumentReviewRow
            key={document.id}
            document={document}
            employerId={employerId}
            isApproving={approveDocument.isPending}
            isRejecting={rejectDocument.isPending}
            rejectError={
              rejectDocument.variables?.documentId === document.id
                ? errorMessage(rejectDocument.error)
                : undefined
            }
            onApprove={() => approveDocument.mutate(document.id)}
            onReject={(reason) => rejectDocument.mutate({ documentId: document.id, reason })}
          />
        ))}
      </section>

      {canDecideProfile && (
        <section className="flex flex-col gap-2">
          {!showRejectProfile && (
            <div className="flex gap-2">
              <Button
                disabled={approveProfile.isPending}
                onClick={() => approveProfile.mutate()}
                className="bg-success hover:bg-success/90"
              >
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
          {errorMessage(approveProfile.error) && (
            <p className="text-sm text-danger">{errorMessage(approveProfile.error)}</p>
          )}
        </section>
      )}

      {canStopCooperation && (
        <section className="flex flex-col gap-2">
          {!showStopCooperation ? (
            <Button
              variant="danger"
              onClick={() => setShowStopCooperation(true)}
              className="self-start"
            >
              {t('stopCooperation')}
            </Button>
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
            <Button onClick={() => setShowResumeCooperation(true)} className="self-start">
              {t('resumeCooperation')}
            </Button>
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
