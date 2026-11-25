import { httpClient } from '@/shared/api/httpClient'
import type {
  CooperatingCompaniesFilter,
  CooperatingCompany,
  CooperatingCompanyDetail,
} from '../types/cooperatingCompanies.types'

export const cooperatingCompaniesApi = {
  // Public endpoint — Approved + Active-cooperation employers only (PublicEmployerListItemDto's
  // doc comment). Used by both the home page's showcase and its "view all" page.
  list: (filter: CooperatingCompaniesFilter) =>
    httpClient.getPaged<CooperatingCompany[]>('/api/employers/public', {
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getById: (id: string) =>
    httpClient.get<CooperatingCompanyDetail>(`/api/employers/${id}/public`),
}
