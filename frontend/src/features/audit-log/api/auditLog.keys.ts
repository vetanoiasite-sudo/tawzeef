import type { AuditLogFilter } from '../types/auditLog.types'

export const auditLogKeys = {
  all: ['auditLog'] as const,
  list: (filter: AuditLogFilter) => [...auditLogKeys.all, 'list', filter] as const,
}
