import { useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import {
  useOfficeCourse,
  useToggleCoursePause,
  useUpdateCourse,
  useUploadCourseImage,
} from '../api/officeCourses.queries'
import type { OfficeCourse } from '../types/officeCourses.types'
import { ApiError } from '@/shared/api/types'
import { env } from '@/shared/config/env'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput, TextArea } from '@/shared/ui/TextInput'
import { LoadingState } from '@/shared/ui/LoadingState'
import { EmptyState } from '@/shared/ui/EmptyState'

export default function CourseDetailPage() {
  const { t } = useTranslation('officeCourses')
  const { id } = useParams<{ id: string }>()
  const courseId = id ?? ''

  const { data: course, isLoading } = useOfficeCourse(courseId)

  if (isLoading) return <LoadingState />
  if (!course)
    return (
      <div className="px-4 py-8 sm:px-6">
        <EmptyState message={t('emptyList')} />
      </div>
    )

  // Keyed on the course id so this remounts (and its form state re-initializes from `course`)
  // if the route ever navigates straight from one course to another.
  return <CourseEditPanel key={course.id} course={course} />
}

function CourseEditPanel({ course }: { course: OfficeCourse }) {
  const { t } = useTranslation('officeCourses')
  const update = useUpdateCourse(course.id)
  const uploadImage = useUploadCourseImage(course.id)
  const togglePause = useToggleCoursePause()

  const [title, setTitle] = useState(course.title)
  const [description, setDescription] = useState(course.description ?? '')
  const [contactPhone, setContactPhone] = useState(course.contactPhone)
  const inputRef = useRef<HTMLInputElement>(null)

  const updateError = update.error instanceof ApiError ? update.error.messages[0] : undefined
  const uploadError = uploadImage.error instanceof ApiError ? uploadImage.error.messages[0] : undefined

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    update.mutate({ title: title.trim(), description: description.trim(), contactPhone: contactPhone.trim() })
  }

  function handleFileChange(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (file) uploadImage.mutate(file)
    e.target.value = ''
  }

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link to="/office/courses" className="text-sm font-medium text-brand hover:underline">
        {t('backToList')}
      </Link>
      <PageHeader title={course.title} />

      {course.status === 'Paused' && (
        <p className="rounded-200 bg-warning/10 px-4 py-3 text-sm text-warning">{t('pausedHint')}</p>
      )}

      <Card className="flex flex-col gap-3">
        <span className="font-medium text-content-primary">{t('imageLabel')}</span>
        <div className="flex items-center gap-4">
          <div className="flex h-20 w-32 shrink-0 items-center justify-center overflow-hidden rounded-200 border border-border-primary bg-surface-muted">
            {course.imageUrl ? (
              <img
                src={`${env.apiBaseUrl}${course.imageUrl}`}
                alt={course.title}
                className="size-full object-cover"
              />
            ) : (
              <span className="text-xs text-content-tertiary">—</span>
            )}
          </div>
          <div className="flex flex-col gap-2">
            <input
              ref={inputRef}
              type="file"
              accept="image/jpeg,image/png"
              className="hidden"
              onChange={handleFileChange}
            />
            <Button
              variant="secondary"
              disabled={uploadImage.isPending}
              onClick={() => inputRef.current?.click()}
              className="self-start"
            >
              {course.imageUrl ? t('replaceImage') : t('uploadImage')}
            </Button>
            <p className="text-xs text-content-tertiary">{t('imageUploadHint')}</p>
          </div>
        </div>
        {uploadError && <p className="text-sm text-danger">{uploadError}</p>}
      </Card>

      <form onSubmit={handleSubmit}>
        <Card className="flex flex-col gap-4">
          <TextInput
            id="courseTitle"
            label={t('titleLabel')}
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
          <TextArea
            id="courseDescription"
            label={t('descriptionLabel')}
            rows={4}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <TextInput
            id="courseContactPhone"
            type="tel"
            label={t('contactPhoneLabel')}
            value={contactPhone}
            onChange={(e) => setContactPhone(e.target.value)}
          />

          {updateError && <p className="text-sm text-danger">{updateError}</p>}
          {update.isSuccess && <p className="text-sm text-success">{t('saveSuccess')}</p>}

          <div className="flex flex-wrap gap-2">
            <Button
              type="submit"
              variant="primary"
              disabled={update.isPending || !title.trim() || !contactPhone.trim()}
            >
              {t('save')}
            </Button>
            <Button
              type="button"
              variant="secondary"
              disabled={togglePause.isPending}
              onClick={() => togglePause.mutate({ id: course.id, pause: course.status === 'Active' })}
            >
              {course.status === 'Active' ? t('pauseAction') : t('resumeAction')}
            </Button>
          </div>
        </Card>
      </form>
    </div>
  )
}
