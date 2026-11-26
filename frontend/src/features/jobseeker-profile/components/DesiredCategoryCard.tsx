import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useJobCategories } from '@/features/job-categories'
import { useUpdateDesiredCategory } from '../api/jobSeekerProfile.queries'
import type { JobSeekerProfile } from '../types/jobSeekerProfile.types'
import { Card } from '@/shared/ui/Card'
import { Select } from '@/shared/ui/Select'
import { Button } from '@/shared/ui/Button'

/** Kept out of ProfileCompletionForm on purpose — a notification preference, not reviewable
 * profile content, saved through its own endpoint that never reverts an Approved office
 * application (see UpdateDesiredCategoryDto's doc comment). */
export function DesiredCategoryCard({ profile }: { profile: JobSeekerProfile }) {
  const { t } = useTranslation('jobSeekerProfile')
  const { data: categories } = useJobCategories()
  const updateDesiredCategory = useUpdateDesiredCategory()
  const [value, setValue] = useState(profile.desiredCategoryId ?? '')

  const isDirty = value !== (profile.desiredCategoryId ?? '')

  return (
    <Card className="flex flex-col gap-3">
      <div>
        <p className="font-bold text-content-primary">{t('desiredCategoryTitle')}</p>
        <p className="mt-0.5 text-xs text-content-tertiary">{t('desiredCategoryHint')}</p>
      </div>

      <div className="flex flex-wrap items-end gap-2">
        <div className="min-w-48 flex-1">
          <Select id="desiredCategory" value={value} onChange={(e) => setValue(e.target.value)}>
            <option value="">{t('desiredCategoryPlaceholder')}</option>
            {(categories ?? []).map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>
        </div>
        <Button
          size="sm"
          disabled={!isDirty || updateDesiredCategory.isPending}
          onClick={() => updateDesiredCategory.mutate({ desiredCategoryId: value || undefined })}
        >
          {t('save')}
        </Button>
      </div>
    </Card>
  )
}
