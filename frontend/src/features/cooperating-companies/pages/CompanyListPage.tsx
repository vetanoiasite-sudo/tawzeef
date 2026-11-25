import { useState } from 'react'
import { CompanyCard } from '../components/CompanyCard'
import { useCooperatingCompanies } from '../api/cooperatingCompanies.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'

const PAGE_SIZE = 24

export default function CompanyListPage() {
  const [pageIndex, setPageIndex] = useState(0)
  const { data, isLoading } = useCooperatingCompanies({ pageIndex, pageSize: PAGE_SIZE })

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title="الشركات المتعاونة معنا" />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && (
        <EmptyState message="لا توجد شركات متعاونة حاليًا" />
      )}

      {!isLoading && data && data.items.length > 0 && (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-6">
          {data.items.map((company) => (
            <CompanyCard key={company.id} company={company} />
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
  )
}
