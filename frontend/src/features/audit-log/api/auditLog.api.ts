import { httpClient } from '@/shared/api/httpClient'
import type { AuditLogEntry, AuditLogFilter } from '../types/auditLog.types'

export const auditLogApi = {
  list: (filter: AuditLogFilter) =>
    httpClient.getPaged<AuditLogEntry[]>('/api/audit-log', {
      officeId: filter.officeId,
      targetType: filter.targetType,
      decisionType: filter.decisionType,
      pageIndex: filter.pageIndex,
      pageSize: filter.pageSize,
    }),
}
