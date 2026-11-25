import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { OfficeLogoUploadCard } from '../components/OfficeLogoUploadCard'
import { OfficeProfileCompletionForm } from '../components/OfficeProfileCompletionForm'
import { OfficeProfileSummaryCard } from '../components/OfficeProfileSummaryCard'
import { useMyOffice, useUpdateMyOffice, useUploadMyOfficeLogo } from '../api/officeProfile.queries'
import type { OfficeProfileFormValues } from '../schemas/officeProfile.schema'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function OfficeProfilePage() {
  const { t } = useTranslation('officeProfile')
  const { data: office, isLoading } = useMyOffice()
  const updateOffice = useUpdateMyOffice()
  const uploadLogo = useUploadMyOfficeLogo()
  // View-first, edit-on-demand — same pattern as every other profile in this app
  // (2026-11-25 product decision).
  const [isEditing, setIsEditing] = useState(false)

  // Flash the "saved" confirmation, then clear it, by resetting the mutation itself rather than
  // mirroring isSuccess into local state (same reasoning as every other profile page here).
  useEffect(() => {
    if (!updateOffice.isSuccess) return
    const timer = setTimeout(() => updateOffice.reset(), 3000)
    return () => clearTimeout(timer)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [updateOffice.isSuccess, updateOffice.reset])

  if (isLoading || !office) {
    return <LoadingState />
  }

  function handleSave(values: OfficeProfileFormValues) {
    updateOffice.mutate(
      {
        name: values.name,
        arName: values.arName || undefined,
        address: values.address || undefined,
        city: values.city || undefined,
        phone: values.phone || undefined,
        email: values.email || undefined,
      },
      { onSuccess: () => setIsEditing(false) },
    )
  }

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      {!isEditing && <OfficeProfileSummaryCard office={office} onEdit={() => setIsEditing(true)} />}

      {isEditing && (
        <section className="flex flex-col gap-4">
          <div className="flex items-center justify-between gap-2">
            <h2 className="text-lg font-semibold text-text">{t('profileSection')}</h2>
            <Button variant="tertiary" size="sm" onClick={() => setIsEditing(false)}>
              {t('backToProfile')}
            </Button>
          </div>

          <OfficeLogoUploadCard
            logoUrl={office.logoUrl}
            isUploading={uploadLogo.isPending}
            error={errorMessage(uploadLogo.error)}
            onUpload={(file) => uploadLogo.mutate(file)}
          />

          <Card>
            <OfficeProfileCompletionForm
              office={office}
              isSubmitting={updateOffice.isPending}
              submitError={errorMessage(updateOffice.error)}
              onSubmit={handleSave}
            />
          </Card>
        </section>
      )}
    </div>
  )
}
