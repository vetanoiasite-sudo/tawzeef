using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Every role's own notifications — API_DOCUMENTATION.md §3.11. notifications.viewown is
/// the catalogue's only action for this module (no separate "view all" exists), granted to every
/// authenticated role for their own rows, OfficeStaff included without a StaffPermissionGrant
/// (RBAC_PERMISSIONS.md §3's matrix: a plain checkmark for every column).</summary>
[Route("api/notifications")]
public class NotificationsController(INotificationService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("mine")]
    [RequiresPermission("notifications.viewown")]
    public async Task<IActionResult> ListMine([FromQuery] NotificationQueryDto query, CancellationToken ct)
        => RespondPaged(await service.ListMineAsync(RequireUserId(), query, ct));

    [HttpPost("{id:guid}/read")]
    [RequiresPermission("notifications.viewown")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
        => Respond(await service.MarkReadAsync(RequireUserId(), id, ct));

    [HttpPost("read-all")]
    [RequiresPermission("notifications.viewown")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        => Respond(await service.MarkAllReadAsync(RequireUserId(), ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}
