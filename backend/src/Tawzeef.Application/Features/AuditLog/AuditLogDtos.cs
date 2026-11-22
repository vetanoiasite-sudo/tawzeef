using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.AuditLog;

public class AuditLogQueryDto
{
    /// <summary>SuperAdmin-only filter — ignored (and overridden to the caller's own office) for
    /// office-scoped callers. Null for SuperAdmin means "every office".</summary>
    public Guid? OfficeId { get; set; }

    public OfficeDecisionTargetType? TargetType { get; set; }
    public OfficeDecisionType? DecisionType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

public class AuditLogEntryDto
{
    public Guid Id { get; set; }
    public Guid OfficeId { get; set; }
    public string? OfficeName { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public string? TargetName { get; set; }
    public string DecisionType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public Guid DecidedBy { get; set; }
    public string? DecidedByName { get; set; }
    public DateTime DecidedAt { get; set; }
}
