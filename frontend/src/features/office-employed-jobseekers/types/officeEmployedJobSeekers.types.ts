/** Mirrors backend/src/Tawzeef.Application/Features/JobSeekers/JobSeekerReviewDtos.cs's
 * EmployedJobSeekerListItemDto/EmployedJobSeekersQueryDto. */

export type EmployedJobSeekerListItem = {
  jobSeekerId: string
  fullName: string
  mobileNumber: string
  employedAt: string
  leftJobAt: string | null
}

export type EmployedJobSeekersQuery = {
  pageIndex: number
  pageSize: number
}
