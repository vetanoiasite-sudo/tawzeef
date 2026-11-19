import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useCreateInterviewReport, useInterviewReport } from '../api/interviewsAndReports.queries'
import type { AttendanceResultCode } from '../types/interviewsAndReports.types'
import { Card } from '@/shared/ui/Card'
import { Select } from '@/shared/ui/Select'
import { TextInput, TextArea } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'
import { LoadingState } from '@/shared/ui/LoadingState'

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

  if (isLoading) return <LoadingState />

  if (notFound || !report) {
    if (!editable)
      return (
        <Card>
          <p className="text-sm text-text-muted">{t('noReportYet')}</p>
        </Card>
      )
    return (
      <Card className="flex flex-col gap-3">
        <p className="text-sm text-text-muted">{t('noReportYet')}</p>
        <Select value={attendance} onChange={(e) => setAttendance(e.target.value as AttendanceResultCode)}>
          <option value="Attended">{t('attendance.Attended')}</option>
          <option value="DidNotAttend">{t('attendance.DidNotAttend')}</option>
        </Select>
        <TextInput
          type="text"
          placeholder={t('interviewResultLabel')}
          value={interviewResult}
          onChange={(e) => setInterviewResult(e.target.value)}
        />
        <TextArea
          placeholder={t('strengthsLabel')}
          value={strengths}
          onChange={(e) => setStrengths(e.target.value)}
          rows={2}
        />
        <TextArea
          placeholder={t('concernsLabel')}
          value={concerns}
          onChange={(e) => setConcerns(e.target.value)}
          rows={2}
        />
        <TextArea
          placeholder={t('recommendationLabel')}
          value={recommendation}
          onChange={(e) => setRecommendation(e.target.value)}
          rows={2}
        />
        <TextArea placeholder={t('notesLabel')} value={notes} onChange={(e) => setNotes(e.target.value)} rows={2} />
        <Button
          variant="primary"
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
          className="self-start"
        >
          {t('writeReport')}
        </Button>
        {errorMessage(create.error) && <p className="text-sm text-danger">{errorMessage(create.error)}</p>}
      </Card>
    )
  }

  return (
    <Card className="flex flex-col gap-1.5 text-sm">
      <p className="text-text">
        {t('attendanceLabel')}: {t(`attendance.${report.attendance}`)}
      </p>
      {report.interviewResult && <p className="text-text">{t('interviewResultLabel')}: {report.interviewResult}</p>}
      {report.strengths && <p className="text-text">{t('strengthsLabel')}: {report.strengths}</p>}
      {report.concerns && <p className="text-text">{t('concernsLabel')}: {report.concerns}</p>}
      {report.recommendation && <p className="text-text">{t('recommendationLabel')}: {report.recommendation}</p>}
      {report.notes && <p className="text-text">{t('notesLabel')}: {report.notes}</p>}
    </Card>
  )
}
