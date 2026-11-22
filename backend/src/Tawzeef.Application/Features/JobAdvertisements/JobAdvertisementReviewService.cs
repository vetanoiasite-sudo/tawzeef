using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Application.Features.PlatformSettings;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobAdvertisements;

public class JobAdvertisementReviewService(
    IUnitOfWork uow, INotificationService notificationService, IPlatformSettingsService platformSettings) : IJobAdvertisementReviewService
{
    public async Task<ServiceResult<PagedResult<JobAdvertisementListItemDto>>> ListQueueAsync(
        Guid officeId, JobAdvertisementQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<JobAdvertisement>().PageAsync(
            pageIndex,
            pageSize,
            a => a.OfficeId == officeId && (!query.Status.HasValue || a.Status == query.Status.Value),
            orderBy: a => a.CreationDate,
            descending: true,
            ct: ct);

        var employerIds = page.Items.Select(a => a.EmployerId).Distinct().ToList();
        var employers = employerIds.Count == 0
            ? []
            : await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var namesByEmployer = employers.ToDictionary(e => e.Id, e => e.DisplayName);

        return ServiceResult<PagedResult<JobAdvertisementListItemDto>>.Success(new PagedResult<JobAdvertisementListItemDto>
        {
            Items = page.Items.Select(a => a.ToListItemDto(namesByEmployer.GetValueOrDefault(a.EmployerId))).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<JobAdvertisementDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");

        // Same implicit-transition convention as every other review queue in this codebase.
        if (ad.Status == JobAdvertisementStatus.SubmittedForReview)
        {
            ad.Status = JobAdvertisementStatus.UnderReview;
            uow.Repository<JobAdvertisement>().Update(ad);
            await uow.SaveChangesAsync(ct);
        }

        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == ad.EmployerId, ct: ct);
        var payment = await uow.Repository<AdvertisementPayment>().FindAsync(p => p.JobAdvertisementId == ad.Id, ct: ct);
        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto(payment, employer?.DisplayName));
    }

    public async Task<ServiceResult<JobAdvertisementDto>> ApproveAsync(Guid officeId, Guid reviewerId, Guid id, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");
        if (ad.Status != JobAdvertisementStatus.UnderReview)
            return ServiceResult<JobAdvertisementDto>.Conflict("The advertisement is not under review.");

        ad.Status = JobAdvertisementStatus.ApprovedAwaitingPayment;
        ad.ApprovedBy = reviewerId;
        ad.ApprovedAt = DateTime.UtcNow;
        ad.RejectionReason = null;
        uow.Repository<JobAdvertisement>().Update(ad);

        // WORKFLOWS_AND_STATE_MACHINES.md §5 transition table: "Notification -> Employer with
        // payment instructions".
        await NotifyEmployerAsync(ad, "JobAdvertisement.Approved", $"تمت الموافقة على إعلان \"{ad.Title}\" — برجاء إتمام السداد لنشره.", ct);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto());
    }

    public async Task<ServiceResult<JobAdvertisementDto>> RejectAsync(Guid officeId, Guid reviewerId, Guid id, string reason, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");
        if (ad.Status != JobAdvertisementStatus.UnderReview)
            return ServiceResult<JobAdvertisementDto>.Conflict("The advertisement is not under review.");

        ad.Status = JobAdvertisementStatus.Rejected;
        ad.RejectionReason = reason.Trim();
        uow.Repository<JobAdvertisement>().Update(ad);

        // WORKFLOWS_AND_STATE_MACHINES.md §5 transition table: "Notification -> Employer".
        await NotifyEmployerAsync(ad, "JobAdvertisement.Rejected", $"تم رفض إعلان \"{ad.Title}\" — السبب: {ad.RejectionReason}", ct);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto());
    }

    private async Task NotifyEmployerAsync(JobAdvertisement ad, string eventType, string message, CancellationToken ct)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == ad.EmployerId, ct: ct);
        if (employer is null) return;
        await notificationService.CreateAsync(ad.OfficeId, employer.UserId, eventType, message, nameof(JobAdvertisement), ad.Id, ct);
    }

    public async Task<ServiceResult<JobAdvertisementDto>> PauseAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");
        if (ad.Status != JobAdvertisementStatus.Published)
            return ServiceResult<JobAdvertisementDto>.Conflict("Only a published advertisement can be paused.");

        ad.Status = JobAdvertisementStatus.Paused;
        uow.Repository<JobAdvertisement>().Update(ad);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto());
    }

    public async Task<ServiceResult<JobAdvertisementDto>> ResumeAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");
        if (ad.Status != JobAdvertisementStatus.Paused)
            return ServiceResult<JobAdvertisementDto>.Conflict("Only a paused advertisement can be resumed.");

        ad.Status = JobAdvertisementStatus.Published;
        uow.Repository<JobAdvertisement>().Update(ad);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto());
    }

    public async Task<ServiceResult<JobAdvertisementDto>> ConfirmPaymentAsync(
        Guid officeId, Guid reviewerId, Guid id, ConfirmAdvertisementPaymentDto dto, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");
        if (ad.Status != JobAdvertisementStatus.ApprovedAwaitingPayment)
            return ServiceResult<JobAdvertisementDto>.Conflict("This advertisement is not awaiting payment.");

        // Deliberate v1 simplification (no payment gateway exists, no "initiate payment" route is
        // documented either): one manual office action records the payment AND publishes the ad,
        // collapsing the diagram's PaymentPending step rather than exposing an endpoint for a
        // transition nothing can ever trigger. WORKFLOWS_AND_STATE_MACHINES.md §5/§6.
        var payment = new AdvertisementPayment
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            EmployerId = ad.EmployerId,
            JobAdvertisementId = ad.Id,
            Amount = dto.Amount,
            Currency = string.IsNullOrWhiteSpace(dto.Currency)
                ? await platformSettings.GetValueAsync(PlatformSettingKeys.AdvertisementDefaultFeeCurrency, "EGP", ct)
                : dto.Currency.Trim().ToUpperInvariant(),
            PaymentStatus = PaymentStatus.Paid,
            PaymentMethod = "Manual",
            PaidAt = DateTime.UtcNow,
            RecordedBy = reviewerId,
        };
        await uow.Repository<AdvertisementPayment>().AddAsync(payment, ct);

        // WORKFLOWS_AND_STATE_MACHINES.md §5 confirmed item 15: the clock anchors on publish time.
        // Duration is a platform setting now (platformsettings.edit), defaulting to the documented 15 days.
        var durationDaysRaw = await platformSettings.GetValueAsync(PlatformSettingKeys.AdvertisementDurationDays, "15", ct);
        var durationDays = int.TryParse(durationDaysRaw, out var parsedDays) ? parsedDays : 15;
        ad.Status = JobAdvertisementStatus.Published;
        ad.PublishedAt = DateTime.UtcNow;
        ad.ExpiresAt = ad.PublishedAt.Value.AddDays(durationDays);
        uow.Repository<JobAdvertisement>().Update(ad);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto(payment));
    }
}
