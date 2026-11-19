import { useTranslation } from 'react-i18next'
import { GRANTABLE_PERMISSION_CODES } from '../types/officeUser.types'

export function PermissionCheckboxList({
  selected,
  onChange,
}: {
  selected: string[]
  onChange: (next: string[]) => void
}) {
  const { t } = useTranslation('officeUsers')

  function toggle(code: string) {
    onChange(selected.includes(code) ? selected.filter((c) => c !== code) : [...selected, code])
  }

  return (
    <div className="grid grid-cols-1 gap-2 rounded-md bg-surface-muted p-3 sm:grid-cols-2">
      {GRANTABLE_PERMISSION_CODES.map((code) => (
        <label key={code} className="flex items-center gap-2 text-sm text-text">
          <input
            type="checkbox"
            checked={selected.includes(code)}
            onChange={() => toggle(code)}
            className="size-4 accent-primary"
          />
          {t(`permission.${code}`)}
        </label>
      ))}
    </div>
  )
}
