import { z } from 'zod'

export const jobAdFormSchema = z.object({
  /** Which of the employer's own Approved offices this ad posts through — fixed once set on an
   * existing ad (JobAdvertisementService.UpdateAsync never changes it), only ever chosen on create. */
  officeId: z.string().min(1, 'validation:required'),
  title: z.string().trim().min(1, 'validation:required').max(200, 'validation:tooLong'),
  description: z.string().trim().max(4000, 'validation:tooLong'),
  experienceRequirement: z.string().trim().max(200, 'validation:tooLong'),
  workingHours: z.string().trim().max(100, 'validation:tooLong'),
  address: z.string().trim().max(300, 'validation:tooLong'),
  category: z.string().trim().max(100, 'validation:tooLong'),
  city: z.string().trim().max(100, 'validation:tooLong'),
  employmentType: z.union([z.literal(''), z.enum(['FullTime', 'PartTime', 'Contract'])]),
  salaryMin: z.union([z.literal(''), z.coerce.number().nonnegative('validation:invalid')]),
  salaryMax: z.union([z.literal(''), z.coerce.number().nonnegative('validation:invalid')]),
  vacancyCount: z.coerce.number().int().min(1, 'validation:invalid'),
})
export type JobAdFormValues = z.infer<typeof jobAdFormSchema>
