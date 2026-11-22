using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Common;

namespace Tawzeef.Tests.TestHelpers;

/// <summary>Records every "notify the office admins" call instead of touching the database, so a
/// test can assert exactly how many times (and for which subject) a notification fired — the
/// thing ComplaintReviewService.TriageAsync's 3-strike alert behavior actually needs verifying.
/// The read-side methods are never called by the services under test here.</summary>
public class RecordingNotificationService : INotificationService
{
    public List<(Guid OfficeId, string EventType, string Message, Guid? RelatedId)> OfficeAdminCalls { get; } = [];

    public Task<ServiceResult<PagedResult<NotificationDto>>> ListMineAsync(Guid userId, NotificationQueryDto query, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<ServiceResult> MarkReadAsync(Guid userId, Guid id, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<ServiceResult> MarkAllReadAsync(Guid userId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task CreateAsync(Guid? officeId, Guid recipientUserId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task CreateForOfficeAdminsAsync(Guid officeId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default)
    {
        OfficeAdminCalls.Add((officeId, eventType, message, relatedId));
        return Task.CompletedTask;
    }
}
