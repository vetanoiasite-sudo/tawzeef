using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Complaints;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>The office complaint-review queue — RBAC_PERMISSIONS.md §3's
/// complaints.view/.triage/.resolve/.decide, office-scoped. API_DOCUMENTATION.md §3.10.</summary>
[Route("api/office/complaints")]
public class OfficeComplaintsController(IComplaintReviewService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListQueue([FromQuery] ComplaintQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListQueueAsync(RequireOfficeId(), query, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetByIdAsync(RequireOfficeId(), id, ct));

    [HttpPost("{id:guid}/triage")]
    [RequiresPermission("complaints.triage")]
    public async Task<IActionResult> Triage(Guid id, [FromBody] TriageComplaintDto dto, CancellationToken ct)
        => Respond(await service.TriageAsync(RequireOfficeId(), RequireUserId(), id, dto.ValidityDecision, ct));

    [HttpPost("{id:guid}/resolve")]
    [RequiresPermission("complaints.resolve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveComplaintDto dto, CancellationToken ct)
        => Respond(await service.ResolveAsync(RequireOfficeId(), RequireUserId(), id, dto.Outcome, dto.ResolutionNotes, ct));

    /// <summary>OfficeAdmin only — RBAC_PERMISSIONS.md §0 item 8, enforced by
    /// PermissionCatalogue.OfficeAdminOnly in PermissionChecker.</summary>
    [HttpPost("{id:guid}/decide")]
    [RequiresPermission("complaints.decide")]
    public async Task<IActionResult> Decide(Guid id, [FromBody] DecideEnforcementDto dto, CancellationToken ct)
        => Respond(await service.DecideAsync(RequireOfficeId(), RequireUserId(), id, dto.DecisionType, dto.Reason, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}
