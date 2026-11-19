using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.OfficeUsers;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>RBAC_PERMISSIONS.md §2/§3: OfficeAdmin-only, never grantable to OfficeStaff (footnote
/// 1 — a staff member must never be able to escalate their own access). Enforced by
/// PermissionCatalogue.OfficeAdminOnly in PermissionChecker, not by the attributes below alone.
/// Invite is the one dual-mode action: a SuperAdmin may invite into any office (the "first
/// OfficeAdmin" case, BUSINESS_REQUIREMENTS.md §12.3); everything else here is scoped to the
/// caller's own office, same as every other office-review controller in this app.</summary>
[Route("api/office-users")]
public class OfficeUsersController(IOfficeUserService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost("invite")]
    [RequiresPermission("officeusers.invite")]
    public async Task<IActionResult> Invite([FromBody] InviteOfficeStaffDto dto, CancellationToken ct)
        => Respond(await service.InviteAsync(RequireUserId(), currentUser.IsSuperAdmin, currentUser.OfficeId, dto, ct));

    [HttpGet]
    [RequiresPermission("officeusers.view")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Respond(await service.ListAsync(RequireOfficeId(), ct));

    [HttpPut("{id:guid}/permissions")]
    [RequiresPermission("officeusers.edit")]
    public async Task<IActionResult> UpdatePermissions(Guid id, [FromBody] UpdateOfficeUserPermissionsDto dto, CancellationToken ct)
        => Respond(await service.UpdatePermissionsAsync(RequireOfficeId(), id, dto.PermissionCodes, ct));

    [HttpDelete("{id:guid}")]
    [RequiresPermission("officeusers.deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        => Respond(await service.DeactivateAsync(RequireOfficeId(), RequireUserId(), id, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}
