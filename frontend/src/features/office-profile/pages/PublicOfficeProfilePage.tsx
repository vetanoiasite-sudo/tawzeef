import { useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { OfficeProfileSummaryCard } from '../components/OfficeProfileSummaryCard'
import { usePublicOffice } from '../api/officeProfile.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { LoadingState } from '@/shared/ui/LoadingState'
import { EmptyState } from '@/shared/ui/EmptyState'

export default function PublicOfficeProfilePage() {
  const { t } = useTranslation('officeProfile')
  const { id } = useParams<{ id: string }>()
  const { data: office, isLoading } = usePublicOffice(id ?? '')

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('publicPageTitle')} />

      {isLoading && <LoadingState />}
      {!isLoading && !office && <EmptyState message={t('officeNotFound')} />}
      {!isLoading && office && <OfficeProfileSummaryCard office={office} />}
    </div>
  )
}
