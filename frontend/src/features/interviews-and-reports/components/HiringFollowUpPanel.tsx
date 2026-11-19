import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useCompleteHiringFollowUp, useCreateHiringFollowUp, useHiringFollowUp } from '../api/interviewsAndReports.queries'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

function BoolLabel({ value, t }: { value: boolean | null; t: (key: string) => string }) {
  if (value === null) return <>{t('unspecified')}</>
  return <>{value ? t('yes') : t('no')}</>
}

/// office-managed section; Employer sees a read-only view (canManage=false).
export function HiringFollowUpPanel({ applicationId, canManage }: { applicationId: string; canManage: boolean }) {
  const { t } = useTranslation('interviewsAndReports')
  const { data: followUp, error, isLoading } = useHiringFollowUp(applicationId)
  const markHired = useCreateHiringFollowUp(applicationId)
  const complete = useCompleteHiringFollowUp(applicationId)

  const [stillEmployed, setStillEmployed] = useState<boolean | null>(null)
  const [employerSatisfied, setEmployerSatisfied] = useState<boolean | null>(null)
  const [jobSeekerSatisfied, setJobSeekerSatisfied] = useState<boolean | null>(null)
  const [notes, setNotes] = useState('')

  const notFound = error instanceof ApiError && error.kind === 'notFound'

  if (isLoading) return <p className="text-sm text-text-muted">...</p>

  if (notFound || !followUp) {
    if (!canManage) return <p className="text-sm text-text-muted">{t('noFollowUpYet')}</p>
    return (
      <div className="flex flex-col gap-2">
        <p className="text-sm text-text-muted">{t('noFollowUpYet')}</p>
        <button
          type="button"
          disabled={markHired.isPending}
          onClick={() => markHired.mutate(undefined)}
          className="self-start rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
        >
          {t('markHired')}
        </button>
        {errorMessage(markHired.error) && <p className="text-sm text-danger">{errorMessage(markHired.error)}</p>}
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-2 text-sm">
      <p className="text-text">
        {t('hiredAtLabel')}: {new Date(followUp.hiredAt).toLocaleDateString('ar')}
      </p>
      <p className="text-text">
        {t('dueAtLabel')}: {new Date(followUp.dueAt).toLocaleDateString('ar')}
      </p>
      <p className="text-text">{t(`followUpStatus.${followUp.status}`)}</p>

      {followUp.status === 'Completed' ? (
        <>
          <p className="text-text">
            {t('stillEmployedLabel')}: <BoolLabel value={followUp.stillEmployed} t={t} />
          </p>
          <p className="text-text">
            {t('employerSatisfiedLabel')}: <BoolLabel value={followUp.employerSatisfied} t={t} />
          </p>
          <p className="text-text">
            {t('jobSeekerSatisfiedLabel')}: <BoolLabel value={followUp.jobSeekerSatisfied} t={t} />
          </p>
          {followUp.notes && <p className="text-text">{t('notesLabel')}: {followUp.notes}</p>}
        </>
      ) : (
        canManage && (
          <div className="flex flex-col gap-2 rounded-md border border-surface-muted p-3">
            {[
              { label: t('stillEmployedLabel'), value: stillEmployed, set: setStillEmployed },
              { label: t('employerSatisfiedLabel'), value: employerSatisfied, set: setEmployerSatisfied },
              { label: t('jobSeekerSatisfiedLabel'), value: jobSeekerSatisfied, set: setJobSeekerSatisfied },
            ].map((field) => (
              <div key={field.label} className="flex items-center gap-2">
                <span className="text-text-muted">{field.label}</span>
                <select
                  value={field.value === null ? '' : field.value ? 'yes' : 'no'}
                  onChange={(e) => field.set(e.target.value === '' ? null : e.target.value === 'yes')}
                  className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
                >
                  <option value="">{t('unspecified')}</option>
                  <option value="yes">{t('yes')}</option>
                  <option value="no">{t('no')}</option>
                </select>
              </div>
            ))}
            <textarea
              placeholder={t('notesLabel')}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={2}
              className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
            />
            <button
              type="button"
              disabled={complete.isPending}
              onClick={() => complete.mutate({ stillEmployed, employerSatisfied, jobSeekerSatisfied, notes })}
              className="self-start rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
            >
              {t('completeFollowUp')}
            </button>
            {errorMessage(complete.error) && <p className="text-sm text-danger">{errorMessage(complete.error)}</p>}
          </div>
        )
      )}
    </div>
  )
}
