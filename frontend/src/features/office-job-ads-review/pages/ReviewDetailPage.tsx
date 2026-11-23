import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useParams } from 'react-router-dom'
import { ApiError } from '@/shared/api/types'
import { useAccess } from '@/shared/auth/useAccess'
import { PaymentConfirmForm } from '../components/PaymentConfirmForm'
import { ReasonForm } from '../components/ReasonForm'
import {
  useApproveJobAd,
  useConfirmPayment,
  useOfficeJobAdDetail,
  usePauseJobAd,
  useRejectJobAd,
  useResumeJobAd,
} from '../api/officeJobAdsReview.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

export default function ReviewDetailPage() {
  const { t } = useTranslation('officeJobAdsReview')
  const { hasPermission } = useAccess()
  const { id } = useParams<{ id: string }>()
  const adId = id ?? ''
  const [showReject, setShowReject] = useState(false)
  const [showPayment, setShowPayment] = useState(false)

  const { data: ad, isLoading } = useOfficeJobAdDetail(adId)
  const approve = useApproveJobAd(adId)
  const reject = useRejectJobAd(adId)
  const pause = usePauseJobAd(adId)
  const resume = useResumeJobAd(adId)
  const confirmPayment = useConfirmPayment(adId)

  if (isLoading || !ad) {
    return <LoadingState />
  }

  const canDecide = ad.status === 'UnderReview' && hasPermission('jobadvertisements.approve')
  const canConfirmPayment =
    ad.status === 'ApprovedAwaitingPayment' && hasPermission('advertisementpayments.record')
  const canPause = ad.status === 'Published' && hasPermission('jobadvertisements.pause')
  const canResume = ad.status === 'Paused' && hasPermission('jobadvertisements.pause')

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8 sm:px-6">
      <Link
        to="/office/job-advertisements"
        className="text-sm font-medium text-primary hover:underline"
      >
        {t('backToQueue')}
      </Link>

      <Card className="flex flex-col gap-5">
        <PageHeader title={ad.title} />
        <p className="text-sm text-text-muted">{ad.employerDisplayName}</p>

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
              <p className="text-text">{ad.description}</p>
            </div>
          )}
          <div>
            <p className="text-text-muted">{t('columnCity')}</p>
            <p className="text-text">{ad.city ?? '—'}</p>
          </div>
          <div>
            <p className="text-text-muted">{t('columnSubmittedAt')}</p>
            <p className="text-text">
              {ad.submittedAt ? new Date(ad.submittedAt).toLocaleDateString('ar') : '—'}
            </p>
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
          </section>
        )}

        {canDecide && (
          <section className="flex flex-col gap-2 border-t border-surface-muted pt-5">
            {!showReject ? (
              <div className="flex gap-3">
                <Button
                  variant="success"
                  disabled={approve.isPending}
                  onClick={() => approve.mutate()}
                >
                  {t('approveAction')}
                </Button>
                <Button variant="danger" onClick={() => setShowReject(true)}>
                  {t('rejectAction')}
                </Button>
              </div>
            ) : (
              <ReasonForm
                confirmLabel={t('rejectAction')}
                isSubmitting={reject.isPending}
                error={errorMessage(reject.error)}
                onConfirm={(reason) => {
                  reject.mutate(reason)
                  setShowReject(false)
                }}
                onCancel={() => setShowReject(false)}
              />
            )}
            {errorMessage(approve.error) && (
              <p className="text-sm text-danger">{errorMessage(approve.error)}</p>
            )}
          </section>
        )}

        {canConfirmPayment && (
          <section className="flex flex-col gap-2 border-t border-surface-muted pt-5">
            {!showPayment ? (
              <Button variant="primary" className="self-start" onClick={() => setShowPayment(true)}>
                {t('confirmPaymentAction')}
              </Button>
            ) : (
              <PaymentConfirmForm
                isSubmitting={confirmPayment.isPending}
                error={errorMessage(confirmPayment.error)}
                onConfirm={(amount, currency) => {
                  confirmPayment.mutate({ amount, currency })
                  setShowPayment(false)
                }}
                onCancel={() => setShowPayment(false)}
              />
            )}
          </section>
        )}

        <div className="flex flex-wrap gap-3 border-t border-surface-muted pt-5">
          {canPause && (
            <Button variant="danger" disabled={pause.isPending} onClick={() => pause.mutate()}>
              {t('pauseAction')}
            </Button>
          )}
          {canResume && (
            <Button variant="primary" disabled={resume.isPending} onClick={() => resume.mutate()}>
              {t('resumeAction')}
            </Button>
          )}
          <Link to={`/office/job-advertisements/${ad.id}/applicants`}>
            <Button variant="secondary">{t('applicantsAction')}</Button>
          </Link>
        </div>
      </Card>
    </div>
  )
}
