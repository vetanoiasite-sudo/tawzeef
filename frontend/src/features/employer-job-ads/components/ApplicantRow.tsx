import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { httpClient } from '@/shared/api/httpClient'
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
    <tr className="border-t border-surface-muted">
      <td className="px-3 py-2 text-text">{applicant.jobSeekerName}</td>
      <td className="px-3 py-2 text-text-muted">{new Date(applicant.appliedAt).toLocaleDateString('ar')}</td>
      <td className="px-3 py-2 text-text">{applicant.withdrawnAt ? t('withdrawn') : '—'}</td>
      <td className="px-3 py-2">
        {isGated ? (
          <div className="flex flex-col gap-0.5 text-sm text-text">
            {applicant.phone && <span>{applicant.phone}</span>}
            {applicant.email && <span>{applicant.email}</span>}
          </div>
        ) : (
          <span className="text-xs text-text-muted">{t('dataLockedUntilPayment')}</span>
        )}
      </td>
      <td className="px-3 py-2">
        <select
          value={applicant.employerFacingStatus}
          disabled={isUpdating}
          onChange={(e) => onUpdateOutcome(e.target.value as EmployerFacingStatusCode)}
          className="rounded-md border border-surface-muted px-2 py-1 text-sm text-text"
        >
          {OUTCOMES.map((o) => (
            <option key={o} value={o}>
              {t(`outcome.${o}`)}
            </option>
          ))}
        </select>
      </td>
      <td className="px-3 py-2">
        {isGated && (
          <button
            type="button"
            disabled={downloading}
            onClick={() => void handleDownloadCv()}
            className="text-sm text-primary disabled:opacity-60"
          >
            {t('downloadCv')}
          </button>
        )}
      </td>
      <td className="px-3 py-2">
        <Link to={`/employer/applications/${applicant.applicationId}`} className="text-sm text-primary">
          {t('interviewAndReports')}
        </Link>
      </td>
    </tr>
  )
}
