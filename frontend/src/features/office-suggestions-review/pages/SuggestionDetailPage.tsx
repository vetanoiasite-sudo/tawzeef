import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useOfficeSuggestionDetail, useRespondToSuggestion } from '../api/officeSuggestionsReview.queries'

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
    return <div className="mx-auto max-w-2xl px-4 py-10 text-text-muted">...</div>
  }

  const canRespond = suggestion.status === 'New' || suggestion.status === 'UnderReview'

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-10">
      <Link to="/office/suggestions" className="text-sm text-primary">
        {t('backToQueue')}
      </Link>

      <div className="rounded-md bg-surface-muted p-4">
        <p className="text-sm text-text-muted">
          {t('columnStatus')}: <span className="font-medium text-text">{t(`status.${suggestion.status}`)}</span>
        </p>
      </div>

      <p className="text-text">{suggestion.content}</p>

      {suggestion.officeNotes && (
        <div className="rounded-md border border-surface-muted p-4">
          <p className="text-sm text-text-muted">{t('officeNotesLabel')}</p>
          <p className="text-text">{suggestion.officeNotes}</p>
        </div>
      )}

      {canRespond && (
        <div className="flex flex-col gap-2">
          <label htmlFor="officeNotes" className="text-sm font-medium text-text">
            {t('respondLabel')}
          </label>
          <textarea
            id="officeNotes"
            rows={4}
            value={officeNotes}
            onChange={(e) => setOfficeNotes(e.target.value)}
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          />
          {errorMessage(respond.error) && <p className="text-sm text-danger">{errorMessage(respond.error)}</p>}
          <button
            type="button"
            disabled={respond.isPending || !officeNotes.trim()}
            onClick={() => respond.mutate(officeNotes.trim())}
            className="self-start rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
          >
            {t('respondAction')}
          </button>
        </div>
      )}

      {!canRespond && suggestion.status === 'Responded' && (
        <p className="text-sm text-text-muted">{t('alreadyResponded')}</p>
      )}
    </div>
  )
}
