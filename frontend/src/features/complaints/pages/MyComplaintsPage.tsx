import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { ComplaintForm } from '../components/ComplaintForm'
import { useCreateComplaint, useMyComplaints } from '../api/complaint.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { Card } from '@/shared/ui/Card'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { StatusTag } from '@/shared/ui/StatusTag'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function MyComplaintsPage() {
  const { t } = useTranslation('complaints')
  const [showForm, setShowForm] = useState(false)

  const { data, isLoading } = useMyComplaints(0, 20)
  const createComplaint = useCreateComplaint()

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
        <ComplaintForm
          isSubmitting={createComplaint.isPending}
          submitError={errorMessage(createComplaint.error)}
          onSubmit={(values) =>
            createComplaint.mutate(values, { onSuccess: () => setShowForm(false) })
          }
          onCancel={() => setShowForm(false)}
        />
      )}

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((complaint) => (
            <Card key={complaint.id} className="flex flex-col gap-2">
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-text">{complaint.subjectName}</span>
                <StatusTag domain="complaint" value={complaint.status} label={t(`status.${complaint.status}`)} />
              </div>
              <p className="text-sm text-text">{complaint.complaintText}</p>
              {complaint.resolutionNotes && (
                <p className="text-sm text-text-muted">
                  {t('resolutionNotesLabel')}: {complaint.resolutionNotes}
                </p>
              )}
              <p className="text-xs text-text-muted">{new Date(complaint.createdAt).toLocaleDateString('ar')}</p>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
