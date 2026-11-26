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
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

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
    return <LoadingState />
  }

  const canEdit = ad.status === 'Draft' || ad.status === 'Rejected' || ad.status === 'Published'
  const canSubmit = ad.status === 'Draft'
  const canClose = ad.status === 'Published'
  const canRenew = ad.status === 'Expired'

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link
        to="/employer/job-advertisements"
        className="text-sm font-medium text-primary hover:underline"
      >
        {t('backToList')}
      </Link>

      {isEditing ? (
        <Card className="flex flex-col gap-5">
          <PageHeader title={ad.title} />
          <JobAdForm
            ad={ad}
            isSubmitting={update.isPending}
            submitError={errorMessage(update.error)}
            onSubmit={(values) => update.mutate(values, { onSuccess: () => setIsEditing(false) })}
          />
          <Button variant="ghost" onClick={() => setIsEditing(false)} className="self-start">
            {t('cancelAction')}
          </Button>
        </Card>
      ) : (
        <Card className="flex flex-col gap-5">
          <PageHeader
            title={ad.title}
            action={
              canEdit && (
                <Button variant="secondary" onClick={() => setIsEditing(true)}>
                  {t('editAction')}
                </Button>
              )
            }
          />

          <div className="flex flex-col gap-2 rounded-md bg-surface-muted p-4">
            <StatusTag domain="advertisement" value={ad.status} label={t(`status.${ad.status}`)} />
            {ad.rejectionReason && (
              <p className="text-sm text-danger">
                {t('rejectionReasonLabel')}: {ad.rejectionReason}
              </p>
            )}
          </div>

          <section className="grid grid-cols-1 gap-4 border-t border-surface-muted pt-5 text-sm sm:grid-cols-2">
            {ad.description && (
              <div className="col-span-1 sm:col-span-2">
                <p className="text-text-muted">{t('descriptionLabel')}</p>
                <p className="whitespace-pre-line text-text">{ad.description}</p>
              </div>
            )}
            <div>
              <p className="text-text-muted">
                {t('officeLabel', { defaultValue: 'مكتب التوظيف' })}
              </p>
              <p className="text-text">{ad.officeName ?? '—'}</p>
            </div>
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
            <section className="flex flex-col gap-1 rounded-md border border-surface-muted p-4">
              <h2 className="mb-1 font-semibold text-text">{t('paymentSection')}</h2>
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
              <p className="text-sm text-text">
                {t('cvQuotaLabel')}: {ad.cvViewsUsed}
                {ad.cvViewLimit !== null ? ` / ${ad.cvViewLimit}` : ` (${t('cvQuotaUnlimited')})`}
              </p>
            </section>
          )}

          <div className="flex flex-wrap gap-3 border-t border-surface-muted pt-5">
            {canSubmit && (
              <Button variant="primary" disabled={submit.isPending} onClick={() => submit.mutate()}>
                {t('submitForReview')}
              </Button>
            )}
            {canClose && (
              <Button
                variant="danger"
                disabled={close.isPending}
                onClick={() => {
                  if (window.confirm(t('closeConfirm'))) close.mutate()
                }}
              >
                {t('closeAction')}
              </Button>
            )}
            {canRenew && (
              <Button
                variant="primary"
                disabled={renew.isPending}
                onClick={() =>
                  renew.mutate(undefined, {
                    onSuccess: (renewed) => {
                      void navigate(`/employer/job-advertisements/${renewed.id}`)
                    },
                  })
                }
              >
                {t('renewAction')}
              </Button>
            )}
            <Link to={`/employer/job-advertisements/${ad.id}/applicants`}>
              <Button variant="secondary">{t('applicantsAction')}</Button>
            </Link>
          </div>

          {(errorMessage(submit.error) ||
            errorMessage(close.error) ||
            errorMessage(renew.error)) && (
            <p className="text-sm text-danger">
              {errorMessage(submit.error) ?? errorMessage(close.error) ?? errorMessage(renew.error)}
            </p>
          )}
        </Card>
      )}
    </div>
  )
}
