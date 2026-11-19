import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useInterview, useScheduleInterview, useUpdateInterview } from '../api/interviewsAndReports.queries'
import type { InterviewStatusCode } from '../types/interviewsAndReports.types'

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

  if (isLoading) return <p className="text-sm text-text-muted">...</p>

  if (notFound || !interview) {
    if (!editable) return <p className="text-sm text-text-muted">{t('noInterviewYet')}</p>
    return (
      <div className="flex flex-col gap-2">
        <p className="text-sm text-text-muted">{t('noInterviewYet')}</p>
        <input
          type="datetime-local"
          value={scheduledAt}
          onChange={(e) => setScheduledAt(e.target.value)}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        />
        <input
          type="text"
          placeholder={t('locationLabel')}
          value={location}
          onChange={(e) => setLocation(e.target.value)}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        />
        <button
          type="button"
          disabled={schedule.isPending}
          onClick={() =>
            schedule.mutate({
              scheduledAt: scheduledAt ? new Date(scheduledAt).toISOString() : '',
              locationOrMeetingDetails: location,
            })
          }
          className="self-start rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-surface disabled:opacity-60"
        >
          {t('scheduleInterview')}
        </button>
        {errorMessage(schedule.error) && <p className="text-sm text-danger">{errorMessage(schedule.error)}</p>}
      </div>
    )
  }

  return (
    <div className="flex flex-col gap-2 text-sm">
      <p className="text-text">
        {t('scheduledAtLabel')}: {interview.scheduledAt ? new Date(interview.scheduledAt).toLocaleString('ar') : '—'}
      </p>
      <p className="text-text">
        {t('locationLabel')}: {interview.locationOrMeetingDetails ?? '—'}
      </p>
      <p className="text-text">
        {t('interviewStatusLabel')}: {t(`interviewStatus.${interview.status}`)}
      </p>

      {editable && interview.status === 'Scheduled' && (
        <div className="flex flex-wrap gap-2">
          {(['Completed', 'NoShow', 'Cancelled'] as InterviewStatusCode[]).map((status) => (
            <button
              key={status}
              type="button"
              disabled={update.isPending}
              onClick={() => update.mutate({ scheduledAt: '', locationOrMeetingDetails: '', status })}
              className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text disabled:opacity-60"
            >
              {t(`interviewStatus.${status}`)}
            </button>
          ))}
        </div>
      )}
      {errorMessage(update.error) && <p className="text-sm text-danger">{errorMessage(update.error)}</p>}
    </div>
  )
}
