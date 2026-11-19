import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useCreateEvaluation, useEvaluation } from '../api/interviewsAndReports.queries'
import type { ApplicantEvaluationOutcomeCode } from '../types/interviewsAndReports.types'

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

  if (isLoading) return <p className="text-sm text-text-muted">...</p>

  if (editable) {
    return (
      <div className="flex flex-col gap-2">
        {evaluation ? (
          <p className="text-sm text-text">
            {t('outcomeLabel')}: {t(`outcome.${evaluation.outcome}`)} {evaluation.notes && `— ${evaluation.notes}`}
          </p>
        ) : (
          <p className="text-sm text-text-muted">{t('noEvaluationYet')}</p>
        )}
        <select
          value={outcome}
          onChange={(e) => setOutcome(e.target.value as ApplicantEvaluationOutcomeCode)}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        >
          {OUTCOMES.map((o) => (
            <option key={o} value={o}>
              {t(`outcome.${o}`)}
            </option>
          ))}
        </select>
        <textarea
          placeholder={t('notesLabel')}
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
          rows={2}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        />
        <button
          type="button"
          disabled={create.isPending}
          onClick={() => create.mutate({ outcome, notes })}
          className="self-start rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
        >
          {t('writeEvaluation')}
        </button>
        {errorMessage(create.error) && <p className="text-sm text-danger">{errorMessage(create.error)}</p>}
      </div>
    )
  }

  if (notFound || !evaluation) return <p className="text-sm text-text-muted">{t('noEvaluationYet')}</p>

  return (
    <p className="text-sm text-text">
      {t('outcomeLabel')}: {t(`outcome.${evaluation.outcome}`)} {evaluation.notes && `— ${evaluation.notes}`}
    </p>
  )
}
