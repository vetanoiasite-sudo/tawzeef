import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useSessionStore } from '@/shared/auth/sessionStore'
import { useOfficePlatformStats } from '../api/superAdmin.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Card } from '@/shared/ui/Card'
import { Button } from '@/shared/ui/Button'

/** The page a SuperAdmin lands on right after login — platform-wide totals plus the one action
 * that mattered most before this feature existed: managing offices (previously API-only). */
export default function SuperAdminDashboardPage() {
  const { t } = useTranslation('superAdmin')
  const user = useSessionStore((s) => s.user)
  const { data: stats } = useOfficePlatformStats()

  const tiles = [
    { label: t('totalOfficesLabel'), value: stats?.totalOfficesCount },
    { label: t('activeOfficesLabel'), value: stats?.activeOfficesCount },
    { label: t('inactiveOfficesLabel'), value: stats?.inactiveOfficesCount },
  ]

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-6 px-4 py-8 sm:px-6">
      <PageHeader title={t('dashboardGreeting', { name: user?.fullName ?? '' })} />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        {tiles.map((tile) => (
          <Card key={tile.label} className="flex flex-col gap-2">
            <p className="text-sm font-bold text-content-secondary">{tile.label}</p>
            <p className="text-3xl font-extrabold text-brand tabular-nums">{tile.value ?? '—'}</p>
          </Card>
        ))}
      </div>

      <div className="flex flex-wrap gap-3">
        <Link to="/super-admin/offices">
          <Button variant="primary">{t('manageOfficesAction')}</Button>
        </Link>
        <Link to="/super-admin/settings">
          <Button variant="secondary">{t('platformSettings:nav')}</Button>
        </Link>
      </div>
    </div>
  )
}
