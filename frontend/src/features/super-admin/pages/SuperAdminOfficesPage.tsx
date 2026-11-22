import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { httpClient } from '@/shared/api/httpClient'
import { OfficeForm } from '../components/OfficeForm'
import { OfficeRow } from '../components/OfficeRow'
import { useActivateOffice, useCreateOffice, useDeactivateOffice, useOffices, useUpdateOffice } from '../api/superAdmin.queries'
import type { CreateOfficePayload } from '../types/superAdmin.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function SuperAdminOfficesPage() {
  const { t } = useTranslation('superAdmin')
  const [showCreate, setShowCreate] = useState(false)
  const [exporting, setExporting] = useState(false)

  const { data: offices, isLoading } = useOffices()
  const createOffice = useCreateOffice()
  const updateOffice = useUpdateOffice()
  const activateOffice = useActivateOffice()
  const deactivateOffice = useDeactivateOffice()

  async function handleExport() {
    setExporting(true)
    try {
      const { blob, fileName } = await httpClient.getBlob('/api/offices/export')
      const url = URL.createObjectURL(blob)
      const link = window.document.createElement('a')
      link.href = url
      link.download = fileName
      link.click()
      URL.revokeObjectURL(url)
    } finally {
      setExporting(false)
    }
  }

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader
        title={t('officesPageTitle')}
        action={
          <div className="flex gap-2">
            <Button variant="secondary" disabled={exporting} onClick={() => void handleExport()}>
              {t('exportAction')}
            </Button>
            {!showCreate && (
              <Button variant="primary" onClick={() => setShowCreate(true)}>
                {t('createOfficeAction')}
              </Button>
            )}
          </div>
        }
      />

      {showCreate && (
        <OfficeForm
          isSubmitting={createOffice.isPending}
          error={errorMessage(createOffice.error)}
          onSubmit={(payload) =>
            createOffice.mutate(payload as CreateOfficePayload, { onSuccess: () => setShowCreate(false) })
          }
          onCancel={() => setShowCreate(false)}
        />
      )}
      {createOffice.isSuccess && !showCreate && <p className="text-sm text-success">{t('createSuccess')}</p>}

      {isLoading && <LoadingState />}

      {!isLoading && offices && offices.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && offices && offices.length > 0 && (
        <div className="flex flex-col gap-3">
          {offices.map((office) => (
            <OfficeRow
              key={office.id}
              office={office}
              isSaving={updateOffice.isPending}
              saveError={updateOffice.variables?.id === office.id ? errorMessage(updateOffice.error) : undefined}
              isTogglingStatus={activateOffice.isPending || deactivateOffice.isPending}
              onSave={(payload) => updateOffice.mutate({ id: office.id, payload })}
              onToggleStatus={() =>
                office.status === 'Active' ? deactivateOffice.mutate(office.id) : activateOffice.mutate(office.id)
              }
            />
          ))}
        </div>
      )}
    </div>
  )
}
