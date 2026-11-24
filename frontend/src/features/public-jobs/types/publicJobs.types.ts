/** Mirrors backend/src/Tawzeef.Application/Features/PublicJobs/PublicJobDtos.cs. */

export type EmploymentTypeCode = 'FullTime' | 'PartTime' | 'Contract'

export type JobListItem = {
  id: string
  title: string
  employerDisplayName: string
  employerLogoUrl: string | null
  /** Which office this ad belongs to — confirmed 2026-11-24: every job ad must show its office. */
  officeId: string
  officeName: string
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
  // No vacancyCount — never shown to a job seeker (employer/office only), per explicit product
  // decision; the backend DTO doesn't carry it at all.
  employerDisplayName: string
  employerLogoUrl: string | null
  officeId: string
  officeName: string
  publishedAt: string | null
  expiresAt: string | null
  contactNote: string
}

/** Home page "trust strip" — real platform counts. */
export type PublicPlatformStats = {
  activeJobsCount: number
  approvedEmployersCount: number
  approvedJobSeekersCount: number
}

/** A city actually carried by a live ad, with how many — drives the home page "browse by
 * city" section instead of a static governorate list. */
export type PublicCity = {
  city: string
  jobCount: number
}

export type JobSearchFilter = {
  keyword?: string
  city?: string
  category?: string
  employerName?: string
  employmentType?: EmploymentTypeCode
  pageIndex: number
  pageSize: number
}
