using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Common;

namespace Tawzeef.Tests.TestHelpers;

/// <summary>No-op — these tests exercise review/application logic, not notification delivery
/// (which needs a real UserManager/IEmailSender NotificationService doesn't have here).</summary>
public class FakeNotificationService : INotificationService
{
    public Task<ServiceResult<PagedResult<NotificationDto>>> ListMineAsync(Guid userId, NotificationQueryDto query, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<ServiceResult> MarkReadAsync(Guid userId, Guid id, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<ServiceResult> MarkAllReadAsync(Guid userId, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task CreateAsync(Guid? officeId, Guid recipientUserId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task CreateForOfficeAdminsAsync(Guid officeId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default) =>
        Task.CompletedTask;
}
