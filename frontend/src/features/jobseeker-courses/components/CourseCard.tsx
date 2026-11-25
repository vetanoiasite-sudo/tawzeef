import { useRef, type ChangeEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { JobSeekerCourse } from '../types/jobSeekerCourse.types'
import { useFilePreview } from '@/shared/hooks/useFilePreview'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { FilePreviewModal } from '@/shared/ui/FilePreviewModal'

export function CourseCard({
  course,
  isUploadingCertificate,
  certificateError,
  isDeleting,
  onUploadCertificate,
  onDelete,
}: {
  course: JobSeekerCourse
  isUploadingCertificate: boolean
  certificateError?: string
  isDeleting: boolean
  onUploadCertificate: (file: File) => void
  onDelete: () => void
}) {
  const { t } = useTranslation('jobSeekerCourses')
  const inputRef = useRef<HTMLInputElement>(null)
  const { preview, isLoading: isLoadingPreview, open: openPreview, close: closePreview } = useFilePreview()

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) onUploadCertificate(file)
    e.target.value = ''
  }

  return (
    <Card className="flex flex-col gap-2">
      <div className="flex items-center justify-between gap-2">
        <span className="font-medium text-text">{course.title}</span>
        <span className="text-sm text-text-muted">{course.institution}</span>
      </div>

      {course.whatLearned && <p className="text-sm text-text">{course.whatLearned}</p>}

      <p className="text-sm text-text-muted">
        {course.hasCertificate ? course.certificateFileName : t('noCertificateUploaded')}
      </p>

      <input
        ref={inputRef}
        type="file"
        accept=".pdf,.jpg,.jpeg,.png,.xlsx,.xls,.doc,.docx,application/pdf,image/jpeg,image/png,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        className="hidden"
        onChange={handleFileChange}
      />

      <div className="flex flex-wrap gap-2">
        <Button
          variant="secondary"
          disabled={isUploadingCertificate}
          onClick={() => inputRef.current?.click()}
        >
          {course.hasCertificate ? t('replaceCertificate') : t('uploadCertificate')}
        </Button>
        {course.hasCertificate && (
          <Button
            variant="secondary"
            disabled={isLoadingPreview}
            onClick={() => void openPreview(`/api/jobseekers/me/courses/${course.id}/certificate`)}
          >
            {t('viewCertificate')}
          </Button>
        )}
        <Button variant="danger" disabled={isDeleting} onClick={onDelete}>
          {t('delete')}
        </Button>
      </div>
      {certificateError && <p className="text-sm text-danger">{certificateError}</p>}

      {preview && <FilePreviewModal preview={preview} onClose={closePreview} />}
    </Card>
  )
}
