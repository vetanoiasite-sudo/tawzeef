import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { useUnreadNotificationCount } from '../api/notifications.queries'

export function NotificationBell() {
  const { t } = useTranslation('notifications')
  const { data: unreadCount } = useUnreadNotificationCount()

  return (
    <Link
      to="/notifications"
      aria-label={t('pageTitle')}
      className="relative inline-flex size-9 items-center justify-center rounded-md text-text-muted transition-colors duration-150 hover:bg-primary/10 hover:text-primary"
    >
      <svg viewBox="0 0 24 24" fill="none" className="size-5" aria-hidden="true">
        <path
          d="M12 3a6 6 0 0 0-6 6v2.6c0 .5-.15.98-.44 1.39L4.3 15.1a1 1 0 0 0 .81 1.57h13.78a1 1 0 0 0 .81-1.57l-1.26-2.11a2.5 2.5 0 0 1-.44-1.39V9a6 6 0 0 0-6-6Z"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinejoin="round"
        />
        <path
          d="M9.5 19a2.5 2.5 0 0 0 5 0"
          stroke="currentColor"
          strokeWidth="1.6"
          strokeLinecap="round"
        />
      </svg>
      {!!unreadCount && unreadCount > 0 && (
        <span className="absolute end-0.5 top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-danger px-1 text-[10px] font-medium text-surface">
          {unreadCount > 99 ? '99+' : unreadCount}
        </span>
      )}
    </Link>
  )
}
