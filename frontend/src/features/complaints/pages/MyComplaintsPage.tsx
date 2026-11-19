import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { ComplaintForm } from '../components/ComplaintForm'
import { useCreateComplaint, useMyComplaints } from '../api/complaint.queries'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function MyComplaintsPage() {
  const { t } = useTranslation('complaints')
  const [showForm, setShowForm] = useState(false)

  const { data, isLoading } = useMyComplaints(0, 20)
  const createComplaint = useCreateComplaint()

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
        <ComplaintForm
          isSubmitting={createComplaint.isPending}
          submitError={errorMessage(createComplaint.error)}
          onSubmit={(values) =>
            createComplaint.mutate(values, { onSuccess: () => setShowForm(false) })
          }
          onCancel={() => setShowForm(false)}
        />
      )}

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && <p className="text-sm text-text-muted">{t('emptyList')}</p>}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((complaint) => (
            <div key={complaint.id} className="flex flex-col gap-1 rounded-md border border-surface-muted p-4">
              <div className="flex items-center justify-between">
                <span className="font-medium text-text">{complaint.subjectName}</span>
                <span className="text-sm text-text-muted">{t(`status.${complaint.status}`)}</span>
              </div>
              <p className="text-sm text-text">{complaint.complaintText}</p>
              {complaint.resolutionNotes && (
                <p className="text-sm text-text-muted">
                  {t('resolutionNotesLabel')}: {complaint.resolutionNotes}
                </p>
              )}
              <p className="text-xs text-text-muted">{new Date(complaint.createdAt).toLocaleDateString('ar')}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
