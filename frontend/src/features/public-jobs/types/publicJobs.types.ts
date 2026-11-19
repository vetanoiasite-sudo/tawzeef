/** Mirrors backend/src/Tawzeef.Application/Features/PublicJobs/PublicJobDtos.cs. */

export type EmploymentTypeCode = 'FullTime' | 'PartTime' | 'Contract'

export type JobListItem = {
  id: string
  title: string
  employerDisplayName: string
  city: string | null
  category: string | null
  employmentType: EmploymentTypeCode | null
  experienceRequirement: string | null
  publishedAt: string | null
  expiresAt: string | null
}

export type JobDetails = {
  id: string
  title: string
  description: string | null
  experienceRequirement: string | null
  workingHours: string | null
  address: string | null
  city: string | null
  category: string | null
  employmentType: EmploymentTypeCode | null
  salaryMin: number | null
  salaryMax: number | null
  vacancyCount: number
  employerDisplayName: string
  publishedAt: string | null
  expiresAt: string | null
  contactNote: string
}

export type JobSearchFilter = {
  keyword?: string
  city?: string
  category?: string
  employmentType?: EmploymentTypeCode
  pageIndex: number
  pageSize: number
}
