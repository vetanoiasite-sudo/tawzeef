import type { CooperatingCompaniesFilter } from '../types/cooperatingCompanies.types'

export const cooperatingCompaniesKeys = {
  all: ['cooperatingCompanies'] as const,
  list: (filter: CooperatingCompaniesFilter) => [...cooperatingCompaniesKeys.all, 'list', filter] as const,
  detail: (id: string) => [...cooperatingCompaniesKeys.all, 'detail', id] as const,
}
