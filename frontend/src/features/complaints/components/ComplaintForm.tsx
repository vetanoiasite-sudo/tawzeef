import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { createComplaintSchema, type CreateComplaintFormValues } from '../schemas/complaint.schema'

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
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4 rounded-md border border-surface-muted p-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="subjectType" className="text-sm font-medium text-text">
          {t('complaints:subjectTypeLabel')}
        </label>
        <select
          id="subjectType"
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          {...register('subjectType')}
        >
          <option value="JobSeeker">{t('complaints:subjectType.JobSeeker')}</option>
          <option value="Employer">{t('complaints:subjectType.Employer')}</option>
        </select>
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="subjectId" className="text-sm font-medium text-text">
          {t('complaints:subjectIdLabel')}
        </label>
        <input
          id="subjectId"
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          {...register('subjectId')}
        />
        <p className="text-xs text-text-muted">{t('complaints:subjectIdHint')}</p>
        {errors.subjectId && <p className="text-sm text-danger">{t(errors.subjectId.message ?? '')}</p>}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="complaintText" className="text-sm font-medium text-text">
          {t('complaints:complaintTextLabel')}
        </label>
        <textarea
          id="complaintText"
          rows={4}
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          {...register('complaintText')}
        />
        {errors.complaintText && <p className="text-sm text-danger">{t(errors.complaintText.message ?? '')}</p>}
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
        >
          {t('complaints:submit')}
        </button>
        <button
          type="button"
          onClick={onCancel}
          className="rounded-md border border-surface-muted px-4 py-2 text-sm text-text"
        >
          {t('complaints:cancel')}
        </button>
      </div>
    </form>
  )
}
