using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Application.Features.PlatformSettings;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobAdvertisements;

public class JobAdvertisementReviewService(
    IUnitOfWork uow, INotificationService notificationService, IPlatformSettingsService platformSettings,
    IFileStorage fileStorage, IConfiguration configuration) : IJobAdvertisementReviewService
{
    // Same shared "what can an office attach" policy as every other upload in this app
    // (RBAC_PERMISSIONS.md §0 item 24) — a receipt is just as likely to be a photo as a PDF scan.
    private static readonly string[] ReceiptAllowedMimeTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
    ];
    private const long DefaultMaxFileSizeBytes = 2 * 1024 * 1024;

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

        var categoryNamesById = await GetCategoryNamesByIdAsync(page.Items.Select(a => a.CategoryId), ct);

        return ServiceResult<PagedResult<JobAdvertisementListItemDto>>.Success(new PagedResult<JobAdvertisementListItemDto>
        {
            Items = page.Items.Select(a => a.ToListItemDto(
                namesByEmployer.GetValueOrDefault(a.EmployerId),
                a.CategoryId.HasValue ? categoryNamesById.GetValueOrDefault(a.CategoryId.Value) : null)).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    private async Task<Dictionary<Guid, string>> GetCategoryNamesByIdAsync(IEnumerable<Guid?> categoryIds, CancellationToken ct)
    {
        var ids = categoryIds.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (ids.Count == 0) return [];
        var categories = await uow.Repository<JobCategory>().ListAsync(c => ids.Contains(c.Id), ct: ct);
        return categories.ToDictionary(c => c.Id, c => c.Name);
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
        var categoryName = await GetCategoryNameAsync(ad.CategoryId, ct);
        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto(payment, employer?.DisplayName, categoryName));
    }

    public async Task<ServiceResult<JobAdvertisementDto>> UpdateAsync(
        Guid officeId, Guid id, UpdateJobAdvertisementDto dto, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");

        // Reuses the exact same field-application logic as the employer's own create/update path
        // (JobAdvertisementMapping.ApplyFrom) — deliberately no Published -> UnderReview demotion
        // here (this method's own doc comment on the interface explains why). ApplyFrom only ever
        // touches the employer's free-text Category suggestion, never CategoryId — the office's
        // own category assignment is untouched by this action, use AssignCategoryAsync for that.
        ad.ApplyFrom(dto);
        uow.Repository<JobAdvertisement>().Update(ad);
        await uow.SaveChangesAsync(ct);

        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == ad.EmployerId, ct: ct);
        var payment = await uow.Repository<AdvertisementPayment>().FindAsync(p => p.JobAdvertisementId == ad.Id, ct: ct);
        var categoryName = await GetCategoryNameAsync(ad.CategoryId, ct);
        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto(payment, employer?.DisplayName, categoryName));
    }

    public async Task<ServiceResult<List<JobCategoryDto>>> ListCategoriesAsync(CancellationToken ct = default)
    {
        var categories = await uow.Repository<JobCategory>().ListAsync(c => c.IsActive, ct: ct);
        return ServiceResult<List<JobCategoryDto>>.Success(categories
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            .Select(c => new JobCategoryDto { Id = c.Id, Name = c.Name, DisplayOrder = c.DisplayOrder })
            .ToList());
    }

    public async Task<ServiceResult<JobAdvertisementDto>> AssignCategoryAsync(
        Guid officeId, Guid id, AssignJobCategoryDto dto, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");

        Guid categoryId;
        if (dto.ExistingCategoryId.HasValue)
        {
            var existing = await uow.Repository<JobCategory>().FindAsync(c => c.Id == dto.ExistingCategoryId.Value && c.IsActive, ct: ct);
            if (existing is null) return ServiceResult<JobAdvertisementDto>.Invalid("That category doesn't exist.");
            categoryId = existing.Id;
        }
        else if (!string.IsNullOrWhiteSpace(dto.NewCategoryName))
        {
            var name = dto.NewCategoryName.Trim();
            var duplicate = await uow.Repository<JobCategory>().FindAsync(c => c.Name == name, ct: ct);
            if (duplicate is not null)
            {
                categoryId = duplicate.Id; // Already exists (maybe created by another reviewer just now) — reuse it, don't duplicate.
            }
            else
            {
                var existingCategories = await uow.Repository<JobCategory>().ListAsync(ct: ct);
                var nextDisplayOrder = existingCategories.Count == 0 ? 0 : existingCategories.Max(c => c.DisplayOrder) + 1;
                var newCategory = new JobCategory { Id = Guid.NewGuid(), Name = name, DisplayOrder = nextDisplayOrder };
                await uow.Repository<JobCategory>().AddAsync(newCategory, ct);
                categoryId = newCategory.Id;
            }
        }
        else
        {
            return ServiceResult<JobAdvertisementDto>.Invalid("Pick an existing category or name a new one.");
        }

        ad.CategoryId = categoryId;
        uow.Repository<JobAdvertisement>().Update(ad);
        await uow.SaveChangesAsync(ct);

        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == ad.EmployerId, ct: ct);
        var categoryName = await GetCategoryNameAsync(ad.CategoryId, ct);
        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto(employerDisplayName: employer?.DisplayName, categoryName: categoryName));
    }

    private async Task<string?> GetCategoryNameAsync(Guid? categoryId, CancellationToken ct)
    {
        if (!categoryId.HasValue) return null;
        var category = await uow.Repository<JobCategory>().FindAsync(c => c.Id == categoryId.Value, ct: ct);
        return category?.Name;
    }

    public async Task<ServiceResult<JobAdvertisementDto>> ApproveAsync(Guid officeId, Guid reviewerId, Guid id, CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");
        if (ad.Status != JobAdvertisementStatus.UnderReview)
            return ServiceResult<JobAdvertisementDto>.Conflict("The advertisement is not under review.");
        // Confirmed 2026-11-23: the office must assign a real category before approving —
        // JobAdvertisement.CategoryId's doc comment.
        if (ad.CategoryId is null)
            return ServiceResult<JobAdvertisementDto>.Invalid("Assign a category before approving this advertisement.");

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

    /// <summary>peppy-greeting-lighthouse.md's plan, item 4: every job seeker this office approved
    /// whose own DesiredCategoryId matches this now-published ad's category gets an FYI — one way,
    /// no reply expected. Only this office's own approved seekers, same tenant-isolation convention
    /// as everywhere else (JobApplicationService.CreateAsync's own office-approval gate).</summary>
    private async Task NotifyMatchingJobSeekersAsync(JobAdvertisement ad, CancellationToken ct)
    {
        if (ad.CategoryId is null) return;

        var candidates = await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => a.OfficeId == ad.OfficeId && a.ReviewStatus == JobSeekerReviewStatus.Approved && a.EmployedAt == null, ct: ct);
        if (candidates.Count == 0) return;

        var jobSeekerIds = candidates.Select(a => a.JobSeekerId).Distinct().ToList();
        var profiles = await uow.Repository<JobSeekerProfile>().ListAsync(
            p => jobSeekerIds.Contains(p.Id) && p.DesiredCategoryId == ad.CategoryId, ct: ct);

        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == ad.OfficeId, ct: ct);
        var officeName = office?.ArName ?? office?.Name ?? string.Empty;

        foreach (var profile in profiles)
        {
            await notificationService.CreateAsync(
                ad.OfficeId, profile.UserId, "JobAdvertisement.NewInCategory",
                $"في وظيفة جديدة في تخصصك: \"{ad.Title}\" في {officeName}.",
                nameof(JobAdvertisement), ad.Id, ct);
        }
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
        Guid officeId, Guid reviewerId, Guid id, ConfirmAdvertisementPaymentDto dto,
        Stream receiptContent, string receiptFileName, string receiptContentType, long receiptFileSizeBytes,
        CancellationToken ct = default)
    {
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.OfficeId == officeId, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");
        if (ad.Status != JobAdvertisementStatus.ApprovedAwaitingPayment)
            return ServiceResult<JobAdvertisementDto>.Conflict("This advertisement is not awaiting payment.");

        if (!ReceiptAllowedMimeTypes.Contains(receiptContentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<JobAdvertisementDto>.Invalid("نوع الملف غير مدعوم — يُسمح فقط بـ PDF وJPG وPNG.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (receiptFileSizeBytes <= 0 || receiptFileSizeBytes > maxSizeBytes)
            return ServiceResult<JobAdvertisementDto>.Invalid($"حجم الملف كبير جدًا — الحد الأقصى {maxSizeBytes / (1024 * 1024)} ميجابايت.");

        var receiptStorageKey = await fileStorage.SaveAsync(receiptContent, receiptFileName, receiptContentType, ct);

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
            ReceiptStorageKey = receiptStorageKey,
            ReceiptOriginalFileName = receiptFileName,
            ReceiptContentType = receiptContentType,
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

        await NotifyEmployerAsync(ad, "JobAdvertisement.Published", $"تم نشر إعلانك \"{ad.Title}\" بنجاح.", ct);
        await NotifyMatchingJobSeekersAsync(ad, ct);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto(payment));
    }
}
