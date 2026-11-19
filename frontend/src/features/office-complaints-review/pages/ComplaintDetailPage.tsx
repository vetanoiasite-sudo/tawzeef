import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useAccess } from '@/shared/auth/useAccess'
import { DecideControl } from '../components/DecideControl'
import { ResolveControl } from '../components/ResolveControl'
import { TriageControl } from '../components/TriageControl'
import {
  useDecideComplaint,
  useOfficeComplaintDetail,
  useResolveComplaint,
  useTriageComplaint,
} from '../api/officeComplaintReview.queries'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function ComplaintDetailPage() {
  const { t } = useTranslation('officeComplaintReview')
  const { hasPermission } = useAccess()
  const { id } = useParams<{ id: string }>()
  const complaintId = id ?? ''

  const { data: complaint, isLoading } = useOfficeComplaintDetail(complaintId)
  const triage = useTriageComplaint(complaintId)
  const resolve = useResolveComplaint(complaintId)
  const decide = useDecideComplaint(complaintId)

  if (isLoading || !complaint) {
    return <div className="mx-auto max-w-2xl px-4 py-10 text-text-muted">...</div>
  }

  const canTriageOrResolve = complaint.status === 'UnderReview' || complaint.status === 'WaitingForInformation'
  const canTriage = canTriageOrResolve && hasPermission('complaints.triage')
  const canResolve = canTriageOrResolve && hasPermission('complaints.resolve')
  const canDecide = hasPermission('complaints.decide')

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-8 px-4 py-10">
      <Link to="/office/complaints" className="text-sm text-primary">
        {t('backToQueue')}
      </Link>

      <div className="flex flex-col gap-1">
        <h1 className="text-lg font-semibold text-text">{complaint.subjectName}</h1>
        <p className="text-sm text-text-muted">
          {t('senderLabel')}: {complaint.senderType}
        </p>
      </div>

      <div className="rounded-md bg-surface-muted p-4">
        <p className="text-sm text-text-muted">
          {t('columnStatus')}: <span className="font-medium text-text">{t(`status.${complaint.status}`)}</span>
        </p>
        <p className="mt-1 text-sm text-text-muted">
          {t('columnValidity')}: <span className="font-medium text-text">{t(`validity.${complaint.validityDecision}`)}</span>
        </p>
      </div>

      <section className="flex flex-col gap-2">
        <h2 className="font-semibold text-text">{t('complaintTextLabel')}</h2>
        <p className="text-sm text-text">{complaint.complaintText}</p>
      </section>

      {complaint.resolutionNotes && (
        <section className="flex flex-col gap-2">
          <h2 className="font-semibold text-text">{t('resolutionNotesLabel')}</h2>
          <p className="text-sm text-text">{complaint.resolutionNotes}</p>
        </section>
      )}

      {canTriage && (
        <TriageControl
          isSubmitting={triage.isPending}
          error={errorMessage(triage.error)}
          onDecide={(v) => triage.mutate(v)}
        />
      )}

      {canResolve && (
        <ResolveControl
          isSubmitting={resolve.isPending}
          error={errorMessage(resolve.error)}
          onResolve={(outcome, notes) => resolve.mutate({ outcome, resolutionNotes: notes })}
        />
      )}

      {canDecide && (
        <>
          {complaint.validityDecision !== 'Valid' ? (
            <p className="text-sm text-text-muted">{t('decideNotValidYet')}</p>
          ) : (
            <DecideControl
              subjectType={complaint.subjectType}
              isSubmitting={decide.isPending}
              error={errorMessage(decide.error)}
              onDecide={(decisionType, reason) => decide.mutate({ decisionType, reason })}
            />
          )}
          {decide.isSuccess && decide.data && (
            <p className="text-sm text-success">
              {t('decideSuccess', {
                status: t(`decideResultStatus.${decide.data.reviewStatus}`, { defaultValue: decide.data.reviewStatus }),
              })}
            </p>
          )}
        </>
      )}
    </div>
  )
}
