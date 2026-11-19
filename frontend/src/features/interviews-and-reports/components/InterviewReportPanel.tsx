import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useCreateInterviewReport, useInterviewReport } from '../api/interviewsAndReports.queries'
import type { AttendanceResultCode } from '../types/interviewsAndReports.types'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export function InterviewReportPanel({ applicationId, editable }: { applicationId: string; editable: boolean }) {
  const { t } = useTranslation('interviewsAndReports')
  const { data: report, error, isLoading } = useInterviewReport(applicationId)
  const create = useCreateInterviewReport(applicationId)

  const [attendance, setAttendance] = useState<AttendanceResultCode>('Attended')
  const [interviewResult, setInterviewResult] = useState('')
  const [strengths, setStrengths] = useState('')
  const [concerns, setConcerns] = useState('')
  const [recommendation, setRecommendation] = useState('')
  const [notes, setNotes] = useState('')

  const notFound = error instanceof ApiError && error.kind === 'notFound'

  if (isLoading) return <p className="text-sm text-text-muted">...</p>

  if (notFound || !report) {
    if (!editable) return <p className="text-sm text-text-muted">{t('noReportYet')}</p>
    return (
      <div className="flex flex-col gap-2">
        <p className="text-sm text-text-muted">{t('noReportYet')}</p>
        <select
          value={attendance}
          onChange={(e) => setAttendance(e.target.value as AttendanceResultCode)}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        >
          <option value="Attended">{t('attendance.Attended')}</option>
          <option value="DidNotAttend">{t('attendance.DidNotAttend')}</option>
        </select>
        <input
          type="text"
          placeholder={t('interviewResultLabel')}
          value={interviewResult}
          onChange={(e) => setInterviewResult(e.target.value)}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        />
        <textarea
          placeholder={t('strengthsLabel')}
          value={strengths}
          onChange={(e) => setStrengths(e.target.value)}
          rows={2}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        />
        <textarea
          placeholder={t('concernsLabel')}
          value={concerns}
          onChange={(e) => setConcerns(e.target.value)}
          rows={2}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        />
        <textarea
          placeholder={t('recommendationLabel')}
          value={recommendation}
          onChange={(e) => setRecommendation(e.target.value)}
          rows={2}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        />
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
          onClick={() =>
            create.mutate({
              attendance,
              interviewResult,
              employerEvaluation: '',
              strengths,
              concerns,
              recommendation,
              notes,
            })
          }
          className="self-start rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
        >
          {t('writeReport')}
        </button>
        {errorMessage(create.error) && <p className="text-sm text-danger">{errorMessage(create.error)}</p>}
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-1 text-sm">
      <p className="text-text">
        {t('attendanceLabel')}: {t(`attendance.${report.attendance}`)}
      </p>
      {report.interviewResult && <p className="text-text">{t('interviewResultLabel')}: {report.interviewResult}</p>}
      {report.strengths && <p className="text-text">{t('strengthsLabel')}: {report.strengths}</p>}
      {report.concerns && <p className="text-text">{t('concernsLabel')}: {report.concerns}</p>}
      {report.recommendation && <p className="text-text">{t('recommendationLabel')}: {report.recommendation}</p>}
      {report.notes && <p className="text-text">{t('notesLabel')}: {report.notes}</p>}
    </div>
  )
}
