import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { DocumentUploadCard } from '../components/DocumentUploadCard'
import { MyOfficesCard } from '../components/MyOfficesCard'
import { PhotoUploadCard } from '../components/PhotoUploadCard'
import { ProfileCompletionForm } from '../components/ProfileCompletionForm'
import { ProfileSummaryCard } from '../components/ProfileSummaryCard'
import {
  useMyJobSeekerOffices,
  useMyJobSeekerProfile,
  useSubmitJobSeekerProfileForReview,
  useUpdateMyJobSeekerProfile,
  useUploadJobSeekerDocument,
  useUploadJobSeekerPhoto,
} from '../api/jobSeekerProfile.queries'
import type { JobSeekerProfileFormValues } from '../schemas/jobSeekerProfile.schema'
import {
  REQUIRED_DOCUMENT_TYPES,
  type JobSeekerDocumentTypeCode,
} from '../types/jobSeekerProfile.types'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function JobSeekerProfilePage() {
  const { t } = useTranslation('jobSeekerProfile')
  const { data: profile, isLoading } = useMyJobSeekerProfile()
  const { data: offices } = useMyJobSeekerOffices()
  const updateProfile = useUpdateMyJobSeekerProfile()
  const uploadDocument = useUploadJobSeekerDocument()
  const uploadPhoto = useUploadJobSeekerPhoto()
  const submitForReview = useSubmitJobSeekerProfileForReview()
  const [cvSuggestions, setCvSuggestions] = useState<
    { jobTitle?: string | null; yearsOfExperience?: number | null } | undefined
  >()
  // View-first, edit-on-demand (confirmed 2026-11-25 user report — there was no read-only
  // profile page at all before this, only the edit form shown straight away).
  const [isEditing, setIsEditing] = useState(false)

  // Flash the "saved" confirmation, then clear it — left on screen forever it reads as still
  // true after further edits, when really it's just stale (confirmed 2026-11-25 user report).
  // Resetting the mutation itself (rather than mirroring isSuccess into local state) keeps
  // `updateProfile.isSuccess` as the single source of truth for whether it's showing.
  useEffect(() => {
    if (!updateProfile.isSuccess) return
    const timer = setTimeout(() => updateProfile.reset(), 3000)
    return () => clearTimeout(timer)
    // Only the two properties actually read below belong in the deps — `updateProfile` itself is
    // a new object every render, so depending on it would re-run this effect on every render.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [updateProfile.isSuccess, updateProfile.reset])

  if (isLoading || !profile) {
    return <LoadingState />
  }

  function handleSaveProfile(values: JobSeekerProfileFormValues) {
    updateProfile.mutate(
      {
        email: values.email || undefined,
        address: values.address || undefined,
        city: values.city || undefined,
        dateOfBirth: values.dateOfBirth || undefined,
        education: values.education || undefined,
        yearsOfExperience: values.yearsOfExperience ? Number(values.yearsOfExperience) : undefined,
        skills: values.skills || undefined,
        careerSummary: values.careerSummary || undefined,
        jobTitle: values.jobTitle || undefined,
      },
      { onSuccess: () => setIsEditing(false) },
    )
  }

  function handleUpload(documentType: JobSeekerDocumentTypeCode, file: File) {
    uploadDocument.mutate(
      { documentType, file },
      {
        onSuccess: (document) => {
          if (
            documentType === 'Cv' &&
            (document.suggestedJobTitle || document.suggestedYearsOfExperience != null)
          ) {
            setCvSuggestions({
              jobTitle: document.suggestedJobTitle,
              yearsOfExperience: document.suggestedYearsOfExperience,
            })
          }
        },
      },
    )
  }

  const presentTypes = new Set(profile.documents.map((d) => d.documentType))
  const allDocumentsPresent = REQUIRED_DOCUMENT_TYPES.every((type) => presentTypes.has(type))
  const hasPendingOffice = (offices ?? []).some(
    (o) => o.reviewStatus === 'Draft' || o.reviewStatus === 'Rejected',
  )
  const canSubmit = hasPendingOffice && allDocumentsPresent

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      <MyOfficesCard />

      {!isEditing && <ProfileSummaryCard profile={profile} onEdit={() => setIsEditing(true)} />}

      {isEditing && (
        <section className="flex flex-col gap-4">
          <div className="flex items-center justify-between gap-2">
            <h2 className="text-lg font-semibold text-text">{t('profileSection')}</h2>
            <Button variant="tertiary" size="sm" onClick={() => setIsEditing(false)}>
              {t('backToProfile')}
            </Button>
          </div>

          <PhotoUploadCard
            hasPhoto={profile.hasPhoto}
            isUploading={uploadPhoto.isPending}
            error={errorMessage(uploadPhoto.error)}
            onUpload={(file) => uploadPhoto.mutate(file)}
          />

          <Card>
            <ProfileCompletionForm
              profile={profile}
              isSubmitting={updateProfile.isPending}
              submitError={errorMessage(updateProfile.error)}
              cvSuggestions={cvSuggestions}
              onSubmit={handleSaveProfile}
            />
          </Card>
        </section>
      )}

      <section className="flex flex-col gap-4">
        <h2 className="text-lg font-semibold text-text">{t('documentsSection')}</h2>
        {REQUIRED_DOCUMENT_TYPES.map((type) => (
          <DocumentUploadCard
            key={type}
            documentType={type}
            document={profile.documents.find((d) => d.documentType === type)}
            isUploading={uploadDocument.isPending}
            error={
              uploadDocument.variables?.documentType === type
                ? errorMessage(uploadDocument.error)
                : undefined
            }
            justSucceeded={uploadDocument.isSuccess && uploadDocument.variables?.documentType === type}
            onUpload={(file) => handleUpload(type, file)}
          />
        ))}
      </section>

      {hasPendingOffice && (
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
