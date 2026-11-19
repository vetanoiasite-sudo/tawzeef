using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Interviews;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>The office's "due this month" work queue — API_DOCUMENTATION.md §3.9's
/// /api/office/hiring-follow-ups/due route.</summary>
[Route("api/office/hiring-follow-ups")]
public class OfficeHiringFollowUpsController(IHiringFollowUpService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("due")]
    [RequiresPermission("hiringfollowupreports.view")]
    public async Task<IActionResult> ListDue([FromQuery] HiringFollowUpQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListDueAsync(RequireOfficeId(), query, ct));

    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}
