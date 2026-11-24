import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { useCourseDetails } from '../api/courses.queries'
import { env } from '@/shared/config/env'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { LoadingState } from '@/shared/ui/LoadingState'
import { EmptyState } from '@/shared/ui/EmptyState'

export default function CourseDetailPage() {
  const { t } = useTranslation('publicCourses')
  const { id } = useParams<{ id: string }>()
  const courseId = id ?? ''

  const { data: course, isLoading } = useCourseDetails(courseId)

  if (isLoading) return <LoadingState />
  if (!course)
    return (
      <div className="px-4 py-8 sm:px-6">
        <EmptyState message={t('notFound')} />
      </div>
    )

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-5 px-4 py-8 sm:px-6">
      <Link to="/courses" className="text-sm font-medium text-brand hover:underline">
        {t('backToList')}
      </Link>

      <Card className="flex flex-col gap-5">
        {course.imageUrl && (
          <div className="-m-6 mb-0 flex h-56 w-[calc(100%+3rem)] items-center justify-center overflow-hidden rounded-t-300 bg-surface-muted">
            <img
              src={`${env.apiBaseUrl}${course.imageUrl}`}
              alt={course.title}
              className="size-full object-cover"
            />
          </div>
        )}

        <div className="flex flex-col gap-1">
          <h1 className="text-xl font-bold text-content-primary">{course.title}</h1>
          <p className="text-sm text-content-secondary">
            {t('offeredByLabel')}: {course.officeName}
          </p>
        </div>

        {course.description && (
          <div className="border-t border-border-primary pt-5 text-sm">
            <p className="text-content-secondary">{t('descriptionLabel')}</p>
            <p className="whitespace-pre-line text-content-primary">{course.description}</p>
          </div>
        )}

        <div className="flex flex-col gap-2 border-t border-border-primary pt-5">
          <p className="text-sm text-content-secondary">{t('contactLabel')}</p>
          <a href={`tel:${course.contactPhone}`} className="w-fit">
            <Button variant="primary">
              {course.contactPhone} — {t('callAction')}
            </Button>
          </a>
        </div>
      </Card>
    </div>
  )
}
