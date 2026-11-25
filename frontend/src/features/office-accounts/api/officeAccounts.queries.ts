import { useQuery } from '@tanstack/react-query'
import { officeAccountsApi } from './officeAccounts.api'
import { officeAccountsKeys } from './officeAccounts.keys'
import type { AccountsFilter } from '../types/officeAccounts.types'

export function useOfficeAccountsPayments(filter: AccountsFilter) {
  return useQuery({
    queryKey: officeAccountsKeys.list(filter),
    queryFn: () => officeAccountsApi.listPayments(filter),
  })
}

export function useOfficeAccountsSummary(year?: number, month?: number) {
  return useQuery({
    queryKey: officeAccountsKeys.summary(year, month),
    queryFn: () => officeAccountsApi.getSummary(year, month),
  })
}
