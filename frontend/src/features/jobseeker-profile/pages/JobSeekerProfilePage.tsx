import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { DocumentUploadCard } from '../components/DocumentUploadCard'
import { ProfileCompletionForm } from '../components/ProfileCompletionForm'
import {
  useMyJobSeekerProfile,
  useSubmitJobSeekerProfileForReview,
  useUpdateMyJobSeekerProfile,
  useUploadJobSeekerDocument,
} from '../api/jobSeekerProfile.queries'
import type { JobSeekerProfileFormValues } from '../schemas/jobSeekerProfile.schema'
import { REQUIRED_DOCUMENT_TYPES, type JobSeekerDocumentTypeCode } from '../types/jobSeekerProfile.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function JobSeekerProfilePage() {
  const { t } = useTranslation('jobSeekerProfile')
  const { data: profile, isLoading } = useMyJobSeekerProfile()
  const updateProfile = useUpdateMyJobSeekerProfile()
  const uploadDocument = useUploadJobSeekerDocument()
  const submitForReview = useSubmitJobSeekerProfileForReview()

  if (isLoading || !profile) {
    return <LoadingState />
  }

  function handleSaveProfile(values: JobSeekerProfileFormValues) {
    updateProfile.mutate({
      email: values.email || undefined,
      address: values.address || undefined,
      city: values.city || undefined,
      dateOfBirth: values.dateOfBirth || undefined,
      education: values.education || undefined,
      yearsOfExperience: values.yearsOfExperience ? Number(values.yearsOfExperience) : undefined,
      skills: values.skills || undefined,
      careerSummary: values.careerSummary || undefined,
    })
  }

  function handleUpload(documentType: JobSeekerDocumentTypeCode, file: File) {
    uploadDocument.mutate({ documentType, file })
  }

  const presentTypes = new Set(profile.documents.map((d) => d.documentType))
  const allDocumentsPresent = REQUIRED_DOCUMENT_TYPES.every((type) => presentTypes.has(type))
  const canSubmit = (profile.reviewStatus === 'Draft' || profile.reviewStatus === 'Rejected') && allDocumentsPresent

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      <Card className="flex flex-col gap-1.5">
        <div className="flex items-center gap-2 text-sm text-text-muted">
          {t('statusLabel')}:
          <StatusTag domain="jobSeekerReview" value={profile.reviewStatus} label={t(`status.${profile.reviewStatus}`)} />
        </div>
        {profile.rejectionReason && (
          <p className="text-sm text-danger">
            {t('rejectionReasonLabel')}: {profile.rejectionReason}
          </p>
        )}
      </Card>

      <section className="flex flex-col gap-4">
        <h2 className="text-lg font-semibold text-text">{t('profileSection')}</h2>
        <Card>
          <ProfileCompletionForm
            profile={profile}
            isSubmitting={updateProfile.isPending}
            submitError={errorMessage(updateProfile.error)}
            onSubmit={handleSaveProfile}
          />
          {updateProfile.isSuccess && <p className="mt-3 text-sm text-success">{t('saveSuccess')}</p>}
        </Card>
      </section>

      <section className="flex flex-col gap-4">
        <h2 className="text-lg font-semibold text-text">{t('documentsSection')}</h2>
        {REQUIRED_DOCUMENT_TYPES.map((type) => (
          <DocumentUploadCard
            key={type}
            documentType={type}
            document={profile.documents.find((d) => d.documentType === type)}
            isUploading={uploadDocument.isPending}
            error={uploadDocument.variables?.documentType === type ? errorMessage(uploadDocument.error) : undefined}
            onUpload={(file) => handleUpload(type, file)}
          />
        ))}
      </section>

      {(profile.reviewStatus === 'Draft' || profile.reviewStatus === 'Rejected') && (
        <div className="flex flex-col items-start gap-2">
          <Button disabled={!canSubmit || submitForReview.isPending} onClick={() => submitForReview.mutate()}>
            {t('submitForReview')}
          </Button>
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
