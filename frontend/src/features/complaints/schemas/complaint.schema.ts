import { z } from 'zod'

export const createComplaintSchema = z.object({
  subjectType: z.enum(['JobSeeker', 'Employer']),
  subjectId: z.string().trim().min(1, 'validation:required'),
  complaintText: z.string().trim().min(10, 'validation:required').max(2000, 'validation:tooLong'),
})
export type CreateComplaintFormValues = z.infer<typeof createComplaintSchema>
