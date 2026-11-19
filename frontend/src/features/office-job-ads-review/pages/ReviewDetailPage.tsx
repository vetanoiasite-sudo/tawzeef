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
    return <div className="mx-auto max-w-2xl px-4 py-10 text-text-muted">...</div>
  }

  const canDecide = ad.status === 'UnderReview' && hasPermission('jobadvertisements.approve')
  const canConfirmPayment = ad.status === 'ApprovedAwaitingPayment' && hasPermission('advertisementpayments.record')
  const canPause = ad.status === 'Published' && hasPermission('jobadvertisements.pause')
  const canResume = ad.status === 'Paused' && hasPermission('jobadvertisements.pause')

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-8 px-4 py-10">
      <Link to="/office/job-advertisements" className="text-sm text-primary">
        {t('backToQueue')}
      </Link>

      <div className="flex flex-col gap-1">
        <h1 className="text-lg font-semibold text-text">{ad.title}</h1>
        <p className="text-sm text-text-muted">{ad.employerDisplayName}</p>
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
            <p className="text-text-muted">{ad.description}</p>
          </div>
        )}
        <div>
          <p className="text-text-muted">{t('columnCity')}</p>
          <p className="text-text">{ad.city ?? '—'}</p>
        </div>
        <div>
          <p className="text-text-muted">{t('columnSubmittedAt')}</p>
          <p className="text-text">{ad.submittedAt ? new Date(ad.submittedAt).toLocaleDateString('ar') : '—'}</p>
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
        </section>
      )}

      {canDecide && (
        <section className="flex flex-col gap-2">
          {!showReject ? (
            <div className="flex gap-2">
              <button
                type="button"
                disabled={approve.isPending}
                onClick={() => approve.mutate()}
                className="rounded-md bg-success px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
              >
                {t('approveAction')}
              </button>
              <button
                type="button"
                onClick={() => setShowReject(true)}
                className="rounded-md border border-danger px-4 py-2 text-sm font-medium text-danger"
              >
                {t('rejectAction')}
              </button>
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
          {errorMessage(approve.error) && <p className="text-sm text-danger">{errorMessage(approve.error)}</p>}
        </section>
      )}

      {canConfirmPayment && (
        <section className="flex flex-col gap-2">
          {!showPayment ? (
            <button
              type="button"
              onClick={() => setShowPayment(true)}
              className="self-start rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface"
            >
              {t('confirmPaymentAction')}
            </button>
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

      <div className="flex flex-wrap gap-2">
        {canPause && (
          <button
            type="button"
            disabled={pause.isPending}
            onClick={() => pause.mutate()}
            className="rounded-md border border-danger px-4 py-2 text-sm font-medium text-danger disabled:opacity-60"
          >
            {t('pauseAction')}
          </button>
        )}
        {canResume && (
          <button
            type="button"
            disabled={resume.isPending}
            onClick={() => resume.mutate()}
            className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-surface disabled:opacity-60"
          >
            {t('resumeAction')}
          </button>
        )}
        <Link
          to={`/office/job-advertisements/${ad.id}/applicants`}
          className="rounded-md border border-surface-muted px-4 py-2 text-sm font-medium text-text"
        >
          {t('applicantsAction')}
        </Link>
      </div>
    </div>
  )
}
