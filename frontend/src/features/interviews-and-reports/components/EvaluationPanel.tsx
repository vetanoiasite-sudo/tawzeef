import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useCreateEvaluation, useEvaluation } from '../api/interviewsAndReports.queries'
import type { ApplicantEvaluationOutcomeCode } from '../types/interviewsAndReports.types'
import { Card } from '@/shared/ui/Card'
import { Select } from '@/shared/ui/Select'
import { TextArea } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

const OUTCOMES: ApplicantEvaluationOutcomeCode[] = ['Suitable', 'NotSuitable', 'DidNotAttend', 'RefusedJob']

export function EvaluationPanel({ applicationId, editable }: { applicationId: string; editable: boolean }) {
  const { t } = useTranslation('interviewsAndReports')
  const { data: evaluation, error, isLoading } = useEvaluation(applicationId)
  const create = useCreateEvaluation(applicationId)

  const [outcome, setOutcome] = useState<ApplicantEvaluationOutcomeCode>('Suitable')
  const [notes, setNotes] = useState('')

  const notFound = error instanceof ApiError && error.kind === 'notFound'

  if (isLoading) return <LoadingState />

  if (editable) {
    return (
      <Card className="flex flex-col gap-3">
        {evaluation ? (
          <div className="flex flex-wrap items-center gap-2 text-sm">
            <StatusTag domain="applicantEvaluation" value={evaluation.outcome} label={t(`outcome.${evaluation.outcome}`)} />
            {evaluation.notes && <span className="text-text-muted">— {evaluation.notes}</span>}
          </div>
        ) : (
          <p className="text-sm text-text-muted">{t('noEvaluationYet')}</p>
        )}
        <Select value={outcome} onChange={(e) => setOutcome(e.target.value as ApplicantEvaluationOutcomeCode)}>
          {OUTCOMES.map((o) => (
            <option key={o} value={o}>
              {t(`outcome.${o}`)}
            </option>
          ))}
        </Select>
        <TextArea placeholder={t('notesLabel')} value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
        <Button variant="primary" disabled={create.isPending} onClick={() => create.mutate({ outcome, notes })} className="self-start">
          {t('writeEvaluation')}
        </Button>
        {errorMessage(create.error) && <p className="text-sm text-danger">{errorMessage(create.error)}</p>}
      </Card>
    )
  }

  if (notFound || !evaluation)
    return (
      <Card>
        <p className="text-sm text-text-muted">{t('noEvaluationYet')}</p>
      </Card>
    )

  return (
    <Card className="flex flex-wrap items-center gap-2 text-sm">
      <StatusTag domain="applicantEvaluation" value={evaluation.outcome} label={t(`outcome.${evaluation.outcome}`)} />
      {evaluation.notes && <span className="text-text-muted">— {evaluation.notes}</span>}
    </Card>
  )
}
