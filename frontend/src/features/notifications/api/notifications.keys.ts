import type { NotificationQuery } from '../types/notifications.types'

export const notificationsKeys = {
  all: ['notifications'] as const,
  mine: (query: NotificationQuery) => [...notificationsKeys.all, 'mine', query] as const,
}
