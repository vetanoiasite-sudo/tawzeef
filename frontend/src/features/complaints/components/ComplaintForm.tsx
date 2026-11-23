import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { createComplaintSchema, type CreateComplaintFormValues } from '../schemas/complaint.schema'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput, TextArea } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'

export function ComplaintForm({
  isSubmitting,
  submitError,
  onSubmit,
  onCancel,
}: {
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: CreateComplaintFormValues) => void
  onCancel: () => void
}) {
  const { t } = useTranslation(['complaints', 'validation'])
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateComplaintFormValues>({
    resolver: zodResolver(createComplaintSchema),
    defaultValues: { subjectType: 'JobSeeker', subjectId: '', complaintText: '' },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)}>
      <Card className="flex flex-col gap-4">
        <Select
          id="subjectType"
          label={t('complaints:subjectTypeLabel')}
          {...register('subjectType')}
        >
          <option value="JobSeeker">{t('complaints:subjectType.JobSeeker')}</option>
          <option value="Employer">{t('complaints:subjectType.Employer')}</option>
        </Select>

        <div>
          <TextInput
            id="subjectId"
            label={t('complaints:subjectIdLabel')}
            error={errors.subjectId ? t(errors.subjectId.message ?? '') : undefined}
            {...register('subjectId')}
          />
          <p className="mt-1 text-xs text-text-muted">{t('complaints:subjectIdHint')}</p>
        </div>

        <TextArea
          id="complaintText"
          rows={4}
          label={t('complaints:complaintTextLabel')}
          error={errors.complaintText ? t(errors.complaintText.message ?? '') : undefined}
          {...register('complaintText')}
        />

        {submitError && <p className="text-sm text-danger">{submitError}</p>}

        <div className="flex gap-2">
          <Button type="submit" variant="primary" disabled={isSubmitting}>
            {t('complaints:submit')}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel}>
            {t('complaints:cancel')}
          </Button>
        </div>
      </Card>
    </form>
  )
}
