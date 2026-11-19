import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useCompleteHiringFollowUp, useCreateHiringFollowUp, useHiringFollowUp } from '../api/interviewsAndReports.queries'
import { Card } from '@/shared/ui/Card'
import { Select } from '@/shared/ui/Select'
import { TextArea } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

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

  if (isLoading) return <LoadingState />

  if (notFound || !followUp) {
    if (!canManage)
      return (
        <Card>
          <p className="text-sm text-text-muted">{t('noFollowUpYet')}</p>
        </Card>
      )
    return (
      <Card className="flex flex-col gap-3">
        <p className="text-sm text-text-muted">{t('noFollowUpYet')}</p>
        <Button variant="primary" disabled={markHired.isPending} onClick={() => markHired.mutate(undefined)} className="self-start">
          {t('markHired')}
        </Button>
        {errorMessage(markHired.error) && <p className="text-sm text-danger">{errorMessage(markHired.error)}</p>}
      </Card>
    )
  }

  return (
    <Card className="flex flex-col gap-3 text-sm">
      <p className="text-text">
        {t('hiredAtLabel')}: {new Date(followUp.hiredAt).toLocaleDateString('ar')}
      </p>
      <p className="text-text">
        {t('dueAtLabel')}: {new Date(followUp.dueAt).toLocaleDateString('ar')}
      </p>
      <StatusTag domain="hiringFollowUp" value={followUp.status} label={t(`followUpStatus.${followUp.status}`)} />

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
          <div className="flex flex-col gap-3 border-t border-surface-muted pt-3">
            {[
              { label: t('stillEmployedLabel'), value: stillEmployed, set: setStillEmployed },
              { label: t('employerSatisfiedLabel'), value: employerSatisfied, set: setEmployerSatisfied },
              { label: t('jobSeekerSatisfiedLabel'), value: jobSeekerSatisfied, set: setJobSeekerSatisfied },
            ].map((field) => (
              <div key={field.label} className="flex items-center gap-2">
                <span className="w-40 shrink-0 text-text-muted">{field.label}</span>
                <Select
                  value={field.value === null ? '' : field.value ? 'yes' : 'no'}
                  onChange={(e) => field.set(e.target.value === '' ? null : e.target.value === 'yes')}
                  className="flex-1"
                >
                  <option value="">{t('unspecified')}</option>
                  <option value="yes">{t('yes')}</option>
                  <option value="no">{t('no')}</option>
                </Select>
              </div>
            ))}
            <TextArea placeholder={t('notesLabel')} value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
            <Button
              variant="primary"
              disabled={complete.isPending}
              onClick={() => complete.mutate({ stillEmployed, employerSatisfied, jobSeekerSatisfied, notes })}
              className="self-start"
            >
              {t('completeFollowUp')}
            </Button>
            {errorMessage(complete.error) && <p className="text-sm text-danger">{errorMessage(complete.error)}</p>}
          </div>
        )
      )}
    </Card>
  )
}
