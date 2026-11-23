import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { OfficeJobAdDetail, UpdateJobAdPayload } from '../types/officeJobAdsReview.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput, TextArea } from '@/shared/ui/TextInput'
import { Select } from '@/shared/ui/Select'
import { EGYPT_GOVERNORATES } from '@/shared/constants/governorates'

/** Office-side correction of a job ad's fields — the employer's own JobAdForm (a different
 * feature folder) isn't reused here to avoid coupling the two features' types together; this is
 * a small, local equivalent, plain useState like super-admin/OfficeForm.tsx's edit-in-place
 * pattern rather than react-hook-form+zod, since this is a quick-fix tool, not a guided form. */
export function OfficeJobAdEditForm({
  ad,
  isSubmitting,
  error,
  onSubmit,
  onCancel,
}: {
  ad: OfficeJobAdDetail
  isSubmitting: boolean
  error?: string
  onSubmit: (payload: UpdateJobAdPayload) => void
  onCancel: () => void
}) {
  const { t } = useTranslation('officeJobAdsReview')
  const [title, setTitle] = useState(ad.title)
  const [description, setDescription] = useState(ad.description ?? '')
  const [experienceRequirement, setExperienceRequirement] = useState(ad.experienceRequirement ?? '')
  const [workingHours, setWorkingHours] = useState(ad.workingHours ?? '')
  const [address, setAddress] = useState(ad.address ?? '')
  const [category, setCategory] = useState(ad.category ?? '')
  const [city, setCity] = useState(ad.city ?? '')
  const [employmentType, setEmploymentType] = useState(ad.employmentType ?? '')
  const [salaryMin, setSalaryMin] = useState(ad.salaryMin?.toString() ?? '')
  const [salaryMax, setSalaryMax] = useState(ad.salaryMax?.toString() ?? '')
  const [vacancyCount, setVacancyCount] = useState(ad.vacancyCount.toString())

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    onSubmit({
      title: title.trim(),
      description: description.trim() || undefined,
      experienceRequirement: experienceRequirement.trim() || undefined,
      workingHours: workingHours.trim() || undefined,
      address: address.trim() || undefined,
      category: category.trim() || undefined,
      city: city || undefined,
      employmentType: employmentType || undefined,
      salaryMin: salaryMin ? Number(salaryMin) : undefined,
      salaryMax: salaryMax ? Number(salaryMax) : undefined,
      vacancyCount: vacancyCount ? Number(vacancyCount) : 1,
    })
  }

  return (
    <form onSubmit={handleSubmit}>
      <Card className="flex flex-col gap-4">
        <p className="text-sm text-content-secondary">{t('editAdvertisementHint')}</p>

        <TextInput id="editTitle" label={t('titleLabel')} value={title} onChange={(e) => setTitle(e.target.value)} />

        <TextArea
          id="editDescription"
          label={t('descriptionLabel')}
          rows={4}
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <TextInput
            id="editCategory"
            label={t('categoryLabel')}
            value={category}
            onChange={(e) => setCategory(e.target.value)}
          />

          <Select id="editCity" label={t('columnCity')} value={city} onChange={(e) => setCity(e.target.value)}>
            <option value="">—</option>
            {EGYPT_GOVERNORATES.map((g) => (
              <option key={g} value={g}>
                {g}
              </option>
            ))}
          </Select>

          <Select
            id="editEmploymentType"
            label={t('employmentTypeLabel')}
            value={employmentType}
            onChange={(e) => setEmploymentType(e.target.value)}
          >
            <option value="">—</option>
            <option value="FullTime">{t('employmentType.FullTime')}</option>
            <option value="PartTime">{t('employmentType.PartTime')}</option>
            <option value="Contract">{t('employmentType.Contract')}</option>
          </Select>

          <TextInput
            id="editExperienceRequirement"
            label={t('experienceRequirementLabel')}
            value={experienceRequirement}
            onChange={(e) => setExperienceRequirement(e.target.value)}
          />

          <TextInput
            id="editWorkingHours"
            label={t('workingHoursLabel')}
            value={workingHours}
            onChange={(e) => setWorkingHours(e.target.value)}
          />

          <TextInput
            id="editAddress"
            label={t('addressLabel')}
            value={address}
            onChange={(e) => setAddress(e.target.value)}
          />

          <TextInput
            id="editSalaryMin"
            type="number"
            label={t('salaryMinLabel')}
            value={salaryMin}
            onChange={(e) => setSalaryMin(e.target.value)}
          />

          <TextInput
            id="editSalaryMax"
            type="number"
            label={t('salaryMaxLabel')}
            value={salaryMax}
            onChange={(e) => setSalaryMax(e.target.value)}
          />

          <TextInput
            id="editVacancyCount"
            type="number"
            min={1}
            label={t('vacancyCountLabel')}
            value={vacancyCount}
            onChange={(e) => setVacancyCount(e.target.value)}
          />
        </div>

        {error && <p className="text-sm text-danger">{error}</p>}

        <div className="flex gap-2">
          <Button type="submit" variant="primary" disabled={isSubmitting || !title.trim()}>
            {t('saveChanges')}
          </Button>
          <Button type="button" variant="secondary" onClick={onCancel}>
            {t('cancel')}
          </Button>
        </div>
      </Card>
    </form>
  )
}
