import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  useRespondToEmploymentCheck,
  useRespondToHiringFollowUp,
  useRespondToInterviewFeedback,
} from '../api/notifications.queries'
import { Button } from '@/shared/ui/Button'
import { TextInput } from '@/shared/ui/TextInput'

/** Renders only for the three self-report event types a job seeker can act on — every other
 * notification is read-only text. `onResponded` hides these buttons locally once a reply lands;
 * there's no persisted "answered" flag on the Notification row itself, so a page refresh can show
 * them again for an already-answered one (harmless — every respond endpoint either rejects an
 * already-completed report or just re-records the same answer). */
export function NotificationResponseActions({
  eventType,
  relatedId,
  onResponded,
}: {
  eventType: string
  relatedId: string | null
  onResponded: () => void
}) {
  const { t } = useTranslation('notifications')
  const [notes, setNotes] = useState('')
  const respondToEmploymentCheck = useRespondToEmploymentCheck()
  const respondToHiringFollowUp = useRespondToHiringFollowUp()
  const respondToInterviewFeedback = useRespondToInterviewFeedback()

  if (!relatedId) return null

  const isPending =
    respondToEmploymentCheck.isPending ||
    respondToHiringFollowUp.isPending ||
    respondToInterviewFeedback.isPending

  if (eventType === 'JobSeekerEmployment.CheckIn') {
    return (
      <div className="flex gap-2">
        <Button
          size="sm"
          disabled={isPending}
          onClick={() =>
            respondToEmploymentCheck.mutate({ id: relatedId, foundJob: true }, { onSuccess: onResponded })
          }
        >
          {t('actionFoundJob')}
        </Button>
        <Button
          size="sm"
          variant="secondary"
          disabled={isPending}
          onClick={() =>
            respondToEmploymentCheck.mutate({ id: relatedId, foundJob: false }, { onSuccess: onResponded })
          }
        >
          {t('actionStillSearching')}
        </Button>
      </div>
    )
  }

  if (eventType === 'HiringFollowUp.CheckIn') {
    return (
      <div className="flex flex-col gap-2">
        <TextInput
          id={`hiring-follow-up-notes-${relatedId}`}
          placeholder={t('actionNotesPlaceholder')}
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
        />
        <div className="flex gap-2">
          <Button
            size="sm"
            disabled={isPending}
            onClick={() =>
              respondToHiringFollowUp.mutate(
                { id: relatedId, stillEmployed: true, notes: notes || undefined },
                { onSuccess: onResponded },
              )
            }
          >
            {t('actionStillEmployed')}
          </Button>
          <Button
            size="sm"
            variant="secondary"
            disabled={isPending}
            onClick={() =>
              respondToHiringFollowUp.mutate(
                { id: relatedId, stillEmployed: false, notes: notes || undefined },
                { onSuccess: onResponded },
              )
            }
          >
            {t('actionLeftJob')}
          </Button>
        </div>
      </div>
    )
  }

  if (eventType === 'Interview.SeekerFeedbackRequest') {
    return (
      <div className="flex flex-col gap-2">
        <TextInput
          id={`interview-feedback-notes-${relatedId}`}
          placeholder={t('actionNotesPlaceholder')}
          value={notes}
          onChange={(e) => setNotes(e.target.value)}
        />
        <div className="flex flex-wrap gap-2">
          <Button
            size="sm"
            disabled={isPending}
            onClick={() =>
              respondToInterviewFeedback.mutate(
                { id: relatedId, outcome: 'Accepted', notes: notes || undefined },
                { onSuccess: onResponded },
              )
            }
          >
            {t('actionAccepted')}
          </Button>
          <Button
            size="sm"
            variant="secondary"
            disabled={isPending}
            onClick={() =>
              respondToInterviewFeedback.mutate(
                { id: relatedId, outcome: 'Rejected', notes: notes || undefined },
                { onSuccess: onResponded },
              )
            }
          >
            {t('actionRejected')}
          </Button>
          <Button
            size="sm"
            variant="tertiary"
            disabled={isPending}
            onClick={() =>
              respondToInterviewFeedback.mutate(
                { id: relatedId, outcome: 'StillWaiting', notes: notes || undefined },
                { onSuccess: onResponded },
              )
            }
          >
            {t('actionStillWaiting')}
          </Button>
        </div>
      </div>
    )
  }

  return null
}
