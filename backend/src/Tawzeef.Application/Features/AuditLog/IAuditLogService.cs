using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.AuditLog;

public interface IAuditLogService
{
    /// <summary>officeId is the caller's own office (non-null for office-scoped staff, forcing
    /// the query to that office regardless of query.OfficeId); null means the caller is
    /// SuperAdmin and may optionally filter by query.OfficeId.</summary>
    Task<ServiceResult<PagedResult<AuditLogEntryDto>>> ListAsync(Guid? officeId, AuditLogQueryDto query, CancellationToken ct = default);
}
