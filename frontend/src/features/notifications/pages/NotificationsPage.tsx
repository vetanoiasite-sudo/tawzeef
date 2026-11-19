import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useMarkAllNotificationsRead, useMarkNotificationRead, useMyNotifications } from '../api/notifications.queries'

const PAGE_SIZE = 20

export default function NotificationsPage() {
  const { t } = useTranslation('notifications')
  const [unreadOnly, setUnreadOnly] = useState(false)
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useMyNotifications({ unreadOnly, pageIndex, pageSize: PAGE_SIZE })
  const markRead = useMarkNotificationRead()
  const markAllRead = useMarkAllNotificationsRead()

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-10">
      <div className="flex items-center justify-between">
        <h1 className="text-lg font-semibold text-text">{t('pageTitle')}</h1>
        <button
          type="button"
          disabled={markAllRead.isPending}
          onClick={() => markAllRead.mutate()}
          className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text disabled:opacity-60"
        >
          {t('markAllRead')}
        </button>
      </div>

      <div className="flex gap-2">
        <button
          type="button"
          onClick={() => {
            setUnreadOnly(false)
            setPageIndex(0)
          }}
          className={
            !unreadOnly
              ? 'rounded-full bg-primary px-3 py-1 text-sm font-medium text-surface'
              : 'rounded-full border border-surface-muted px-3 py-1 text-sm text-text'
          }
        >
          {t('all')}
        </button>
        <button
          type="button"
          onClick={() => {
            setUnreadOnly(true)
            setPageIndex(0)
          }}
          className={
            unreadOnly
              ? 'rounded-full bg-primary px-3 py-1 text-sm font-medium text-surface'
              : 'rounded-full border border-surface-muted px-3 py-1 text-sm text-text'
          }
        >
          {t('unreadOnly')}
        </button>
      </div>

      {isLoading && <p className="text-sm text-text-muted">...</p>}

      {!isLoading && data && data.items.length === 0 && <p className="text-sm text-text-muted">{t('emptyList')}</p>}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-2">
          {data.items.map((n) => (
            <div
              key={n.id}
              className={
                n.readAt
                  ? 'flex items-start justify-between gap-3 rounded-md border border-surface-muted p-3'
                  : 'flex items-start justify-between gap-3 rounded-md border border-primary bg-primary/5 p-3'
              }
            >
              <div className="flex flex-col gap-1">
                <p className="text-sm text-text">{n.message}</p>
                <p className="text-xs text-text-muted">{new Date(n.createdAt).toLocaleString('ar')}</p>
              </div>
              {!n.readAt && (
                <button
                  type="button"
                  disabled={markRead.isPending}
                  onClick={() => markRead.mutate(n.id)}
                  className="shrink-0 text-sm text-primary disabled:opacity-60"
                >
                  {t('markRead')}
                </button>
              )}
            </div>
          ))}
        </div>
      )}

      {data && data.pagination.totalPages > 1 && (
        <div className="flex items-center justify-center gap-4">
          <button
            type="button"
            disabled={pageIndex === 0}
            onClick={() => setPageIndex((p) => p - 1)}
            className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text disabled:opacity-60"
          >
            {t('prevPage')}
          </button>
          <span className="text-sm text-text-muted">
            {t('pageOf', { current: pageIndex + 1, total: data.pagination.totalPages })}
          </span>
          <button
            type="button"
            disabled={pageIndex + 1 >= data.pagination.totalPages}
            onClick={() => setPageIndex((p) => p + 1)}
            className="rounded-md border border-surface-muted px-3 py-1.5 text-sm text-text disabled:opacity-60"
          >
            {t('nextPage')}
          </button>
        </div>
      )}
    </div>
  )
}
