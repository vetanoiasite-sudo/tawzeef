import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { jobSeekerProfileApi } from '../api/jobSeekerProfile.api'
import type { JobSeekerProfile } from '../types/jobSeekerProfile.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'

/** The read-only "CV-style" view every edit used to skip straight past (confirmed 2026-11-25
 * user report — there was no profile page, only the edit form). This is what opens by default;
 * "تعديل" is the one way into the actual edit form. */
export function ProfileSummaryCard({
  profile,
  onEdit,
}: {
  profile: JobSeekerProfile
  onEdit: () => void
}) {
  const { t } = useTranslation('jobSeekerProfile')
  const [photoUrl, setPhotoUrl] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    let objectUrl: string | null = null

    if (profile.hasPhoto) {
      void jobSeekerProfileApi.getPhoto().then(({ blob }) => {
        if (cancelled) return
        objectUrl = URL.createObjectURL(blob)
        setPhotoUrl(objectUrl)
      })
    } else {
      // Deferred rather than called synchronously at the top of the effect, so this never
      // trips react-hooks/set-state-in-effect — the "no photo" case still needs to clear a
      // stale URL left over from a previous hasPhoto=true render.
      void Promise.resolve().then(() => {
        if (!cancelled) setPhotoUrl(null)
      })
    }

    return () => {
      cancelled = true
      if (objectUrl) URL.revokeObjectURL(objectUrl)
    }
  }, [profile.hasPhoto])

  const infoRows: { label: string; value: string | null }[] = [
    { label: t('emailLabel'), value: profile.email },
    { label: t('cityLabel'), value: profile.city },
    { label: t('addressLabel'), value: profile.address },
    { label: t('dateOfBirthLabel'), value: profile.dateOfBirth },
    {
      label: t('yearsOfExperienceLabel'),
      value: profile.yearsOfExperience != null ? String(profile.yearsOfExperience) : null,
    },
    { label: t('educationLabel'), value: profile.education },
  ]

  return (
    <Card className="flex flex-col gap-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="flex items-center gap-4">
          <div className="flex size-20 shrink-0 items-center justify-center overflow-hidden rounded-full border border-border-primary bg-surface-muted">
            {photoUrl ? (
              <img src={photoUrl} alt="" className="size-full object-cover" />
            ) : (
              <PersonIcon />
            )}
          </div>
          <div>
            <h1 className="text-xl font-bold text-content-primary">{profile.fullName}</h1>
            {profile.jobTitle && (
              <p className="mt-0.5 text-sm font-semibold text-brand">{profile.jobTitle}</p>
            )}
          </div>
        </div>
        <Button variant="secondary" onClick={onEdit}>
          {t('editProfile')}
        </Button>
      </div>

      {profile.careerSummary && (
        <section className="flex flex-col gap-1.5">
          <h2 className="text-xs font-bold tracking-wide text-content-tertiary uppercase">
            {t('careerSummaryLabel')}
          </h2>
          <p className="text-sm whitespace-pre-line text-content-primary">{profile.careerSummary}</p>
        </section>
      )}

      {profile.skills && (
        <section className="flex flex-col gap-1.5">
          <h2 className="text-xs font-bold tracking-wide text-content-tertiary uppercase">
            {t('skillsLabel')}
          </h2>
          <p className="text-sm whitespace-pre-line text-content-primary">{profile.skills}</p>
        </section>
      )}

      <div className="grid grid-cols-1 gap-x-6 gap-y-3 border-t border-border-primary pt-4 sm:grid-cols-2">
        {infoRows.map(
          (row) =>
            row.value && (
              <div key={row.label} className="flex flex-col gap-0.5">
                <span className="text-xs font-medium text-content-tertiary">{row.label}</span>
                <span className="text-sm text-content-primary">{row.value}</span>
              </div>
            ),
        )}
      </div>
    </Card>
  )
}

function PersonIcon() {
  return (
    <svg width="32" height="32" viewBox="0 0 24 24" fill="none" aria-hidden className="text-content-tertiary">
      <circle cx="12" cy="8" r="4" stroke="currentColor" strokeWidth="1.8" />
      <path d="M4 20c0-3.3 3.6-6 8-6s8 2.7 8 6" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  )
}
