import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'
import { EmployerDocumentUploadCard } from '../components/EmployerDocumentUploadCard'
import { EmployerProfileCompletionForm } from '../components/EmployerProfileCompletionForm'
import {
  useMyEmployerProfile,
  useSubmitEmployerProfileForReview,
  useUpdateMyEmployerProfile,
  useUploadEmployerDocument,
} from '../api/employerProfile.queries'
import type { EmployerProfileFormValues } from '../schemas/employerProfile.schema'
import { REQUIRED_DOCUMENT_TYPES, type EmployerDocumentTypeCode } from '../types/employerProfile.types'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function EmployerProfilePage() {
  const { t } = useTranslation('employerProfile')
  const { data: profile, isLoading } = useMyEmployerProfile()
  const updateProfile = useUpdateMyEmployerProfile()
  const uploadDocument = useUploadEmployerDocument()
  const submitForReview = useSubmitEmployerProfileForReview()

  if (isLoading || !profile) {
    return (
      <div className="mx-auto flex max-w-2xl flex-col items-center gap-4 px-4 py-10">
        <TawzeefLogo height={48} />
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
  const canSubmit = (profile.verificationStatus === 'Draft' || profile.verificationStatus === 'Rejected') && allDocumentsPresent

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-8 px-4 py-10">
      <div className="flex flex-col items-center gap-2">
        <TawzeefLogo height={48} />
        <h1 className="text-lg font-semibold text-text">{t('pageTitle')}</h1>
      </div>

      <div className="rounded-md bg-surface-muted p-4">
        <p className="text-sm text-text-muted">
          {t('statusLabel')}: <span className="font-medium text-text">{t(`status.${profile.verificationStatus}`)}</span>
        </p>
        <p className="mt-1 text-sm text-text-muted">
          {t('cooperationStatusLabel')}:{' '}
          <span className="font-medium text-text">{t(`cooperationStatus.${profile.cooperationStatus}`)}</span>
        </p>
        {profile.rejectionReason && (
          <p className="mt-1 text-sm text-danger">
            {t('rejectionReasonLabel')}: {profile.rejectionReason}
          </p>
        )}
        {profile.cooperationStoppedReason && (
          <p className="mt-1 text-sm text-danger">
            {t('cooperationStoppedReasonLabel')}: {profile.cooperationStoppedReason}
          </p>
        )}
      </div>

      <section className="flex flex-col gap-4">
        <h2 className="font-semibold text-text">{t('profileSection')}</h2>
        <EmployerProfileCompletionForm
          profile={profile}
          isSubmitting={updateProfile.isPending}
          submitError={errorMessage(updateProfile.error)}
          onSubmit={handleSaveProfile}
        />
        {updateProfile.isSuccess && <p className="text-sm text-success">{t('saveSuccess')}</p>}
      </section>

      <section className="flex flex-col gap-4">
        <h2 className="font-semibold text-text">{t('documentsSection')}</h2>
        {REQUIRED_DOCUMENT_TYPES.map((type) => (
          <EmployerDocumentUploadCard
            key={type}
            documentType={type}
            document={profile.documents.find((d) => d.documentType === type)}
            isUploading={uploadDocument.isPending}
            error={uploadDocument.variables?.documentType === type ? errorMessage(uploadDocument.error) : undefined}
            onUpload={(file) => handleUpload(type, file)}
          />
        ))}
      </section>

      {(profile.verificationStatus === 'Draft' || profile.verificationStatus === 'Rejected') && (
        <div className="flex flex-col items-start gap-2">
          <button
            type="button"
            disabled={!canSubmit || submitForReview.isPending}
            onClick={() => submitForReview.mutate()}
            className="rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
          >
            {t('submitForReview')}
          </button>
          {!allDocumentsPresent && <p className="text-sm text-text-muted">{t('submitMissingDocs')}</p>}
          {errorMessage(submitForReview.error) && (
            <p className="text-sm text-danger">{errorMessage(submitForReview.error)}</p>
          )}
          {submitForReview.isSuccess && <p className="text-sm text-success">{t('submitSuccess')}</p>}
        </div>
      )}
    </div>
  )
}
