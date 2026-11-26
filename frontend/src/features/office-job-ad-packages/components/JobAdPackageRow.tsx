import { useTranslation } from 'react-i18next'
import { JobAdPackageEditForm } from './JobAdPackageEditForm'
import type {
  JobAdvertisementPackage,
  JobAdvertisementPackagePayload,
} from '../types/officeJobAdPackages.types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'

export function JobAdPackageRow({
  pkg,
  editing,
  isSaving,
  saveError,
  isTogglingActive,
  onStartEdit,
  onCancelEdit,
  onSave,
  onToggleActive,
}: {
  pkg: JobAdvertisementPackage
  editing: boolean
  isSaving: boolean
  saveError?: string
  isTogglingActive: boolean
  onStartEdit: () => void
  onCancelEdit: () => void
  onSave: (payload: JobAdvertisementPackagePayload) => void
  onToggleActive: (activate: boolean) => void
}) {
  const { t } = useTranslation('officeJobAdPackages')

  return (
    <Card className="flex flex-col gap-3">
      <div className="flex items-center justify-between gap-2">
        <p className="font-medium text-text">{pkg.name}</p>
        <span
          className={
            pkg.isActive
              ? 'inline-flex items-center gap-1.5 rounded-full bg-success/10 px-2.5 py-1 text-xs font-medium text-success'
              : 'inline-flex items-center gap-1.5 rounded-full bg-neutral/10 px-2.5 py-1 text-xs font-medium text-neutral'
          }
        >
          {pkg.isActive ? t('status.active') : t('status.inactive')}
        </span>
      </div>

      {!editing && (
        <div className="flex flex-wrap gap-4 text-sm text-text-muted">
          <span>
            {pkg.price} {pkg.currency}
          </span>
          <span>
            {pkg.cvViewLimit === null ? t('unlimitedCv') : pkg.cvViewLimit} CV
          </span>
          <span>
            {pkg.durationDays} {t('days')}
          </span>
        </div>
      )}

      {editing && (
        <JobAdPackageEditForm
          package={pkg}
          isSubmitting={isSaving}
          error={saveError}
          onSave={(payload) => onSave(payload)}
          onCancel={onCancelEdit}
        />
      )}

      {!editing && (
        <div className="flex flex-wrap gap-2">
          <Button variant="secondary" onClick={onStartEdit}>
            {t('editAction')}
          </Button>
          <Button
            variant={pkg.isActive ? 'danger' : 'primary'}
            disabled={isTogglingActive}
            onClick={() => onToggleActive(!pkg.isActive)}
          >
            {pkg.isActive ? t('deactivateAction') : t('activateAction')}
          </Button>
        </div>
      )}
    </Card>
  )
}
