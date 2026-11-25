import { useQuery } from '@tanstack/react-query'
import { cooperatingCompaniesApi } from './cooperatingCompanies.api'
import { cooperatingCompaniesKeys } from './cooperatingCompanies.keys'
import type { CooperatingCompaniesFilter } from '../types/cooperatingCompanies.types'

export function useCooperatingCompanies(filter: CooperatingCompaniesFilter) {
  return useQuery({
    queryKey: cooperatingCompaniesKeys.list(filter),
    queryFn: () => cooperatingCompaniesApi.list(filter),
    staleTime: 60_000,
  })
}

export function useCooperatingCompany(id: string) {
  return useQuery({
    queryKey: cooperatingCompaniesKeys.detail(id),
    queryFn: () => cooperatingCompaniesApi.getById(id),
    enabled: id.length > 0,
  })
}
