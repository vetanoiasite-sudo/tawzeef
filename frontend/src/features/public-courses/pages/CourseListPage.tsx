import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useCourseSearch } from '../api/courses.queries'
import { CourseCard } from '../components/CourseCard'
import { useDebouncedValue } from '@/shared/hooks/useDebouncedValue'
import { PageHeader } from '@/shared/ui/PageHeader'
import { TextInput } from '@/shared/ui/TextInput'
import { SearchIcon } from '@/shared/ui/icons'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20

export default function CourseListPage() {
  const { t } = useTranslation('publicCourses')
  const [keyword, setKeyword] = useState('')
  const [pageIndex, setPageIndex] = useState(0)
  const debouncedKeyword = useDebouncedValue(keyword, 400)

  const { data, isLoading } = useCourseSearch({
    keyword: debouncedKeyword.trim() || undefined,
    pageIndex,
    pageSize: PAGE_SIZE,
  })

  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      <TextInput
        value={keyword}
        onChange={(e) => {
          setKeyword(e.target.value)
          setPageIndex(0)
        }}
        placeholder={t('searchPlaceholder')}
        startIcon={<SearchIcon />}
        className="max-w-md border-brand/30 bg-brand-soft/40 focus:border-brand"
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyResults')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {data.items.map((course) => (
            <CourseCard key={course.id} course={course} />
          ))}
        </div>
      )}

      {data && (
        <Pagination pageIndex={pageIndex} totalPages={data.pagination.totalPages} onChange={setPageIndex} />
      )}
    </div>
  )
}
