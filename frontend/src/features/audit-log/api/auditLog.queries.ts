import { useQuery } from '@tanstack/react-query'
import { auditLogApi } from './auditLog.api'
import { auditLogKeys } from './auditLog.keys'
import type { AuditLogFilter } from '../types/auditLog.types'

export function useAuditLog(filter: AuditLogFilter) {
  return useQuery({
    queryKey: auditLogKeys.list(filter),
    queryFn: () => auditLogApi.list(filter),
  })
}
