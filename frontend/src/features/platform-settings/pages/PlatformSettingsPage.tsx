import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '@/shared/api/types'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'
import { TextInput } from '@/shared/ui/TextInput'
import { PageHeader } from '@/shared/ui/PageHeader'
import { LoadingState } from '@/shared/ui/LoadingState'
import { usePlatformSettings, useUpdatePlatformSetting } from '../api/platformSettings.queries'
import type { PlatformSetting } from '../types/platformSettings.types'

// These two settings hold plain numbers (days / an amount) — the currency setting stays text.
// Backend/src/Tawzeef.Domain/Common/PlatformSettingKeys.cs is the source of truth for the keys.
const NUMERIC_SETTING_KEYS = new Set(['advertisement.duration_days', 'advertisement.default_fee_amount'])

function SettingRow({ setting }: { setting: PlatformSetting }) {
  const { t } = useTranslation('platformSettings')
  const [editing, setEditing] = useState(false)
  const [value, setValue] = useState(setting.value)
  const update = useUpdatePlatformSetting()

  const label = t(`settingLabel.${setting.key}`, { defaultValue: setting.key })

  function handleSave() {
    update.mutate({ key: setting.key, value }, { onSuccess: () => setEditing(false) })
  }

  return (
    <Card className="flex flex-col gap-3">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="font-bold text-content-primary">{label}</p>
          <p className="text-sm text-content-tertiary">{setting.description}</p>
        </div>
        {!editing && (
          <Button
            variant="secondary"
            size="sm"
            onClick={() => {
              setValue(setting.value)
              setEditing(true)
            }}
          >
            {t('editAction')}
          </Button>
        )}
      </div>

      {editing ? (
        <div className="flex flex-wrap items-end gap-3">
          <div className="min-w-[200px] flex-1">
            <TextInput
              type={NUMERIC_SETTING_KEYS.has(setting.key) ? 'number' : 'text'}
              value={value}
              onChange={(e) => setValue(e.target.value)}
            />
          </div>
          <Button size="sm" disabled={update.isPending} onClick={handleSave}>
            {t('saveAction')}
          </Button>
          <Button
            variant="secondary"
            size="sm"
            disabled={update.isPending}
            onClick={() => setEditing(false)}
          >
            {t('cancelAction')}
          </Button>
        </div>
      ) : (
        <p className="text-lg font-extrabold text-brand tabular-nums">{setting.value}</p>
      )}

      {update.isError && update.variables?.key === setting.key && (
        <p className="text-sm text-danger">
          {update.error instanceof ApiError ? update.error.messages[0] : t('updateError')}
        </p>
      )}

      <p className="text-xs text-content-tertiary">
        {t('savedAt', { date: new Date(setting.updatedAt).toLocaleString('ar-EG') })}
      </p>
    </Card>
  )
}

export default function PlatformSettingsPage() {
  const { t } = useTranslation('platformSettings')
  const { data: settings, isLoading } = usePlatformSettings()

  if (isLoading) return <LoadingState />

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('pageTitle')} />
      <p className="text-sm text-content-tertiary">{t('pageSubtitle')}</p>

      <div className="flex flex-col gap-4">
        {settings?.map((setting) => (
          <SettingRow key={setting.key} setting={setting} />
        ))}
        {settings?.length === 0 && (
          <Card>
            <p className="text-sm text-content-tertiary">{t('emptyState')}</p>
          </Card>
        )}
      </div>
    </div>
  )
}
