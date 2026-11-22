using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.AuditLog;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>سجل التدقيق — a read-only viewer over OfficeDecision (BUSINESS_REQUIREMENTS.md
/// §11.3/§11.4's suspend/reinstate/stop-cooperation/resume-cooperation trail). Office staff
/// (via auditlogs.view, held by OfficeAdmin automatically) see only their own office; SuperAdmin
/// sees every office and may filter by officeId.</summary>
[Route("api/audit-log")]
public class AuditLogController(IAuditLogService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    [RequiresPermission("auditlogs.view")]
    public async Task<IActionResult> List([FromQuery] AuditLogQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListAsync(currentUser.OfficeId, query, ct));
}
