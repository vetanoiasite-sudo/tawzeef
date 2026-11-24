import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'
import { JobCard } from '../components/JobCard'
import { useJobSearch } from '../api/publicJobs.queries'
import type { EmploymentTypeCode } from '../types/publicJobs.types'
import { useJobCategories } from '@/features/job-categories'
import { useGovernorates } from '@/features/governorates'
import { useDebouncedValue } from '@/shared/hooks/useDebouncedValue'
import { PageHeader } from '@/shared/ui/PageHeader'
import { TextInput } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { Card } from '@/shared/ui/Card'
import { SearchIcon } from '@/shared/ui/icons'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 20
const EMPLOYMENT_TYPES: EmploymentTypeCode[] = ['FullTime', 'PartTime', 'Contract']

export default function JobListPage() {
  const { t } = useTranslation('publicJobs')
  // Pre-filled from the URL (e.g. a "browse by category/city" tile on the home page linking to
  // /jobs?category=... or /jobs?city=...) — read once on mount, same as every other filter here.
  const [searchParams] = useSearchParams()
  const [keyword, setKeyword] = useState(searchParams.get('keyword') ?? '')
  const [city, setCity] = useState(searchParams.get('city') ?? '')
  const [category, setCategory] = useState(searchParams.get('category') ?? '')
  const [employerName, setEmployerName] = useState('')
  const [employmentType, setEmploymentType] = useState<EmploymentTypeCode | ''>('')
  const [pageIndex, setPageIndex] = useState(0)

  /* Debounced: the DB must not get a request per keystroke. The query only fires once the
   * user stops typing for 400ms (useDebouncedValue), not on every onChange. City/Category are
   * selects (discrete choices, not typed), so they fire their own filter change immediately. */
  const debouncedKeyword = useDebouncedValue(keyword, 400)
  const debouncedEmployerName = useDebouncedValue(employerName, 400)

  const { data: categories } = useJobCategories()
  const { data: governorates } = useGovernorates()

  const { data, isLoading } = useJobSearch({
    keyword: debouncedKeyword.trim() || undefined,
    city: city || undefined,
    category: category || undefined,
    employerName: debouncedEmployerName.trim() || undefined,
    employmentType: employmentType || undefined,
    pageIndex,
    pageSize: PAGE_SIZE,
  })

  return (
    <div className="flex flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />

      <div className="flex flex-col gap-6 lg:flex-row">
        <Card className="flex h-fit flex-col gap-4 lg:w-72 lg:shrink-0">
          {/* The keyword field is the actual "search" — a leading search icon plus a distinct
           *  border/background mark it as the primary control, not just another form field, so
           *  it reads unmistakably as search even though results update live (no submit button). */}
          <TextInput
            label={t('searchAction')}
            value={keyword}
            onChange={(e) => {
              setKeyword(e.target.value)
              setPageIndex(0)
            }}
            placeholder={t('searchPlaceholder')}
            startIcon={<SearchIcon />}
            className="border-brand/30 bg-brand-soft/40 focus:border-brand"
          />
          {/* Governorates come from the real Governorates table (api/governorates), not free
           *  text — a dropdown is the only way to keep the value matchable. */}
          <Select
            label={t('cityPlaceholder')}
            value={city}
            onChange={(e) => {
              setCity(e.target.value)
              setPageIndex(0)
            }}
          >
            <option value="">{t('cityAll')}</option>
            {(governorates ?? []).map((g) => (
              <option key={g.id} value={g.name}>
                {g.name}
              </option>
            ))}
          </Select>
          {/* Real office-managed categories now (JobCategory), not free text — a typed value
           *  could never match since a Published ad's category is always office-assigned. */}
          <Select
            label={t('categoryPlaceholder')}
            value={category}
            onChange={(e) => {
              setCategory(e.target.value)
              setPageIndex(0)
            }}
          >
            <option value="">{t('categoryPlaceholder')}</option>
            {categories?.map((c) => (
              <option key={c.id} value={c.name}>
                {c.name}
              </option>
            ))}
          </Select>
          <TextInput
            label={t('employerFilterLabel')}
            value={employerName}
            onChange={(e) => {
              setEmployerName(e.target.value)
              setPageIndex(0)
            }}
            placeholder={t('employerFilterLabel')}
          />
          <Select
            label={t('employmentTypeAll')}
            value={employmentType}
            onChange={(e) => {
              setEmploymentType(e.target.value as EmploymentTypeCode | '')
              setPageIndex(0)
            }}
          >
            <option value="">{t('employmentTypeAll')}</option>
            {EMPLOYMENT_TYPES.map((et) => (
              <option key={et} value={et}>
                {t(`employmentType.${et}`)}
              </option>
            ))}
          </Select>
        </Card>

        <div className="flex flex-1 flex-col gap-4">
          {isLoading && <LoadingState />}

          {!isLoading && data && data.items.length === 0 && (
            <EmptyState message={t('emptyResults')} />
          )}

          {!isLoading && data && data.items.length > 0 && (
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
              {data.items.map((job) => (
                <JobCard key={job.id} job={job} />
              ))}
            </div>
          )}

          {data && (
            <Pagination
              pageIndex={pageIndex}
              totalPages={data.pagination.totalPages}
              onChange={setPageIndex}
            />
          )}
        </div>
      </div>
    </div>
  )
}
