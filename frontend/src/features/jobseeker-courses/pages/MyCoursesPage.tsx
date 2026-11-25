import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { CourseCard } from '../components/CourseCard'
import { CourseForm } from '../components/CourseForm'
import {
  useCreateCourse,
  useDeleteCourse,
  useMyCourses,
  useUploadCourseCertificate,
} from '../api/jobSeekerCourse.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function MyCoursesPage() {
  const { t } = useTranslation('jobSeekerCourses')
  const [showForm, setShowForm] = useState(false)

  const { data: courses, isLoading } = useMyCourses()
  const createCourse = useCreateCourse()
  const uploadCertificate = useUploadCourseCertificate()
  const deleteCourse = useDeleteCourse()

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader
        title={t('pageTitle')}
        action={
          !showForm && (
            <Button variant="primary" onClick={() => setShowForm(true)}>
              {t('addNew')}
            </Button>
          )
        }
      />

      {showForm && (
        <CourseForm
          isSubmitting={createCourse.isPending || uploadCertificate.isPending}
          submitError={errorMessage(createCourse.error)}
          onSubmit={(values, certificateFile) =>
            createCourse.mutate(
              {
                title: values.title,
                institution: values.institution,
                whatLearned: values.whatLearned || undefined,
              },
              {
                onSuccess: (newCourse) => {
                  setShowForm(false)
                  // Same submit, no separate step (confirmed 2026-11-25 user report) — the
                  // certificate can only go up once the course exists, so this chains right
                  // after create succeeds instead of making the user come back for it.
                  if (certificateFile) uploadCertificate.mutate({ id: newCourse.id, file: certificateFile })
                },
              },
            )
          }
          onCancel={() => setShowForm(false)}
        />
      )}

      {isLoading && <LoadingState />}

      {!isLoading && courses && courses.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && courses && courses.length > 0 && (
        <div className="flex flex-col gap-3">
          {courses.map((course) => (
            <CourseCard
              key={course.id}
              course={course}
              isUploadingCertificate={uploadCertificate.isPending}
              certificateError={
                uploadCertificate.variables?.id === course.id
                  ? errorMessage(uploadCertificate.error)
                  : undefined
              }
              isDeleting={deleteCourse.isPending}
              onUploadCertificate={(file) => uploadCertificate.mutate({ id: course.id, file })}
              onDelete={() => {
                if (window.confirm(t('deleteConfirm'))) deleteCourse.mutate(course.id)
              }}
            />
          ))}
        </div>
      )}
    </div>
  )
}
