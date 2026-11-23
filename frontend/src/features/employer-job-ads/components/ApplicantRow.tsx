import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { httpClient } from '@/shared/api/httpClient'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { Select } from '@/shared/ui/Select'
import { GatedField } from '@/shared/ui/GatedField'
import type { ApplicantListItem, EmployerFacingStatusCode } from '../types/employerJobAds.types'

const OUTCOMES: EmployerFacingStatusCode[] = [
  'NotYetContacted',
  'ContactedInterviewScheduled',
  'ContactedRefused',
  'NoResponseAfterContact',
]

export function ApplicantRow({
  applicant,
  isUpdating,
  onUpdateOutcome,
}: {
  applicant: ApplicantListItem
  isUpdating: boolean
  onUpdateOutcome: (outcome: EmployerFacingStatusCode) => void
}) {
  const { t } = useTranslation('employerJobAds')
  const [downloading, setDownloading] = useState(false)
  const isGated = applicant.cvDownloadUrl !== undefined && applicant.cvDownloadUrl !== null

  async function handleDownloadCv() {
    if (!applicant.cvDownloadUrl) return
    setDownloading(true)
    try {
      const { blob, fileName } = await httpClient.getBlob(applicant.cvDownloadUrl)
      const url = URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = fileName
      link.click()
      URL.revokeObjectURL(url)
    } finally {
      setDownloading(false)
    }
  }

  return (
    <Card className="flex flex-col gap-4">
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="font-semibold text-text">{applicant.jobSeekerName}</p>
          <p className="text-xs text-text-muted">
            {new Date(applicant.appliedAt).toLocaleDateString('ar')}
          </p>
        </div>
        {applicant.withdrawnAt && (
          <span className="rounded-full bg-neutral/10 px-2.5 py-1 text-xs font-medium text-neutral">
            {t('withdrawn')}
          </span>
        )}
      </div>

      <div className="flex flex-wrap items-end gap-4">
        <div className="flex flex-col gap-1">
          <span className="text-xs font-medium text-text">{t('columnContact')}</span>
          {isGated ? (
            <div className="flex flex-col gap-0.5 text-sm text-text">
              {applicant.phone && <span>{applicant.phone}</span>}
              {applicant.email && <span>{applicant.email}</span>}
            </div>
          ) : (
            <GatedField reason={t('dataLockedUntilPayment')} />
          )}
        </div>

        <Select
          label={t('columnOutcome')}
          value={applicant.employerFacingStatus}
          disabled={isUpdating}
          onChange={(e) => onUpdateOutcome(e.target.value as EmployerFacingStatusCode)}
          className="min-w-[200px]"
        >
          {OUTCOMES.map((o) => (
            <option key={o} value={o}>
              {t(`outcome.${o}`)}
            </option>
          ))}
        </Select>
      </div>

      <div className="flex flex-wrap items-center gap-3 border-t border-surface-muted pt-3">
        {isGated && (
          <Button
            variant="secondary"
            disabled={downloading}
            onClick={() => void handleDownloadCv()}
          >
            {t('downloadCv')}
          </Button>
        )}
        <Link to={`/employer/applications/${applicant.applicationId}`}>
          <Button variant="ghost">{t('interviewAndReports')}</Button>
        </Link>
      </div>
    </Card>
  )
}
