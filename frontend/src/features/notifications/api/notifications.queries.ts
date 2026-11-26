import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { notificationsApi } from './notifications.api'
import { notificationsKeys } from './notifications.keys'
import type { NotificationQuery } from '../types/notifications.types'

export function useMyNotifications(query: NotificationQuery) {
  return useQuery({
    queryKey: notificationsKeys.mine(query),
    queryFn: () => notificationsApi.listMine(query),
  })
}

/** Polled lightly (no push mechanism exists yet) to keep the nav bell's badge roughly current. */
export function useUnreadNotificationCount() {
  const query: NotificationQuery = { unreadOnly: true, pageIndex: 0, pageSize: 1 }
  return useQuery({
    queryKey: notificationsKeys.mine(query),
    queryFn: () => notificationsApi.listMine(query),
    select: (data) => data.pagination.totalCount,
    refetchInterval: 60_000,
  })
}

export function useMarkNotificationRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => notificationsApi.markRead(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationsKeys.all }),
  })
}

export function useMarkAllNotificationsRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => notificationsApi.markAllRead(),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: notificationsKeys.all }),
  })
}

export function useRespondToEmploymentCheck() {
  return useMutation({ mutationFn: notificationsApi.respondToEmploymentCheck })
}

export function useRespondToHiringFollowUp() {
  return useMutation({ mutationFn: notificationsApi.respondToHiringFollowUp })
}

export function useRespondToInterviewFeedback() {
  return useMutation({ mutationFn: notificationsApi.respondToInterviewFeedback })
}
