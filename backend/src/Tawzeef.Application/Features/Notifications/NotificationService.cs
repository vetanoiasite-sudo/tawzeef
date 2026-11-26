using Microsoft.AspNetCore.Identity;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Notifications;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Application.Features.Notifications;

public class NotificationService(
    IUnitOfWork uow, UserManager<ApplicationUser> userManager, IEmailSender emailSender) : INotificationService
{
    public async Task<ServiceResult<PagedResult<NotificationDto>>> ListMineAsync(Guid userId, NotificationQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<Notification>().PageAsync(
            pageIndex, pageSize,
            n => n.RecipientUserId == userId && (!query.UnreadOnly || n.ReadAt == null),
            orderBy: n => n.CreatedAt, descending: true, ct: ct);

        return ServiceResult<PagedResult<NotificationDto>>.Success(new PagedResult<NotificationDto>
        {
            Items = page.Items.Select(n => n.ToDto()).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult> MarkReadAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var notification = await uow.Repository<Notification>().FindAsync(n => n.Id == id && n.RecipientUserId == userId, ct: ct);
        if (notification is null) return ServiceResult.NotFound("Notification not found.");

        if (notification.ReadAt is null)
        {
            notification.ReadAt = DateTime.UtcNow;
            uow.Repository<Notification>().Update(notification);
            await uow.SaveChangesAsync(ct);
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await uow.Repository<Notification>().ListAsync(n => n.RecipientUserId == userId && n.ReadAt == null, ct: ct);
        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.ReadAt = now;
            uow.Repository<Notification>().Update(notification);
        }
        if (unread.Count > 0) await uow.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task CreateAsync(
        Guid? officeId, Guid recipientUserId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default)
    {
        await uow.Repository<Notification>().AddAsync(new Notification
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            RecipientUserId = recipientUserId,
            EventType = eventType,
            Message = message,
            RelatedType = relatedType,
            RelatedId = relatedId,
        }, ct);

        // Added 2026-11-23: every in-app notification also goes out by email, so the user finds
        // out about a change to their account even if they never open the app.
        var recipient = await userManager.FindByIdAsync(recipientUserId.ToString());
        if (!string.IsNullOrWhiteSpace(recipient?.Email))
            await emailSender.SendAsync(recipient.Email, "إشعار جديد من توظيف", message, ct);
    }

    public async Task CreateForOfficeAdminsAsync(
        Guid officeId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default)
    {
        var admins = await uow.Repository<UserOfficeMembership>().ListAsync(
            m => m.OfficeId == officeId && m.Role == OfficeRole.OfficeAdmin && m.MembershipStatus == MembershipStatus.Active, ct: ct);

        foreach (var admin in admins)
            await CreateAsync(officeId, admin.UserId, eventType, message, relatedType, relatedId, ct);
    }

    public async Task CreateForAllOfficeStaffAsync(
        Guid officeId, string eventType, string message, string? relatedType = null, Guid? relatedId = null, CancellationToken ct = default)
    {
        var members = await uow.Repository<UserOfficeMembership>().ListAsync(
            m => m.OfficeId == officeId && m.MembershipStatus == MembershipStatus.Active, ct: ct);

        foreach (var member in members)
            await CreateAsync(officeId, member.UserId, eventType, message, relatedType, relatedId, ct);
    }
}
