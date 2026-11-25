import { z } from 'zod'

export const officeProfileFormSchema = z.object({
  name: z.string().trim().min(1, 'validation:required').max(200, 'validation:tooLong'),
  arName: z.string().trim().max(200, 'validation:tooLong'),
  address: z.string().trim().max(300, 'validation:tooLong'),
  city: z.string().trim().max(100, 'validation:tooLong'),
  phone: z.string().trim().max(30, 'validation:tooLong'),
  email: z.union([z.literal(''), z.email('validation:emailFormat')]),
})
export type OfficeProfileFormValues = z.infer<typeof officeProfileFormSchema>
