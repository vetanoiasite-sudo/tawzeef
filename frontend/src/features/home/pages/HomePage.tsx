import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useAccess } from '@/shared/auth/useAccess'
import { TawzeefLogo } from '@/shared/ui/TawzeefLogo'
import { Button } from '@/shared/ui/Button'

export default function HomePage() {
  const { t } = useTranslation(['home', 'auth'])
  const { isAuthenticated } = useAccess()

  return (
    <div className="flex min-h-[70vh] flex-col items-center justify-center gap-6 px-4 text-center">
      <TawzeefLogo height={82} />
      <p className="max-w-md text-sm text-text-muted">{t('home:tagline')}</p>
      <div className="flex flex-wrap items-center justify-center gap-3">
        <Link to="/jobs">
          <Button variant="primary">{t('home:browseJobsAction')}</Button>
        </Link>
        {!isAuthenticated && (
          <Link to="/login">
            <Button variant="secondary">{t('auth:loginLink')}</Button>
          </Link>
        )}
      </div>
    </div>
  )
}
