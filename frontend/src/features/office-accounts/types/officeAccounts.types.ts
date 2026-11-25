/** Mirrors backend/src/Tawzeef.Application/Features/Accounts/AccountsDtos.cs. */

export type AccountsPaymentListItem = {
  id: string
  employerDisplayName: string
  amount: number
  currency: string
  paidAt: string | null
  recordedByName: string
  receiptUrl: string
}

export type AccountsMonthSummary = {
  year: number
  month: number
  totalAmount: number
  currency: string
  paymentsCount: number
}

export type AccountsFilter = {
  year?: number
  month?: number
  employerName?: string
  pageIndex: number
  pageSize: number
}
