import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useCreateSuggestion, useMySuggestions } from '../api/suggestions.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { Card } from '@/shared/ui/Card'
import { TextArea } from '@/shared/ui/TextInput'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { StatusTag } from '@/shared/ui/StatusTag'

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
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8">
      <PageHeader
        title={t('pageTitle')}
        action={
          !showForm && (
            <Button variant="primary" onClick={() => setShowForm(true)}>
              {t('submitNew')}
            </Button>
          )
        }
      />

      {showForm && (
        <Card className="flex flex-col gap-3">
          <TextArea id="content" rows={4} label={t('contentLabel')} value={content} onChange={(e) => setContent(e.target.value)} />
          {errorMessage(create.error) && <p className="text-sm text-danger">{errorMessage(create.error)}</p>}
          <div className="flex gap-2">
            <Button
              variant="primary"
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
            >
              {t('submitAction')}
            </Button>
            <Button variant="secondary" onClick={() => setShowForm(false)}>
              {t('cancel')}
            </Button>
          </div>
        </Card>
      )}

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((suggestion) => (
            <Card key={suggestion.id} className="flex flex-col gap-2">
              <div className="flex items-center justify-between gap-2">
                <StatusTag domain="suggestion" value={suggestion.status} label={t(`status.${suggestion.status}`)} />
                <span className="text-xs text-text-muted">{new Date(suggestion.creationDate).toLocaleDateString('ar')}</span>
              </div>
              <p className="text-sm text-text">{suggestion.content}</p>
              {suggestion.officeNotes && (
                <p className="text-sm text-text-muted">
                  {t('officeNotesLabel')}: {suggestion.officeNotes}
                </p>
              )}
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
