import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { usePublicOffices } from '@/features/auth'
import { EmployerDocumentUploadCard } from '../components/EmployerDocumentUploadCard'
import { EmployerProfileCompletionForm } from '../components/EmployerProfileCompletionForm'
import { LogoUploadCard } from '../components/LogoUploadCard'
import {
  useMyEmployerProfile,
  useSelectEmployerOffice,
  useSubmitEmployerProfileForReview,
  useUpdateMyEmployerProfile,
  useUploadEmployerDocument,
  useUploadEmployerLogo,
} from '../api/employerProfile.queries'
import type { EmployerProfileFormValues } from '../schemas/employerProfile.schema'
import {
  REQUIRED_DOCUMENT_TYPES,
  type EmployerDocumentTypeCode,
} from '../types/employerProfile.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { Select } from '@/shared/ui/Select'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

function SelectOfficeCard() {
  const { t } = useTranslation('employerProfile')
  const { data: offices, isLoading: officesLoading } = usePublicOffices()
  const [officeId, setOfficeId] = useState('')
  const selectOffice = useSelectEmployerOffice()

  return (
    <Card className="flex flex-col gap-3">
      <div>
        <p className="font-bold text-content-primary">{t('selectOfficeTitle')}</p>
        <p className="text-sm text-content-tertiary">{t('selectOfficeHint')}</p>
      </div>
      <Select
        id="applyOfficeId"
        disabled={officesLoading}
        value={officeId}
        label={t('officeLabel')}
        onChange={(e) => setOfficeId(e.target.value)}
      >
        <option value="" disabled>
          {t('officePlaceholder')}
        </option>
        {offices?.map((office) => (
          <option key={office.id} value={office.id}>
            {office.city ? `${office.name} — ${office.city}` : office.name}
          </option>
        ))}
      </Select>
      {errorMessage(selectOffice.error) && (
        <p className="text-sm text-danger">{errorMessage(selectOffice.error)}</p>
      )}
      <Button
        className="self-start"
        disabled={!officeId || selectOffice.isPending}
        onClick={() => selectOffice.mutate(officeId)}
      >
        {t('applyToOfficeAction')}
      </Button>
    </Card>
  )
}

export default function EmployerProfilePage() {
  const { t } = useTranslation('employerProfile')
  const { data: profile, isLoading } = useMyEmployerProfile()
  const updateProfile = useUpdateMyEmployerProfile()
  const uploadDocument = useUploadEmployerDocument()
  const uploadLogo = useUploadEmployerLogo()
  const submitForReview = useSubmitEmployerProfileForReview()

  if (isLoading || !profile) {
    return <LoadingState />
  }

  // Step 2 of registration: apply to an office before anything else becomes available — matches
  // the backend gate in EmployerProfileService.UploadDocumentAsync/SubmitForReviewAsync.
  if (!profile.officeId) {
    return (
      <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
        <PageHeader title={t('pageTitle')} />
        <SelectOfficeCard />
      </div>
    )
  }

  function handleSaveProfile(values: EmployerProfileFormValues) {
    updateProfile.mutate({
      businessActivity: values.businessActivity || undefined,
      address: values.address || undefined,
      city: values.city || undefined,
      companyEmail: values.companyEmail || undefined,
      companyPhone: values.companyPhone || undefined,
      representativeName: values.representativeName || undefined,
      representativeRole: values.representativeRole || undefined,
    })
  }

  function handleUpload(documentType: EmployerDocumentTypeCode, file: File) {
    uploadDocument.mutate({ documentType, file })
  }

  const presentTypes = new Set(profile.documents.map((d) => d.documentType))
  const allDocumentsPresent = REQUIRED_DOCUMENT_TYPES.every((type) => presentTypes.has(type))
  const canSubmit =
    (profile.verificationStatus === 'Draft' || profile.verificationStatus === 'Rejected') &&
    allDocumentsPresent

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      <Card className="flex flex-col gap-2">
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('statusLabel')}:
          <StatusTag
            domain="employerVerification"
            value={profile.verificationStatus}
            label={t(`status.${profile.verificationStatus}`)}
          />
        </div>
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('cooperationStatusLabel')}:
          <StatusTag
            domain="employerCooperation"
            value={profile.cooperationStatus}
            label={t(`cooperationStatus.${profile.cooperationStatus}`)}
          />
        </div>
        {profile.rejectionReason && (
          <p className="text-sm text-danger">
            {t('rejectionReasonLabel')}: {profile.rejectionReason}
          </p>
        )}
        {profile.cooperationStoppedReason && (
          <p className="text-sm text-danger">
            {t('cooperationStoppedReasonLabel')}: {profile.cooperationStoppedReason}
          </p>
        )}
      </Card>

      <LogoUploadCard
        logoUrl={profile.logoUrl}
        isUploading={uploadLogo.isPending}
        error={errorMessage(uploadLogo.error)}
        onUpload={(file) => uploadLogo.mutate(file)}
      />

      <section className="flex flex-col gap-4">
        <h2 className="text-lg font-semibold text-text">{t('profileSection')}</h2>
        <Card>
          <EmployerProfileCompletionForm
            profile={profile}
            isSubmitting={updateProfile.isPending}
            submitError={errorMessage(updateProfile.error)}
            onSubmit={handleSaveProfile}
          />
          {updateProfile.isSuccess && (
            <p className="mt-3 text-sm text-success">{t('saveSuccess')}</p>
          )}
        </Card>
      </section>

      <section className="flex flex-col gap-4">
        <h2 className="text-lg font-semibold text-text">{t('documentsSection')}</h2>
        {REQUIRED_DOCUMENT_TYPES.map((type) => (
          <EmployerDocumentUploadCard
            key={type}
            documentType={type}
            document={profile.documents.find((d) => d.documentType === type)}
            isUploading={uploadDocument.isPending}
            error={
              uploadDocument.variables?.documentType === type
                ? errorMessage(uploadDocument.error)
                : undefined
            }
            onUpload={(file) => handleUpload(type, file)}
          />
        ))}
      </section>

      {(profile.verificationStatus === 'Draft' || profile.verificationStatus === 'Rejected') && (
        <div className="flex flex-col items-start gap-2">
          <Button
            disabled={!canSubmit || submitForReview.isPending}
            onClick={() => submitForReview.mutate()}
          >
            {t('submitForReview')}
          </Button>
          {!allDocumentsPresent && (
            <p className="text-sm text-text-muted">{t('submitMissingDocs')}</p>
          )}
          {errorMessage(submitForReview.error) && (
            <p className="text-sm text-danger">{errorMessage(submitForReview.error)}</p>
          )}
          {submitForReview.isSuccess && (
            <p className="text-sm text-success">{t('submitSuccess')}</p>
          )}
        </div>
      )}
    </div>
  )
}
