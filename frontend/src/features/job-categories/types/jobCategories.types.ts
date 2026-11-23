/** Mirrors backend/src/Tawzeef.Application/Features/JobAdvertisements/JobAdvertisementDtos.cs's
 * JobCategoryDto/AssignJobCategoryDto — office-managed, platform-wide (JobCategory's doc comment). */

export type JobCategory = {
  id: string
  name: string
  displayOrder: number
}

export type AssignJobCategoryPayload = {
  existingCategoryId?: string
  newCategoryName?: string
}
