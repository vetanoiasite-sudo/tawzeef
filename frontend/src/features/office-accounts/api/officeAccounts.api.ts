import { httpClient } from '@/shared/api/httpClient'
import type { AccountsFilter, AccountsMonthSummary, AccountsPaymentListItem } from '../types/officeAccounts.types'

export const officeAccountsApi = {
  listPayments: (filter: AccountsFilter) =>
    httpClient.getPaged<AccountsPaymentListItem[]>('/api/office/accounts/payments', {
      year: filter.year,
      month: filter.month,
      employerName: filter.employerName,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
  getSummary: (year?: number, month?: number) =>
    httpClient.get<AccountsMonthSummary>('/api/office/accounts/summary', { query: { year, month } }),
  getReceipt: (id: string) => httpClient.getBlob(`/api/office/accounts/payments/${id}/receipt`),
}
