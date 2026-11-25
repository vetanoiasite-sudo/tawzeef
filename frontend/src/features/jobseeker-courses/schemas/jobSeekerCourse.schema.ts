import { z } from 'zod'

export const createJobSeekerCourseSchema = z.object({
  title: z.string().trim().min(1, 'validation:required').max(200, 'validation:tooLong'),
  institution: z.string().trim().min(1, 'validation:required').max(200, 'validation:tooLong'),
  whatLearned: z.string().trim().max(2000, 'validation:tooLong').optional(),
})
export type CreateJobSeekerCourseFormValues = z.infer<typeof createJobSeekerCourseSchema>
