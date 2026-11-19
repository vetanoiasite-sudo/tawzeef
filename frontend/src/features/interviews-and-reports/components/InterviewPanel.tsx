import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useInterview, useScheduleInterview, useUpdateInterview } from '../api/interviewsAndReports.queries'
import type { InterviewStatusCode } from '../types/interviewsAndReports.types'
import { Card } from '@/shared/ui/Card'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export function InterviewPanel({ applicationId, editable }: { applicationId: string; editable: boolean }) {
  const { t } = useTranslation('interviewsAndReports')
  const { data: interview, error, isLoading } = useInterview(applicationId)
  const schedule = useScheduleInterview(applicationId)
  const update = useUpdateInterview(applicationId)

  const [scheduledAt, setScheduledAt] = useState('')
  const [location, setLocation] = useState('')

  const notFound = error instanceof ApiError && error.kind === 'notFound'

  if (isLoading) return <LoadingState />

  if (notFound || !interview) {
    if (!editable)
      return (
        <Card>
          <p className="text-sm text-text-muted">{t('noInterviewYet')}</p>
        </Card>
      )
    return (
      <Card className="flex flex-col gap-3">
        <p className="text-sm text-text-muted">{t('noInterviewYet')}</p>
        <TextInput
          type="datetime-local"
          value={scheduledAt}
          onChange={(e) => setScheduledAt(e.target.value)}
        />
        <TextInput
          type="text"
          placeholder={t('locationLabel')}
          value={location}
          onChange={(e) => setLocation(e.target.value)}
        />
        <Button
          variant="primary"
          disabled={schedule.isPending}
          onClick={() =>
            schedule.mutate({
              scheduledAt: scheduledAt ? new Date(scheduledAt).toISOString() : '',
              locationOrMeetingDetails: location,
            })
          }
          className="self-start"
        >
          {t('scheduleInterview')}
        </Button>
        {errorMessage(schedule.error) && <p className="text-sm text-danger">{errorMessage(schedule.error)}</p>}
      </Card>
    )
  }

  return (
    <Card className="flex flex-col gap-3 text-sm">
      <p className="text-text">
        {t('scheduledAtLabel')}: {interview.scheduledAt ? new Date(interview.scheduledAt).toLocaleString('ar') : '—'}
      </p>
      <p className="text-text">
        {t('locationLabel')}: {interview.locationOrMeetingDetails ?? '—'}
      </p>
      <div className="flex items-center gap-2 text-text">
        <span>{t('interviewStatusLabel')}:</span>
        <StatusTag domain="interview" value={interview.status} label={t(`interviewStatus.${interview.status}`)} />
      </div>

      {editable && interview.status === 'Scheduled' && (
        <div className="flex flex-wrap gap-2">
          {(['Completed', 'NoShow', 'Cancelled'] as InterviewStatusCode[]).map((status) => (
            <Button
              key={status}
              variant="secondary"
              disabled={update.isPending}
              onClick={() => update.mutate({ scheduledAt: '', locationOrMeetingDetails: '', status })}
            >
              {t(`interviewStatus.${status}`)}
            </Button>
          ))}
        </div>
      )}
      {errorMessage(update.error) && <p className="text-sm text-danger">{errorMessage(update.error)}</p>}
    </Card>
  )
}
