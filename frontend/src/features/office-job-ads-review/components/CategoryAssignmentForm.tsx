import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useJobCategories } from '@/features/job-categories'
import { Select } from '@/shared/ui/Select'
import { TextInput } from '@/shared/ui/TextInput'
import { Button } from '@/shared/ui/Button'

const NEW_CATEGORY_VALUE = '__new__'

/** Office-only — the employer never picks the final category, only suggests one as free text at
 * creation (shown here for reference). Picking "+ تخصص جديد" from the dropdown reveals a text
 * input to name the new category inline — IJobAdvertisementReviewService.AssignCategoryAsync's
 * doc comment. */
export function CategoryAssignmentForm({
  employerSuggestion,
  currentCategoryId,
  currentCategoryName,
  isSubmitting,
  error,
  onAssign,
}: {
  employerSuggestion: string | null
  currentCategoryId: string | null
  currentCategoryName: string | null
  isSubmitting: boolean
  error?: string
  onAssign: (vars: { existingCategoryId?: string; newCategoryName?: string }) => void
}) {
  const { t } = useTranslation('officeJobAdsReview')
  const { data: categories, isLoading } = useJobCategories()
  const [selected, setSelected] = useState(currentCategoryId ?? '')
  const [newName, setNewName] = useState('')

  function handleSave() {
    if (selected === NEW_CATEGORY_VALUE) {
      if (!newName.trim()) return
      onAssign({ newCategoryName: newName.trim() })
    } else if (selected) {
      onAssign({ existingCategoryId: selected })
    }
  }

  return (
    <div className="flex flex-col gap-3 rounded-md border border-surface-muted p-4">
      <h2 className="font-semibold text-text">{t('categorySectionTitle')}</h2>

      {employerSuggestion && (
        <p className="text-sm text-text-muted">
          {t('employerSuggestedCategoryLabel')}: <span className="text-text">{employerSuggestion}</span>
        </p>
      )}

      <p className="text-sm text-text-muted">
        {t('assignedCategoryLabel')}:{' '}
        <span className="text-text">{currentCategoryName ?? t('noCategoryAssigned')}</span>
      </p>

      <Select
        label={t('existingCategoryLabel')}
        value={selected}
        onChange={(e) => setSelected(e.target.value)}
        disabled={isLoading}
      >
        <option value="">—</option>
        {categories?.map((c) => (
          <option key={c.id} value={c.id}>
            {c.name}
          </option>
        ))}
        <option value={NEW_CATEGORY_VALUE}>{t('newCategoryOption')}</option>
      </Select>

      {selected === NEW_CATEGORY_VALUE && (
        <TextInput
          label={t('newCategoryLabel')}
          value={newName}
          onChange={(e) => setNewName(e.target.value)}
        />
      )}

      {error && <p className="text-sm text-danger">{error}</p>}

      <Button
        variant="primary"
        size="sm"
        className="self-start"
        disabled={isSubmitting || !selected || (selected === NEW_CATEGORY_VALUE && !newName.trim())}
        onClick={handleSave}
      >
        {t('assignCategoryAction')}
      </Button>
    </div>
  )
}
