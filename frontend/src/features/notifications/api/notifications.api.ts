import { httpClient } from '@/shared/api/httpClient'
import type { Notification, NotificationQuery } from '../types/notifications.types'

export const notificationsApi = {
  listMine: (query: NotificationQuery) =>
    httpClient.getPaged<Notification[]>('/api/notifications/mine', {
      unreadOnly: query.unreadOnly,
      pageIndex: query.pageIndex,
      pageSize: query.pageSize,
    }),
  markRead: (id: string) => httpClient.post<void>(`/api/notifications/${id}/read`),
  markAllRead: () => httpClient.post<void>('/api/notifications/read-all'),
}
