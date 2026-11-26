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
  // The job seeker's own replies to the three self-report notifications — {id} is always the
  // Notification row's relatedId (JobSeekerOfficeApplication/HiringFollowUpReport/Interview).
  respondToEmploymentCheck: ({ id, foundJob }: { id: string; foundJob: boolean }) =>
    httpClient.post<void>(`/api/jobseekers/me/follow-up/employment-check/${id}/respond`, { foundJob }),
  respondToHiringFollowUp: ({ id, stillEmployed, notes }: { id: string; stillEmployed: boolean; notes?: string }) =>
    httpClient.post<void>(`/api/jobseekers/me/follow-up/hiring-follow-up/${id}/respond`, { stillEmployed, notes }),
  respondToInterviewFeedback: ({
    id,
    outcome,
    notes,
  }: {
    id: string
    outcome: 'StillWaiting' | 'Accepted' | 'Rejected'
    notes?: string
  }) => httpClient.post<void>(`/api/jobseekers/me/follow-up/interview/${id}/feedback`, { outcome, notes }),
}
