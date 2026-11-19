import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { JobAdForm } from '../components/JobAdForm'
import {
  useCloseJobAd,
  useMyJobAdDetail,
  useRenewJobAd,
  useSubmitJobAd,
  useUpdateJobAd,
} from '../api/employerJobAds.queries'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function JobAdDetailPage() {
  const { t } = useTranslation('employerJobAds')
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const adId = id ?? ''
  const [isEditing, setIsEditing] = useState(false)

  const { data: ad, isLoading } = useMyJobAdDetail(adId)
  const update = useUpdateJobAd(adId)
  const submit = useSubmitJobAd(adId)
  const close = useCloseJobAd(adId)
  const renew = useRenewJobAd(adId)

  if (isLoading || !ad) {
    return <div className="mx-auto max-w-2xl px-4 py-10 text-text-muted">...</div>
  }

  const canEdit = ad.status === 'Draft' || ad.status === 'Rejected' || ad.status === 'Published'
  const canSubmit = ad.status === 'Draft'
  const canClose = ad.status === 'Published'
  const canRenew = ad.status === 'Expired'

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-10">
      <Link to="/employer/job-advertisements" className="text-sm text-primary">
        {t('backToList')}
      </Link>

      {isEditing ? (
        <>
          <h1 className="text-lg font-semibold text-text">{ad.title}</h1>
          <JobAdForm
            ad={ad}
            isSubmitting={update.isPending}
            submitError={errorMessage(update.error)}
            onSubmit={(values) => update.mutate(values, { onSuccess: () => setIsEditing(false) })}
          />
          <button type="button" onClick={() => setIsEditing(false)} className="self-start text-sm text-text-muted">
            {t('cancelAction')}
          </button>
        </>
      ) : (
        <>
          <div className="flex items-center justify-between">
            <h1 className="text-lg font-semibold text-text">{ad.title}</h1>
            {canEdit && (
              <button type="button" onClick={() => setIsEditing(true)} className="text-sm text-primary">
                {t('editAction')}
              </button>
            )}
          </div>

          <div className="rounded-md bg-surface-muted p-4">
            <p className="text-sm text-text-muted">
              {t('columnStatus')}: <span className="font-medium text-text">{t(`status.${ad.status}`)}</span>
            </p>
            {ad.rejectionReason && (
              <p className="mt-1 text-sm text-danger">
                {t('rejectionReasonLabel')}: {ad.rejectionReason}
              </p>
            )}
          </div>

          <section className="grid grid-cols-2 gap-4 text-sm">
            {ad.description && (
              <div className="col-span-2">
                <p className="text-text-muted">{t('descriptionLabel')}</p>
                <p className="whitespace-pre-line text-text">{ad.description}</p>
              </div>
            )}
            <div>
              <p className="text-text-muted">{t('cityLabel')}</p>
              <p className="text-text">{ad.city ?? '—'}</p>
            </div>
            <div>
              <p className="text-text-muted">{t('categoryLabel')}</p>
              <p className="text-text">{ad.category ?? '—'}</p>
            </div>
            <div>
              <p className="text-text-muted">{t('vacancyCountLabel')}</p>
              <p className="text-text">{ad.vacancyCount}</p>
            </div>
          </section>

          {ad.payment && (
            <section className="rounded-md border border-surface-muted p-4">
              <h2 className="mb-2 font-semibold text-text">{t('paymentSection')}</h2>
              <p className="text-sm text-text">
                {t('paymentAmount')}: {ad.payment.amount} {ad.payment.currency}
              </p>
              <p className="text-sm text-text">
                {t('paymentStatus')}: {ad.payment.paymentStatus}
              </p>
              {ad.payment.paidAt && (
                <p className="text-sm text-text-muted">
                  {t('paymentPaidAt')}: {new Date(ad.payment.paidAt).toLocaleDateString('ar')}
                </p>
              )}
            </section>
          )}

          <div className="flex flex-wrap gap-2">
            {canSubmit && (
              <button
                type="button"
                disabled={submit.isPending}
                onClick={() => submit.mutate()}
                className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
              >
                {t('submitForReview')}
              </button>
            )}
            {canClose && (
              <button
                type="button"
                disabled={close.isPending}
                onClick={() => {
                  if (window.confirm(t('closeConfirm'))) close.mutate()
                }}
                className="rounded-md border border-danger px-4 py-2 text-sm font-medium text-danger disabled:opacity-60"
              >
                {t('closeAction')}
              </button>
            )}
            {canRenew && (
              <button
                type="button"
                disabled={renew.isPending}
                onClick={() =>
                  renew.mutate(undefined, {
                    onSuccess: (renewed) => {
                      void navigate(`/employer/job-advertisements/${renewed.id}`)
                    },
                  })
                }
                className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
              >
                {t('renewAction')}
              </button>
            )}
            <Link
              to={`/employer/job-advertisements/${ad.id}/applicants`}
              className="rounded-md border border-surface-muted px-4 py-2 text-sm font-medium text-text"
            >
              {t('applicantsAction')}
            </Link>
          </div>

          {(errorMessage(submit.error) || errorMessage(close.error) || errorMessage(renew.error)) && (
            <p className="text-sm text-danger">
              {errorMessage(submit.error) ?? errorMessage(close.error) ?? errorMessage(renew.error)}
            </p>
          )}
        </>
      )}
    </div>
  )
}
