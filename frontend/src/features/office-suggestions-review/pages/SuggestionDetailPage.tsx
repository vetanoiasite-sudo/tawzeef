import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useOfficeSuggestionDetail, useRespondToSuggestion } from '../api/officeSuggestionsReview.queries'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextArea } from '@/shared/ui/TextInput'
import { LoadingState } from '@/shared/ui/LoadingState'
import { StatusTag } from '@/shared/ui/StatusTag'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function SuggestionDetailPage() {
  const { t } = useTranslation('officeSuggestionsReview')
  const { id } = useParams<{ id: string }>()
  const suggestionId = id ?? ''
  const [officeNotes, setOfficeNotes] = useState('')

  const { data: suggestion, isLoading } = useOfficeSuggestionDetail(suggestionId)
  const respond = useRespondToSuggestion(suggestionId)

  if (isLoading || !suggestion) {
    return <LoadingState />
  }

  const canRespond = suggestion.status === 'New' || suggestion.status === 'UnderReview'

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8">
      <Link to="/office/suggestions" className="text-sm font-medium text-primary hover:underline">
        {t('backToQueue')}
      </Link>

      <Card className="flex flex-col gap-5">
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('columnStatus')}: <StatusTag domain="suggestion" value={suggestion.status} label={t(`status.${suggestion.status}`)} />
        </div>

        <p className="text-text">{suggestion.content}</p>

        {suggestion.officeNotes && (
          <div className="border-t border-surface-muted pt-4">
            <p className="text-sm text-text-muted">{t('officeNotesLabel')}</p>
            <p className="text-text">{suggestion.officeNotes}</p>
          </div>
        )}

        {canRespond && (
          <div className="flex flex-col gap-2 border-t border-surface-muted pt-4">
            <TextArea
              id="officeNotes"
              rows={4}
              label={t('respondLabel')}
              value={officeNotes}
              onChange={(e) => setOfficeNotes(e.target.value)}
            />
            {errorMessage(respond.error) && <p className="text-sm text-danger">{errorMessage(respond.error)}</p>}
            <Button
              variant="primary"
              disabled={respond.isPending || !officeNotes.trim()}
              onClick={() => respond.mutate(officeNotes.trim())}
              className="self-start"
            >
              {t('respondAction')}
            </Button>
          </div>
        )}

        {!canRespond && suggestion.status === 'Responded' && (
          <p className="text-sm text-text-muted">{t('alreadyResponded')}</p>
        )}
      </Card>
    </div>
  )
}
