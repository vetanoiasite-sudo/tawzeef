import { z } from 'zod'

export const employerProfileFormSchema = z.object({
  businessActivity: z.string().trim().max(200, 'validation:tooLong'),
  address: z.string().trim().max(300, 'validation:tooLong'),
  city: z.string().trim().max(100, 'validation:tooLong'),
  companyEmail: z.union([z.literal(''), z.email('validation:emailFormat')]),
  companyPhone: z
    .string()
    .trim()
    .refine((v) => v === '' || /^\+?[0-9]{8,15}$/.test(v), 'validation:phoneFormat'),
  representativeName: z.string().trim().max(200, 'validation:tooLong'),
  representativeRole: z.string().trim().max(100, 'validation:tooLong'),
})
export type EmployerProfileFormValues = z.infer<typeof employerProfileFormSchema>
