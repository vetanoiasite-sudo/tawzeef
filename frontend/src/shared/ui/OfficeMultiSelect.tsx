import { useTranslation } from 'react-i18next'
import { usePublicOffices } from '@/features/auth'

export type OfficeMultiSelectValue = {
  officeIds: string[]
  allOffices: boolean
}

/**
 * Confirmed 2026-11-24: a job seeker/employer may submit to several offices at once, or every
 * office that exists. Shared by registration (DetailsStep) and the "My Offices" submit-more flow
 * on both self-service profile pages.
 */
export function OfficeMultiSelect({
  value,
  onChange,
  excludeOfficeIds,
  error,
}: {
  value: OfficeMultiSelectValue
  onChange: (next: OfficeMultiSelectValue) => void
  /** Offices already applied to — hidden from the list so only new choices are offered. */
  excludeOfficeIds?: string[]
  error?: string
}) {
  const { t } = useTranslation('validation')
  const { data: offices, isLoading } = usePublicOffices()
  const excluded = new Set(excludeOfficeIds ?? [])
  const selectable = (offices ?? []).filter((office) => !excluded.has(office.id))

  function toggleAll(checked: boolean) {
    onChange({ officeIds: checked ? [] : value.officeIds, allOffices: checked })
  }

  function toggleOffice(officeId: string, checked: boolean) {
    const next = checked
      ? [...value.officeIds, officeId]
      : value.officeIds.filter((id) => id !== officeId)
    onChange({ officeIds: next, allOffices: value.allOffices })
  }

  return (
    <div className="flex flex-col gap-2">
      <label className="flex cursor-pointer items-center gap-2 rounded-200 border border-border-primary bg-surface px-3 py-2.5 text-[14px] font-bold text-content-primary">
        <input
          type="checkbox"
          checked={value.allOffices}
          onChange={(e) => toggleAll(e.target.checked)}
          className="size-4 accent-brand"
        />
        {t('officeMultiSelect.allOffices', { defaultValue: 'اختر كل المكاتب' })}
      </label>

      {isLoading ? (
        <p className="text-[13px] text-content-tertiary">
          {t('officeMultiSelect.loading', { defaultValue: 'جارٍ تحميل المكاتب...' })}
        </p>
      ) : (
        <div className="flex max-h-56 flex-col gap-1 overflow-y-auto rounded-200 border border-border-primary p-2">
          {selectable.length === 0 && (
            <p className="px-1 py-1 text-[13px] text-content-tertiary">
              {t('officeMultiSelect.none', { defaultValue: 'لا يوجد مكاتب متاحة.' })}
            </p>
          )}
          {selectable.map((office) => (
            <label
              key={office.id}
              className="flex cursor-pointer items-center gap-2 rounded-100 px-2 py-1.5 text-[14px] text-content-primary hover:bg-surface-hover"
            >
              <input
                type="checkbox"
                disabled={value.allOffices}
                checked={value.allOffices || value.officeIds.includes(office.id)}
                onChange={(e) => toggleOffice(office.id, e.target.checked)}
                className="size-4 accent-brand disabled:opacity-50"
              />
              <span>{office.city ? `${office.name} — ${office.city}` : office.name}</span>
            </label>
          ))}
        </div>
      )}

      {error && <p className="text-[12px] font-medium text-danger">{error}</p>}
    </div>
  )
}
