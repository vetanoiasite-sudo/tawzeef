/** Mirrors backend/src/Tawzeef.Application/Features/AuditLog/AuditLogDtos.cs. */

export type AuditLogTargetTypeCode = 'JobSeeker' | 'Employer'

export type AuditLogDecisionTypeCode =
  'Suspend' | 'Reinstate' | 'StopCooperation' | 'ResumeCooperation'

export type AuditLogEntry = {
  id: string
  officeId: string
  officeName: string | null
  targetType: AuditLogTargetTypeCode
  targetId: string
  targetName: string | null
  decisionType: AuditLogDecisionTypeCode
  reason: string
  decidedBy: string
  decidedByName: string | null
  decidedAt: string
}

export type AuditLogFilter = {
  officeId?: string
  targetType?: AuditLogTargetTypeCode
  decisionType?: AuditLogDecisionTypeCode
  pageIndex: number
  pageSize: number
}
