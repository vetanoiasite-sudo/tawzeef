import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useAccess } from '@/shared/auth/useAccess'
import { DocumentReviewRow } from '../components/DocumentReviewRow'
import { OfficeEmployerEditForm } from '../components/OfficeEmployerEditForm'
import { ReasonForm } from '../components/ReasonForm'
import {
  useApproveDocument,
  useOfficeEmployerDetail,
  useRejectDocument,
  useRejectProfile,
  useResumeCooperation,
  useStopCooperation,
  useUpdateEmployerProfile,
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
  const [showEdit, setShowEdit] = useState(false)

  const { data: employer, isLoading } = useOfficeEmployerDetail(employerId)
  const approveDocument = useApproveDocument(employerId)
  const rejectDocument = useRejectDocument(employerId)
  const rejectProfile = useRejectProfile(employerId)
  const stopCooperation = useStopCooperation(employerId)
  const resumeCooperation = useResumeCooperation(employerId)
  const updateProfile = useUpdateEmployerProfile(employerId)

  if (isLoading || !employer) {
    return <LoadingState />
  }

  const canEdit = hasPermission('employers.review')
  const canDecideProfile = employer.officeApplication.verificationStatus === 'UnderReview'
  // API_DOCUMENTATION.md §3.5: stop/resume-cooperation is gated by complaints.decide, not
  // employers.suspend — OfficeAdmin only (RBAC_PERMISSIONS.md footnote 5).
  const canStopCooperation =
    employer.officeApplication.cooperationStatus === 'Active' && hasPermission('complaints.decide')
  const canResumeCooperation =
    employer.officeApplication.cooperationStatus === 'Stopped' && hasPermission('complaints.decide')
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
          {t('officeLabel', { defaultValue: 'المكتب' })}: {employer.officeApplication.officeName}
        </div>
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('columnStatus')}:
          <StatusTag
            domain="employerVerification"
            value={employer.officeApplication.verificationStatus}
            label={t(`status.${employer.officeApplication.verificationStatus}`)}
          />
        </div>
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('columnCooperation')}:
          <StatusTag
            domain="employerCooperation"
            value={employer.officeApplication.cooperationStatus}
            label={t(`cooperationStatus.${employer.officeApplication.cooperationStatus}`)}
          />
        </div>
        {employer.officeApplication.rejectionReason && (
          <p className="text-sm text-danger">
            {t('rejectionReasonLabel')}: {employer.officeApplication.rejectionReason}
          </p>
        )}
        {employer.officeApplication.cooperationStoppedReason && (
          <p className="text-sm text-danger">
            {t('cooperationStoppedReasonLabel')}: {employer.officeApplication.cooperationStoppedReason}
          </p>
        )}
      </Card>

      {showEdit ? (
        <OfficeEmployerEditForm
          employer={employer}
          isSubmitting={updateProfile.isPending}
          error={errorMessage(updateProfile.error)}
          onSubmit={(payload) =>
            updateProfile.mutate(payload, { onSuccess: () => setShowEdit(false) })
          }
          onCancel={() => setShowEdit(false)}
        />
      ) : (
        <Card className="grid grid-cols-1 gap-4 text-sm sm:grid-cols-2">
          {canEdit && (
            <div className="sm:col-span-2">
              <Button variant="secondary" size="sm" onClick={() => setShowEdit(true)}>
                {t('editAction')}
              </Button>
            </div>
          )}
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
      )}

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
          {!showRejectProfile ? (
            <div className="flex flex-col gap-2">
              <p className="text-sm text-content-tertiary">{t('autoApproveHint')}</p>
              <Button
                variant="danger"
                className="self-start"
                onClick={() => setShowRejectProfile(true)}
              >
                {t('rejectProfile')}
              </Button>
            </div>
          ) : (
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
