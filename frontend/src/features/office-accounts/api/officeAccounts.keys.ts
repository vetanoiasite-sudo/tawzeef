import type { AccountsFilter } from '../types/officeAccounts.types'

export const officeAccountsKeys = {
  all: ['officeAccounts'] as const,
  list: (filter: AccountsFilter) => [...officeAccountsKeys.all, 'list', filter] as const,
  summary: (year?: number, month?: number) => [...officeAccountsKeys.all, 'summary', year, month] as const,
}
