import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { JobAdPackageRow } from '../components/JobAdPackageRow'
import { useOfficeJobAdPackages, useToggleJobAdPackageActive, useUpdateJobAdPackage } from '../api/officeJobAdPackages.queries'
import { ApiError } from '@/shared/api/types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function JobAdPackagesManagementPage() {
  const { t } = useTranslation('officeJobAdPackages')
  const { data: packages, isLoading } = useOfficeJobAdPackages()
  const updatePackage = useUpdateJobAdPackage()
  const toggleActive = useToggleJobAdPackageActive()
  const [editingId, setEditingId] = useState<string | null>(null)

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />
      <p className="text-sm text-content-secondary">{t('pageSubtitle')}</p>

      {isLoading && <LoadingState />}

      {!isLoading && packages && packages.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && packages && packages.length > 0 && (
        <div className="flex flex-col gap-3">
          {packages.map((pkg) => (
            <JobAdPackageRow
              key={pkg.id}
              pkg={pkg}
              editing={editingId === pkg.id}
              isSaving={updatePackage.isPending && updatePackage.variables?.id === pkg.id}
              saveError={
                updatePackage.variables?.id === pkg.id ? errorMessage(updatePackage.error) : undefined
              }
              isTogglingActive={toggleActive.isPending && toggleActive.variables?.id === pkg.id}
              onStartEdit={() => setEditingId(pkg.id)}
              onCancelEdit={() => setEditingId(null)}
              onSave={(payload) =>
                updatePackage.mutate({ id: pkg.id, payload }, { onSuccess: () => setEditingId(null) })
              }
              onToggleActive={(activate) => toggleActive.mutate({ id: pkg.id, activate })}
            />
          ))}
        </div>
      )}
    </div>
  )
}
