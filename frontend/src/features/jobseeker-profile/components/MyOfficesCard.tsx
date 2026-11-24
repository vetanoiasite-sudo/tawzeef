import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { useMyJobSeekerOffices, useSubmitJobSeekerOffices } from '../api/jobSeekerProfile.queries'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { StatusTag } from '@/shared/ui/StatusTag'
import { LoadingState } from '@/shared/ui/LoadingState'
import { OfficeMultiSelect, type OfficeMultiSelectValue } from '@/shared/ui/OfficeMultiSelect'

function errorMessage(error: unknown): string | undefined {
  return error instanceof ApiError ? error.messages[0] : undefined
}

/** Confirmed 2026-11-24: a job seeker may submit to several offices (or all of them), each
 * reviewing independently — replaces the old single status badge. */
export function MyOfficesCard() {
  const { t } = useTranslation(['jobSeekerProfile', 'validation'])
  const { data: offices, isLoading } = useMyJobSeekerOffices()
  const submitToOffices = useSubmitJobSeekerOffices()
  const [showAddMore, setShowAddMore] = useState(false)
  const [selection, setSelection] = useState<OfficeMultiSelectValue>({
    officeIds: [],
    allOffices: false,
  })

  if (isLoading || !offices) return <LoadingState />

  const appliedOfficeIds = offices
    .filter((o) => o.reviewStatus !== 'NotSubmitted')
    .map((o) => o.officeId)
  const hasMoreOffices = offices.some((o) => o.reviewStatus === 'NotSubmitted')
  const canSubmit = selection.allOffices || selection.officeIds.length > 0

  function handleSubmit() {
    submitToOffices.mutate(selection, {
      onSuccess: () => {
        setShowAddMore(false)
        setSelection({ officeIds: [], allOffices: false })
      },
    })
  }

  return (
    <Card className="flex flex-col gap-3">
      <p className="font-bold text-content-primary">
        {t('jobSeekerProfile:myOfficesTitle', { defaultValue: 'مكاتب التوظيف' })}
      </p>

      <div className="flex flex-col divide-y divide-border-primary/60">
        {offices.map((office) => (
          <div key={office.officeId} className="flex flex-col gap-1 py-2.5 first:pt-0 last:pb-0">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div>
                <p className="text-sm font-medium text-content-primary">{office.officeName}</p>
                {office.city && <p className="text-xs text-content-tertiary">{office.city}</p>}
              </div>
              <StatusTag
                domain="jobSeekerReview"
                value={office.reviewStatus}
                label={
                  office.reviewStatus === 'NotSubmitted'
                    ? t('jobSeekerProfile:officeNotSubmitted', { defaultValue: 'لم يتقدم' })
                    : t(`jobSeekerProfile:status.${office.reviewStatus}`)
                }
              />
            </div>
            {office.rejectionReason && (
              <p className="text-xs text-danger">
                {t('jobSeekerProfile:rejectionReasonLabel')}: {office.rejectionReason}
              </p>
            )}
            {office.suspensionReason && (
              <p className="text-xs text-danger">
                {t('jobSeekerProfile:suspensionReasonLabel')}: {office.suspensionReason}
              </p>
            )}
          </div>
        ))}
      </div>

      {hasMoreOffices &&
        (showAddMore ? (
          <div className="flex flex-col gap-2 border-t border-border-primary pt-3">
            <OfficeMultiSelect
              value={selection}
              onChange={setSelection}
              excludeOfficeIds={appliedOfficeIds}
            />
            {errorMessage(submitToOffices.error) && (
              <p className="text-sm text-danger">{errorMessage(submitToOffices.error)}</p>
            )}
            <div className="flex gap-2">
              <Button size="sm" disabled={!canSubmit || submitToOffices.isPending} onClick={handleSubmit}>
                {t('jobSeekerProfile:submitToMoreOfficesAction', { defaultValue: 'إرسال' })}
              </Button>
              <Button size="sm" variant="secondary" onClick={() => setShowAddMore(false)}>
                {t('validation:cancel', { defaultValue: 'إلغاء' })}
              </Button>
            </div>
          </div>
        ) : (
          <Button
            size="sm"
            variant="secondary"
            className="self-start"
            onClick={() => setShowAddMore(true)}
          >
            {t('jobSeekerProfile:addMoreOfficesAction', { defaultValue: 'التقديم لمكتب آخر' })}
          </Button>
        ))}
    </Card>
  )
}
