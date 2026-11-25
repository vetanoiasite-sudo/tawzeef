import { zodResolver } from '@hookform/resolvers/zod'
import { useRef, useState, type ChangeEvent } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import {
  createJobSeekerCourseSchema,
  type CreateJobSeekerCourseFormValues,
} from '../schemas/jobSeekerCourse.schema'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput, TextArea } from '@/shared/ui/TextInput'

export function CourseForm({
  isSubmitting,
  submitError,
  onSubmit,
  onCancel,
}: {
  isSubmitting: boolean
  submitError?: string
  /** The certificate file (if the user picked one) goes along with the same submit — no separate
   * step needed to attach it (confirmed 2026-11-25 user report: it used to be add-course first,
   * then a second action to upload the certificate). */
  onSubmit: (values: CreateJobSeekerCourseFormValues, certificateFile: File | null) => void
  onCancel: () => void
}) {
  const { t } = useTranslation(['jobSeekerCourses', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateJobSeekerCourseFormValues>({
    resolver: zodResolver(createJobSeekerCourseSchema),
    defaultValues: { title: '', institution: '', whatLearned: '' },
  })

  const inputRef = useRef<HTMLInputElement>(null)
  const [certificateFile, setCertificateFile] = useState<File | null>(null)

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    setCertificateFile(e.target.files?.[0] ?? null)
  }

  return (
    <form onSubmit={(e) => void handleSubmit((values) => onSubmit(values, certificateFile))(e)}>
      <Card className="flex flex-col gap-4">
        <TextInput
          id="title"
          label={t('jobSeekerCourses:titleLabel')}
          error={errors.title ? t(errors.title.message ?? '') : undefined}
          {...register('title')}
        />

        <TextInput
          id="institution"
          label={t('jobSeekerCourses:institutionLabel')}
          error={errors.institution ? t(errors.institution.message ?? '') : undefined}
          {...register('institution')}
        />

        <TextArea
          id="whatLearned"
          rows={3}
          label={t('jobSeekerCourses:whatLearnedLabel')}
          error={errors.whatLearned ? t(errors.whatLearned.message ?? '') : undefined}
          {...register('whatLearned')}
        />

        <div className="flex flex-col gap-1.5">
          <span className="text-[14px] font-bold text-content-primary">
            {t('jobSeekerCourses:certificateLabel')}
          </span>
          <input
            ref={inputRef}
            type="file"
            accept=".pdf,.jpg,.jpeg,.png,.xlsx,.xls,.doc,.docx,application/pdf,image/jpeg,image/png,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document"
            className="hidden"
            onChange={handleFileChange}
          />
          <Button
            type="button"
            variant="secondary"
            className="self-start"
            onClick={() => inputRef.current?.click()}
          >
            {certificateFile ? t('jobSeekerCourses:replaceCertificate') : t('jobSeekerCourses:uploadCertificate')}
          </Button>
          <p className="text-xs text-text-muted">
            {certificateFile ? certificateFile.name : t('jobSeekerCourses:noCertificateUploaded')}
          </p>
        </div>

        {submitError && <p className="text-sm text-danger">{submitError}</p>}

        <div className="flex gap-2">
          <Button type="submit" variant="primary" disabled={isSubmitting}>
            {t('jobSeekerCourses:save')}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel}>
            {t('jobSeekerCourses:cancel')}
          </Button>
        </div>
      </Card>
    </form>
  )
}
