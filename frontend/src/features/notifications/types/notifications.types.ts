/** Mirrors backend/src/Tawzeef.Application/Features/Notifications/NotificationDtos.cs. */

export type Notification = {
  id: string
  eventType: string
  relatedType: string | null
  relatedId: string | null
  message: string
  readAt: string | null
  createdAt: string
}

export type NotificationQuery = {
  unreadOnly: boolean
  pageIndex: number
  pageSize: number
}
