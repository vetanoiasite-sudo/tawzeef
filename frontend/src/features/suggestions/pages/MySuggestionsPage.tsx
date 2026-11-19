import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useCreateSuggestion, useMySuggestions } from '../api/suggestions.queries'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function MySuggestionsPage() {
  const { t } = useTranslation('suggestions')
  const [showForm, setShowForm] = useState(false)
  const [content, setContent] = useState('')

  const { data, isLoading } = useMySuggestions(0, 20)
  const create = useCreateSuggestion()

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-10">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-text">{t('pageTitle')}</h1>
        {!showForm && (
          <button
            type="button"
            onClick={() => setShowForm(true)}
            className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface"
          >
            {t('submitNew')}
          </button>
        )}
      </div>

      {showForm && (
        <div className="flex flex-col gap-2 rounded-md border border-surface-muted p-4">
          <label htmlFor="content" className="text-sm font-medium text-text">
            {t('contentLabel')}
          </label>
          <textarea
            id="content"
            rows={4}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          />
          {errorMessage(create.error) && <p className="text-sm text-danger">{errorMessage(create.error)}</p>}
          <div className="flex gap-2">
            <button
              type="button"
              disabled={create.isPending || !content.trim()}
              onClick={() =>
                create.mutate(
                  { content: content.trim() },
                  {
                    onSuccess: () => {
                      setShowForm(false)
                      setContent('')
                    },
                  },
                )
              }
              className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
            >
              {t('submitAction')}
            </button>
            <button
              type="button"
              onClick={() => setShowForm(false)}
              className="rounded-md border border-surface-muted px-4 py-2 text-sm text-text"
            >
              {t('cancel')}
            </button>
          </div>
        </div>
      )}

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && <p className="text-sm text-text-muted">{t('emptyList')}</p>}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((suggestion) => (
            <div key={suggestion.id} className="flex flex-col gap-1 rounded-md border border-surface-muted p-4">
              <div className="flex items-center justify-between">
                <span className="text-sm text-text-muted">{t(`status.${suggestion.status}`)}</span>
                <span className="text-xs text-text-muted">{new Date(suggestion.creationDate).toLocaleDateString('ar')}</span>
              </div>
              <p className="text-sm text-text">{suggestion.content}</p>
              {suggestion.officeNotes && (
                <p className="text-sm text-text-muted">
                  {t('officeNotesLabel')}: {suggestion.officeNotes}
                </p>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
