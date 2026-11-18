import { z } from 'zod'

export const jobSeekerProfileFormSchema = z.object({
  email: z.union([z.literal(''), z.email('validation:emailFormat')]),
  address: z.string().trim().max(300, 'validation:tooLong'),
  city: z.string().trim().max(100, 'validation:tooLong'),
  dateOfBirth: z.string(),
  education: z.string().trim().max(200, 'validation:tooLong'),
  yearsOfExperience: z
    .string()
    .refine((v) => v === '' || (/^\d+$/.test(v) && Number(v) <= 80), 'validation:invalidNumber'),
  skills: z.string().trim().max(1000, 'validation:tooLong'),
  careerSummary: z.string().trim().max(2000, 'validation:tooLong'),
})
export type JobSeekerProfileFormValues = z.infer<typeof jobSeekerProfileFormSchema>
