import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useOfficeCourses } from '../api/officeCourses.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { Card } from '@/shared/ui/Card'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20

export default function CoursesManagementPage() {
  const { t } = useTranslation('officeCourses')
  const [pageIndex, setPageIndex] = useState(0)
  const { data, isLoading } = useOfficeCourses({ pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader
        title={t('pageTitle')}
        action={
          <Link to="/office/courses/new">
            <Button variant="primary">{t('createAction')}</Button>
          </Link>
        }
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-3">
          {data.items.map((course) => (
            <Card key={course.id} className="flex flex-wrap items-center justify-between gap-3">
              <div className="flex flex-col gap-1">
                <p className="font-semibold text-content-primary">{course.title}</p>
                <p className="text-xs text-content-secondary">{course.contactPhone}</p>
              </div>
              <div className="flex items-center gap-3">
                <span
                  className={
                    course.status === 'Active'
                      ? 'inline-flex items-center gap-1.5 rounded-full bg-success/10 px-2.5 py-1 text-xs font-medium text-success'
                      : 'inline-flex items-center gap-1.5 rounded-full bg-neutral/10 px-2.5 py-1 text-xs font-medium text-neutral'
                  }
                >
                  {t(`status.${course.status}`)}
                </span>
                <Link to={`/office/courses/${course.id}`}>
                  <Button variant="ghost">{t('viewAction')}</Button>
                </Link>
              </div>
            </Card>
          ))}
        </div>
      )}

      {data && (
        <Pagination pageIndex={pageIndex} totalPages={data.pagination.totalPages} onChange={setPageIndex} />
      )}
    </div>
  )
}
