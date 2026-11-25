using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Employers;

public class EmployerProfileService(IUnitOfWork uow, IFileStorage fileStorage, IConfiguration configuration) : IEmployerProfileService
{
    // Same confirmed decision as the job seeker's documents (RBAC_PERMISSIONS.md §0 item 24):
    // PDF/JPG/PNG, 2 MB, configurable. Excel added 2026-11-23, Word added 2026-11-24.
    private static readonly string[] DefaultAllowedMimeTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
        "application/vnd.ms-excel", // .xls
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // .docx
        "application/msword", // .doc
    ];
    private const long DefaultMaxFileSizeBytes = 2 * 1024 * 1024;

    public async Task<ServiceResult<EmployerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents));
    }

    public async Task<ServiceResult<EmployerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateEmployerProfileDto dto, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        employer.BusinessActivity = dto.BusinessActivity?.Trim();
        employer.Address = dto.Address?.Trim();
        employer.City = dto.City?.Trim();
        employer.CompanyEmail = dto.CompanyEmail?.Trim();
        employer.CompanyPhone = dto.CompanyPhone?.Trim();
        employer.RepresentativeName = dto.RepresentativeName?.Trim();
        employer.RepresentativeRole = dto.RepresentativeRole?.Trim();

        // WORKFLOWS_AND_STATE_MACHINES.md §3: editing an Approved company profile automatically
        // reverts it to UnderReview — same rule as §1 for the job seeker. Documents are shared
        // across every office applied to (confirmed 2026-11-24), so this reverts every one of this
        // employer's Approved office applications, not just one.
        await RevertApprovalIfNeededAsync(employer.Id, ct);

        uow.Repository<Employer>().Update(employer);
        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents));
    }

    public async Task<ServiceResult<EmployerDocumentDto>> UploadDocumentAsync(
        Guid userId,
        EmployerDocumentType documentType,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerDocumentDto>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        var allowedMimeTypes = configuration.GetSection("Uploads:AllowedMimeTypes").Get<string[]>() ?? DefaultAllowedMimeTypes;
        if (!allowedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<EmployerDocumentDto>.Invalid("نوع الملف غير مدعوم — يُسمح فقط بـ PDF وJPG وPNG.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<EmployerDocumentDto>.Invalid($"حجم الملف كبير جدًا — الحد الأقصى {maxSizeBytes / (1024 * 1024)} ميجابايت.");

        var previousActive = await uow.Repository<EmployerDocument>().ListAsync(
            d => d.EmployerId == employer.Id && d.DocumentType == documentType && d.IsActive, ct: ct);
        foreach (var old in previousActive)
        {
            old.IsActive = false;
            uow.Repository<EmployerDocument>().Update(old);
        }

        var storageKey = await fileStorage.SaveAsync(content, originalFileName, contentType, ct);
        var document = new EmployerDocument
        {
            Id = Guid.NewGuid(),
            EmployerId = employer.Id,
            DocumentType = documentType,
            SecureFileReference = storageKey,
            OriginalFileName = originalFileName,
            MimeType = contentType,
            FileSizeBytes = (int)fileSizeBytes,
            ReviewStatus = DocumentReviewStatus.Pending,
        };
        await uow.Repository<EmployerDocument>().AddAsync(document, ct);

        // Same automatic revert as a profile edit (WORKFLOWS_AND_STATE_MACHINES.md §3).
        await RevertApprovalIfNeededAsync(employer.Id, ct);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerDocumentDto>.Success(document.ToDto());
    }

    /// <summary>Lets the employer view/download their own already-uploaded document — the upload
    /// endpoint only ever returns metadata, never the file itself back.</summary>
    public async Task<ServiceResult<EmployerDocumentFileDto>> GetDocumentFileAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerDocumentFileDto>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        var document = await uow.Repository<EmployerDocument>().FindAsync(
            d => d.Id == documentId && d.EmployerId == employer.Id && d.IsActive, ct: ct);
        if (document is null) return ServiceResult<EmployerDocumentFileDto>.NotFound("المستند غير موجود.");

        var stream = await fileStorage.OpenReadAsync(document.SecureFileReference, ct);
        return ServiceResult<EmployerDocumentFileDto>.Success(new EmployerDocumentFileDto
        {
            Content = stream,
            FileName = document.OriginalFileName,
            ContentType = document.MimeType,
        });
    }

    public async Task<ServiceResult<EmployerProfileDto>> SubmitForReviewAsync(Guid userId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        var activeDocuments = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        var presentTypes = activeDocuments.Select(d => d.DocumentType).ToHashSet();
        var missing = Enum.GetValues<EmployerDocumentType>().Where(t => !presentTypes.Contains(t)).ToList();
        if (missing.Count > 0)
            return ServiceResult<EmployerProfileDto>.Invalid($"ارفع كل المستندات المطلوبة أولًا — الناقص: {string.Join(", ", missing)}.");

        // Confirmed 2026-11-24: documents are shared, so this one action pushes every office this
        // employer is still Draft/Rejected at into Submitted, instead of a single VerificationStatus
        // field. Also covers "no office chosen yet" (nothing Draft to push forward).
        var pending = await uow.Repository<EmployerOfficeApplication>().ListAsync(
            a => a.EmployerId == employer.Id && (a.VerificationStatus == EmployerVerificationStatus.Draft || a.VerificationStatus == EmployerVerificationStatus.Rejected), ct: ct);
        if (pending.Count == 0)
            return ServiceResult<EmployerProfileDto>.Invalid("اختر مكتب التوظيف الذي ستتقدم إليه أولًا.");

        foreach (var application in pending)
        {
            application.VerificationStatus = EmployerVerificationStatus.Submitted;
            application.RejectionReason = null; // Rejected -> Submitted clears the reason, same as §1.
            uow.Repository<EmployerOfficeApplication>().Update(application);
        }
        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(activeDocuments));
    }

    public async Task<ServiceResult<List<EmployerOfficeStatusDto>>> GetMyOfficeApplicationsAsync(Guid userId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<List<EmployerOfficeStatusDto>>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        return ServiceResult<List<EmployerOfficeStatusDto>>.Success(await BuildOfficeStatusListAsync(employer.Id, ct));
    }

    public async Task<ServiceResult<List<EmployerOfficeStatusDto>>> SubmitToOfficesAsync(Guid userId, SubmitEmployerOfficesDto dto, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<List<EmployerOfficeStatusDto>>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        var error = await AddOfficeApplicationsAsync(employer.Id, dto.OfficeIds, dto.AllOffices, ct);
        if (error is not null) return ServiceResult<List<EmployerOfficeStatusDto>>.Invalid(error);

        await uow.SaveChangesAsync(ct);
        return ServiceResult<List<EmployerOfficeStatusDto>>.Success(await BuildOfficeStatusListAsync(employer.Id, ct));
    }

    private static readonly string[] LogoAllowedMimeTypes = ["image/jpeg", "image/png"];

    public async Task<ServiceResult<EmployerProfileDto>> UploadLogoAsync(
        Guid userId, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        if (!LogoAllowedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<EmployerProfileDto>.Invalid("نوع الملف غير مدعوم — يُسمح فقط بـ JPG وPNG.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<EmployerProfileDto>.Invalid($"حجم الملف كبير جدًا — الحد الأقصى {maxSizeBytes / (1024 * 1024)} ميجابايت.");

        var previousStorageKey = employer.LogoStorageKey;

        employer.LogoStorageKey = await fileStorage.SaveAsync(content, originalFileName, contentType, ct);
        employer.LogoOriginalFileName = originalFileName;
        employer.LogoContentType = contentType;
        uow.Repository<Employer>().Update(employer);
        await uow.SaveChangesAsync(ct);

        // Only delete the old file once the new one is safely saved and committed — never leave a
        // gap where a crash mid-upload loses the only logo on file.
        if (previousStorageKey is not null)
            await fileStorage.DeleteAsync(previousStorageKey, ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents));
    }

    public async Task<EmployerDocumentFileDto?> GetLogoFileAsync(Guid employerId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == employerId, ct: ct);
        if (employer?.LogoStorageKey is null) return null;

        var content = await fileStorage.OpenReadAsync(employer.LogoStorageKey, ct);
        return new EmployerDocumentFileDto
        {
            Content = content,
            FileName = employer.LogoOriginalFileName ?? "logo",
            ContentType = employer.LogoContentType ?? "application/octet-stream",
        };
    }

    public async Task<ServiceResult<PagedResult<PublicEmployerListItemDto>>> SearchCooperatingAsync(PublicEmployerQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var cooperatingEmployerIds = await GetCooperatingEmployerIdsAsync(ct);
        if (cooperatingEmployerIds.Count == 0)
            return ServiceResult<PagedResult<PublicEmployerListItemDto>>.Success(new PagedResult<PublicEmployerListItemDto>
            {
                Items = [],
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = 0,
            });

        var page = await uow.Repository<Employer>().PageAsync(
            pageIndex, pageSize, e => cooperatingEmployerIds.Contains(e.Id),
            orderBy: e => e.DisplayName, ct: ct);

        return ServiceResult<PagedResult<PublicEmployerListItemDto>>.Success(new PagedResult<PublicEmployerListItemDto>
        {
            Items = page.Items.Select(ToPublicListItemDto).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<PublicEmployerDetailDto>> GetPublicCooperatingByIdAsync(Guid id, CancellationToken ct = default)
    {
        var cooperatingEmployerIds = await GetCooperatingEmployerIdsAsync(ct);
        if (!cooperatingEmployerIds.Contains(id))
            return ServiceResult<PublicEmployerDetailDto>.NotFound("Company not found.");

        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == id, ct: ct);
        if (employer is null) return ServiceResult<PublicEmployerDetailDto>.NotFound("Company not found.");

        return ServiceResult<PublicEmployerDetailDto>.Success(new PublicEmployerDetailDto
        {
            Id = employer.Id,
            DisplayName = employer.DisplayName,
            City = employer.City,
            BusinessActivity = employer.BusinessActivity,
            LogoUrl = employer.LogoStorageKey is null ? null : $"/api/employers/{employer.Id}/logo",
        });
    }

    private async Task<HashSet<Guid>> GetCooperatingEmployerIdsAsync(CancellationToken ct) =>
        (await uow.Repository<EmployerOfficeApplication>().ListAsync(
            a => a.VerificationStatus == EmployerVerificationStatus.Approved && a.CooperationStatus == EmployerCooperationStatus.Active,
            ct: ct))
            .Select(a => a.EmployerId)
            .ToHashSet();

    private static PublicEmployerListItemDto ToPublicListItemDto(Employer e) => new()
    {
        Id = e.Id,
        DisplayName = e.DisplayName,
        City = e.City,
        LogoUrl = e.LogoStorageKey is null ? null : $"/api/employers/{e.Id}/logo",
    };

    /// <summary>Shared by SubmitToOfficesAsync — the employer's old one-time SelectOfficeAsync, now
    /// repeatable. Silently skips any office already applied to.</summary>
    private async Task<string?> AddOfficeApplicationsAsync(Guid employerId, List<Guid> officeIds, bool allOffices, CancellationToken ct)
    {
        var offices = allOffices
            ? await uow.Repository<EmploymentOffice>().ListAsync(o => o.Status == OfficeStatus.Active, ct: ct)
            : await uow.Repository<EmploymentOffice>().ListAsync(o => officeIds.Contains(o.Id) && o.Status == OfficeStatus.Active, ct: ct);
        if (offices.Count == 0) return "اختر مكتب توظيف صالح واحدًا على الأقل.";

        var existingOfficeIds = (await uow.Repository<EmployerOfficeApplication>().ListAsync(a => a.EmployerId == employerId, ct: ct))
            .Select(a => a.OfficeId).ToHashSet();
        var readyForReview = await IsReadyForReviewAsync(employerId, ct);

        foreach (var office in offices)
        {
            if (existingOfficeIds.Contains(office.Id)) continue;
            await uow.Repository<EmployerOfficeApplication>().AddAsync(new EmployerOfficeApplication
            {
                Id = Guid.NewGuid(),
                EmployerId = employerId,
                OfficeId = office.Id,
                VerificationStatus = readyForReview ? EmployerVerificationStatus.Submitted : EmployerVerificationStatus.Draft,
            }, ct);
        }
        return null;
    }

    private async Task<bool> IsReadyForReviewAsync(Guid employerId, CancellationToken ct)
    {
        var presentTypes = (await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employerId && d.IsActive, ct: ct))
            .Select(d => d.DocumentType).ToHashSet();
        return Enum.GetValues<EmployerDocumentType>().All(presentTypes.Contains);
    }

    private async Task<List<EmployerOfficeStatusDto>> BuildOfficeStatusListAsync(Guid employerId, CancellationToken ct)
    {
        var applications = await uow.Repository<EmployerOfficeApplication>().ListAsync(a => a.EmployerId == employerId, ct: ct);
        var appliedOfficeIds = applications.Select(a => a.OfficeId).ToHashSet();

        var offices = await uow.Repository<EmploymentOffice>().ListAsync(
            o => o.Status == OfficeStatus.Active || appliedOfficeIds.Contains(o.Id), ct: ct);
        var applicationsByOffice = applications.ToDictionary(a => a.OfficeId);

        return offices
            .Select(o => applicationsByOffice.TryGetValue(o.Id, out var a)
                ? a.ToDto(o)
                : new EmployerOfficeStatusDto { OfficeId = o.Id, OfficeName = o.ArName ?? o.Name, City = o.City, VerificationStatus = "NotSubmitted" })
            .OrderBy(x => x.OfficeName)
            .ToList();
    }

    private async Task RevertApprovalIfNeededAsync(Guid employerId, CancellationToken ct)
    {
        var approved = await uow.Repository<EmployerOfficeApplication>().ListAsync(
            a => a.EmployerId == employerId && a.VerificationStatus == EmployerVerificationStatus.Approved, ct: ct);
        foreach (var application in approved)
        {
            application.VerificationStatus = EmployerVerificationStatus.UnderReview;
            uow.Repository<EmployerOfficeApplication>().Update(application);
        }
    }
}
