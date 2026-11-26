using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.Notifications;

/// <summary>API_DOCUMENTATION.md §3.11's controller-facing surface, plus the creation helpers other
/// Application services call directly (never through HTTP) to raise a notification —
/// BUSINESS_REQUIREMENTS.md §13's "keep notification generation separate from the delivery
/// provider". The creation methods only stage the row (no SaveChangesAsync of their own); the
/// calling service's own save persists it in the same commit as whatever triggered it.</summary>
public interface INotificationService
{
    Task<ServiceResult<PagedResult<NotificationDto>>> ListMineAsync(Guid userId, NotificationQueryDto query, CancellationToken ct = default);
    Task<ServiceResult> MarkReadAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<ServiceResult> MarkAllReadAsync(Guid userId, CancellationToken ct = default);

    Task CreateAsync(Guid? officeId, Guid recipientUserId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default);

    /// <summary>One notification per active OfficeAdmin of the office — OfficeStaff are never
    /// included, since they may not even have been granted the relevant action.</summary>
    Task CreateForOfficeAdminsAsync(Guid officeId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default);

    /// <summary>One notification per active member of the office — OfficeAdmin and OfficeStaff
    /// alike. Added 2026-11-26 for the new "something landed in the office's queue" notifications,
    /// deliberately unfiltered by role for now (confirmed: role-based targeting, e.g. an "accounts"
    /// role, is a later pass — don't narrow this on your own).</summary>
    Task CreateForAllOfficeStaffAsync(Guid officeId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default);
}
