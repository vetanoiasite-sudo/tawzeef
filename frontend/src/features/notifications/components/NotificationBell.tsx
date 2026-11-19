import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useUnreadNotificationCount } from '../api/notifications.queries'

export function NotificationBell() {
  const { t } = useTranslation('notifications')
  const { data: unreadCount } = useUnreadNotificationCount()

  return (
    <Link to="/notifications" className="relative text-primary">
      {t('pageTitle')}
      {!!unreadCount && unreadCount > 0 && (
        <span className="absolute -end-2 -top-2 flex h-4 min-w-4 items-center justify-center rounded-full bg-danger px-1 text-[10px] font-medium text-surface">
          {unreadCount > 99 ? '99+' : unreadCount}
        </span>
      )}
    </Link>
  )
}
