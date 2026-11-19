import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useMarkAllNotificationsRead, useMarkNotificationRead, useMyNotifications } from '../api/notifications.queries'
import { PageHeader } from '@/shared/ui/PageHeader'
import { Button } from '@/shared/ui/Button'
import { Card } from '@/shared/ui/Card'
import { FilterChips } from '@/shared/ui/FilterChips'
import { EmptyState } from '@/shared/ui/EmptyState'
import { LoadingState } from '@/shared/ui/LoadingState'
import { Pagination } from '@/shared/ui/Pagination'
import clsx from 'clsx'

const PAGE_SIZE = 20

export default function NotificationsPage() {
  const { t } = useTranslation('notifications')
  const [unreadOnly, setUnreadOnly] = useState(false)
  const [pageIndex, setPageIndex] = useState(0)

  const { data, isLoading } = useMyNotifications({ unreadOnly, pageIndex, pageSize: PAGE_SIZE })
  const markRead = useMarkNotificationRead()
  const markAllRead = useMarkAllNotificationsRead()

  return (
    <div className="mx-auto flex max-w-2xl flex-col gap-6 px-4 py-8">
      <PageHeader
        title={t('pageTitle')}
        action={
          <Button variant="secondary" disabled={markAllRead.isPending} onClick={() => markAllRead.mutate()}>
            {t('markAllRead')}
          </Button>
        }
      />

      <FilterChips
        options={[
          { value: 'all', label: t('all') },
          { value: 'unread', label: t('unreadOnly') },
        ]}
        value={unreadOnly ? 'unread' : 'all'}
        onChange={(v) => {
          setUnreadOnly(v === 'unread')
          setPageIndex(0)
        }}
      />

      {isLoading && <LoadingState />}

      {!isLoading && data && data.items.length === 0 && <EmptyState message={t('emptyList')} />}

      {!isLoading && data && data.items.length > 0 && (
        <div className="flex flex-col gap-2">
          {data.items.map((n) => (
            <Card
              key={n.id}
              className={clsx('flex items-start justify-between gap-3', !n.readAt && 'border-primary bg-primary/5')}
            >
              <div className="flex flex-col gap-1">
                <p className="text-sm text-text">{n.message}</p>
                <p className="text-xs text-text-muted">{new Date(n.createdAt).toLocaleString('ar')}</p>
              </div>
              {!n.readAt && (
                <Button variant="ghost" disabled={markRead.isPending} onClick={() => markRead.mutate(n.id)} className="shrink-0 px-2">
                  {t('markRead')}
                </Button>
              )}
            </Card>
          ))}
        </div>
      )}

      {data && <Pagination pageIndex={pageIndex} totalPages={data.pagination.totalPages} onChange={setPageIndex} />}
    </div>
  )
}
