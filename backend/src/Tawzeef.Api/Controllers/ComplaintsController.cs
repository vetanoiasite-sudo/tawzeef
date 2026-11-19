using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Complaints;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Self-service — a JobSeeker or an Employer submitting/viewing their own complaints
/// (RBAC_PERMISSIONS.md §3: complaints.create/.viewown, granted to either role simply by holding
/// it). Not the office review surface (OfficeComplaintsController).</summary>
[Route("api/complaints")]
public class ComplaintsController(IComplaintService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost]
    [RequiresPermission("complaints.create")]
    public async Task<IActionResult> Create([FromBody] CreateComplaintDto dto, CancellationToken ct)
        => Respond(await service.CreateAsync(RequireUserId(), dto, ct));

    [HttpGet("mine")]
    [RequiresPermission("complaints.viewown")]
    public async Task<IActionResult> ListMine([FromQuery] int pageIndex, [FromQuery] int pageSize, CancellationToken ct)
        => RespondPaged(await service.ListMineAsync(RequireUserId(), pageIndex, pageSize, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}
