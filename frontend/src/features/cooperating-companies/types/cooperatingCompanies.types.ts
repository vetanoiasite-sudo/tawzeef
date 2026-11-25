/** Mirrors backend/src/Tawzeef.Application/Features/Employers/EmployerProfileDtos.cs's
 * PublicEmployerListItemDto/PublicEmployerDetailDto — the home page's "cooperating companies"
 * showcase, its "view all" page, and each company's public profile page. */

export type CooperatingCompany = {
  id: string
  displayName: string
  city: string | null
  logoUrl: string | null
}

export type CooperatingCompanyDetail = CooperatingCompany & {
  businessActivity: string | null
}

export type CooperatingCompaniesFilter = {
  pageIndex: number
  pageSize: number
}
