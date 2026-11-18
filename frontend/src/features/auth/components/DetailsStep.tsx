import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { usePublicOffices } from '../api/auth.queries'
import { jobSeekerDetailsSchema, type JobSeekerDetailsFormValues } from '../schemas/auth.schemas'

export function DetailsStep({
  isSubmitting,
  submitError,
  onSubmit,
}: {
  isSubmitting: boolean
  submitError?: string
  onSubmit: (values: JobSeekerDetailsFormValues) => void
}) {
  const { t } = useTranslation(['auth', 'validation'])
  const { data: offices, isLoading: officesLoading } = usePublicOffices()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<JobSeekerDetailsFormValues>({
    resolver: zodResolver(jobSeekerDetailsSchema),
    defaultValues: { fullName: '', email: '', password: '', officeId: '' },
  })

  return (
    <form onSubmit={(e) => void handleSubmit(onSubmit)(e)} className="flex flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="fullName" className="text-sm font-medium text-text">
          {t('auth:fullNameLabel')}
        </label>
        <input
          id="fullName"
          type="text"
          autoComplete="name"
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          {...register('fullName')}
        />
        {errors.fullName && <p className="text-sm text-danger">{t(errors.fullName.message ?? '')}</p>}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="email" className="text-sm font-medium text-text">
          {t('auth:emailLabel')}
        </label>
        <input
          id="email"
          type="email"
          autoComplete="email"
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          {...register('email')}
        />
        {errors.email && <p className="text-sm text-danger">{t(errors.email.message ?? '')}</p>}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="password" className="text-sm font-medium text-text">
          {t('auth:passwordLabel')}
        </label>
        <input
          id="password"
          type="password"
          autoComplete="new-password"
          className="rounded-md border border-surface-muted px-3 py-2 text-text outline-none focus:border-primary"
          {...register('password')}
        />
        {errors.password && <p className="text-sm text-danger">{t(errors.password.message ?? '')}</p>}
      </div>

      <div className="flex flex-col gap-1">
        <label htmlFor="officeId" className="text-sm font-medium text-text">
          {t('auth:officeLabel')}
        </label>
        <select
          id="officeId"
          disabled={officesLoading}
          defaultValue=""
          className="rounded-md border border-surface-muted bg-surface px-3 py-2 text-text outline-none focus:border-primary"
          {...register('officeId')}
        >
          <option value="" disabled>
            {t('auth:officePlaceholder')}
          </option>
          {offices?.map((office) => (
            <option key={office.id} value={office.id}>
              {office.city ? `${office.name} — ${office.city}` : office.name}
            </option>
          ))}
        </select>
        {errors.officeId && <p className="text-sm text-danger">{t(errors.officeId.message ?? '')}</p>}
      </div>

      {submitError && <p className="text-sm text-danger">{submitError}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-md bg-primary px-4 py-2 font-medium text-surface disabled:opacity-60"
      >
        {t('auth:createAccount')}
      </button>
    </form>
  )
}
