using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

public class JobSeekerProfileService(IUnitOfWork uow, IFileStorage fileStorage, IConfiguration configuration, ICvParser cvParser) : IJobSeekerProfileService
{
    // BUSINESS_REQUIREMENTS.md §5.1 Step 4 — PDF/JPG/PNG, confirmed 2026-11-18, size limit lowered
    // to 2 MB 2026-11-23, configurable either way. Excel added 2026-11-23, Word added 2026-11-24.
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

    public async Task<ServiceResult<JobSeekerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateJobSeekerProfileDto dto, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        profile.Email = dto.Email?.Trim();
        profile.Address = dto.Address?.Trim();
        profile.City = dto.City?.Trim();
        profile.DateOfBirth = dto.DateOfBirth;
        profile.Education = dto.Education?.Trim();
        profile.YearsOfExperience = dto.YearsOfExperience;
        profile.Skills = dto.Skills?.Trim();
        profile.CareerSummary = dto.CareerSummary?.Trim();
        profile.JobTitle = dto.JobTitle?.Trim();

        // WORKFLOWS_AND_STATE_MACHINES.md §1: editing an Approved profile automatically reverts it
        // to UnderReview — not a client choice. Documents are shared across every office the job
        // seeker applied to (confirmed 2026-11-24), so this reverts every one of their Approved
        // office applications, not just one.
        await RevertApprovalIfNeededAsync(profile.Id, ct);

        uow.Repository<JobSeekerProfile>().Update(profile);
        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents));
    }

    public async Task<ServiceResult<JobSeekerDocumentDto>> UploadDocumentAsync(
        Guid userId,
        JobSeekerDocumentType documentType,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerDocumentDto>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        var allowedMimeTypes = configuration.GetSection("Uploads:AllowedMimeTypes").Get<string[]>() ?? DefaultAllowedMimeTypes;
        if (!allowedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<JobSeekerDocumentDto>.Invalid("نوع الملف غير مدعوم — يُسمح فقط بـ PDF وJPG وPNG.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<JobSeekerDocumentDto>.Invalid($"حجم الملف كبير جدًا — الحد الأقصى {maxSizeBytes / (1024 * 1024)} ميجابايت.");

        var previousActive = await uow.Repository<JobSeekerDocument>().ListAsync(
            d => d.JobSeekerId == profile.Id && d.DocumentType == documentType && d.IsActive, ct: ct);
        foreach (var old in previousActive)
        {
            old.IsActive = false;
            uow.Repository<JobSeekerDocument>().Update(old);
        }

        // Buffered once so the same bytes can both be saved and (for a Cv) parsed for suggestions
        // — the original stream may not be seekable/re-readable a second time.
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);
        var fileBytes = buffer.ToArray();

        var storageKey = await fileStorage.SaveAsync(new MemoryStream(fileBytes), originalFileName, contentType, ct);
        var document = new JobSeekerDocument
        {
            Id = Guid.NewGuid(),
            JobSeekerId = profile.Id,
            DocumentType = documentType,
            SecureFileReference = storageKey,
            OriginalFileName = originalFileName,
            MimeType = contentType,
            FileSizeBytes = (int)fileSizeBytes,
            ReviewStatus = DocumentReviewStatus.Pending,
        };
        await uow.Repository<JobSeekerDocument>().AddAsync(document, ct);

        // Same automatic revert as a profile edit (WORKFLOWS_AND_STATE_MACHINES.md §1).
        await RevertApprovalIfNeededAsync(profile.Id, ct);

        await uow.SaveChangesAsync(ct);

        var dto = document.ToDto();
        if (documentType == JobSeekerDocumentType.Cv)
        {
            // Suggestions only — never written to the profile here. The job seeker reviews/edits
            // them in the profile form and saves explicitly via UpdateMyProfileAsync.
            var suggestion = await cvParser.TryParseAsync(fileBytes, contentType, ct);
            dto.SuggestedJobTitle = suggestion?.SuggestedJobTitle;
            dto.SuggestedYearsOfExperience = suggestion?.SuggestedYearsOfExperience;
        }

        return ServiceResult<JobSeekerDocumentDto>.Success(dto);
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> SubmitForReviewAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        var activeDocuments = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        var presentTypes = activeDocuments.Select(d => d.DocumentType).ToHashSet();
        var missing = Enum.GetValues<JobSeekerDocumentType>().Where(t => !presentTypes.Contains(t)).ToList();
        if (missing.Count > 0)
            return ServiceResult<JobSeekerProfileDto>.Invalid($"ارفع كل المستندات المطلوبة أولًا — الناقص: {string.Join(", ", missing)}.");

        // Confirmed 2026-11-24: documents are shared, so this one action pushes every office this
        // job seeker is still Draft/Rejected at into Submitted, instead of a single profile field.
        var pending = await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => a.JobSeekerId == profile.Id && (a.ReviewStatus == JobSeekerReviewStatus.Draft || a.ReviewStatus == JobSeekerReviewStatus.Rejected), ct: ct);
        if (pending.Count == 0)
            return ServiceResult<JobSeekerProfileDto>.Conflict("لا يوجد مكتب في انتظار التقديم حاليًا.");

        foreach (var application in pending)
        {
            application.ReviewStatus = JobSeekerReviewStatus.Submitted;
            application.RejectionReason = null; // WORKFLOWS_AND_STATE_MACHINES.md §1: Rejected -> Submitted clears the reason.
            uow.Repository<JobSeekerOfficeApplication>().Update(application);
        }
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(activeDocuments));
    }

    public async Task<ServiceResult<List<JobSeekerOfficeStatusDto>>> GetMyOfficeApplicationsAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<List<JobSeekerOfficeStatusDto>>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        return ServiceResult<List<JobSeekerOfficeStatusDto>>.Success(await BuildOfficeStatusListAsync(profile.Id, ct));
    }

    public async Task<ServiceResult<List<JobSeekerOfficeStatusDto>>> SubmitToOfficesAsync(Guid userId, SubmitJobSeekerOfficesDto dto, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<List<JobSeekerOfficeStatusDto>>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        var error = await AddOfficeApplicationsAsync(profile.Id, dto.OfficeIds, dto.AllOffices, ct);
        if (error is not null) return ServiceResult<List<JobSeekerOfficeStatusDto>>.Invalid(error);

        await uow.SaveChangesAsync(ct);
        return ServiceResult<List<JobSeekerOfficeStatusDto>>.Success(await BuildOfficeStatusListAsync(profile.Id, ct));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> UploadPhotoAsync(
        Guid userId, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        string[] allowedPhotoMimeTypes = ["image/jpeg", "image/png"];
        if (!allowedPhotoMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<JobSeekerProfileDto>.Invalid("نوع الملف غير مدعوم — يُسمح فقط بـ JPG وPNG للصورة الشخصية.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<JobSeekerProfileDto>.Invalid($"حجم الملف كبير جدًا — الحد الأقصى {maxSizeBytes / (1024 * 1024)} ميجابايت.");

        var previousStorageKey = profile.PhotoStorageKey;

        var storageKey = await fileStorage.SaveAsync(content, originalFileName, contentType, ct);
        profile.PhotoStorageKey = storageKey;
        profile.PhotoOriginalFileName = originalFileName;
        profile.PhotoContentType = contentType;
        uow.Repository<JobSeekerProfile>().Update(profile);
        await uow.SaveChangesAsync(ct);

        if (previousStorageKey is not null)
            await fileStorage.DeleteAsync(previousStorageKey, ct);

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents));
    }

    public async Task<ServiceResult<JobSeekerDocumentFileDto>> GetPhotoAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerDocumentFileDto>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");
        if (profile.PhotoStorageKey is null) return ServiceResult<JobSeekerDocumentFileDto>.NotFound("لا توجد صورة محفوظة.");

        var stream = await fileStorage.OpenReadAsync(profile.PhotoStorageKey, ct);
        return ServiceResult<JobSeekerDocumentFileDto>.Success(new JobSeekerDocumentFileDto
        {
            Content = stream,
            FileName = profile.PhotoOriginalFileName ?? "photo",
            ContentType = profile.PhotoContentType ?? "application/octet-stream",
        });
    }

    /// <summary>Shared by SubmitToOfficesAsync and AuthService.RegisterJobSeekerAsync (the latter
    /// inlines the same shape instead of calling this, since the profile row isn't saved yet at
    /// that point). Silently skips any office already applied to — repeatable, not a one-time
    /// action like the old SelectOfficeAsync.</summary>
    private async Task<string?> AddOfficeApplicationsAsync(Guid jobSeekerId, List<Guid> officeIds, bool allOffices, CancellationToken ct)
    {
        var offices = allOffices
            ? await uow.Repository<EmploymentOffice>().ListAsync(o => o.Status == OfficeStatus.Active, ct: ct)
            : await uow.Repository<EmploymentOffice>().ListAsync(o => officeIds.Contains(o.Id) && o.Status == OfficeStatus.Active, ct: ct);
        if (offices.Count == 0) return "اختر مكتب توظيف صالح واحدًا على الأقل.";

        var existingOfficeIds = (await uow.Repository<JobSeekerOfficeApplication>().ListAsync(a => a.JobSeekerId == jobSeekerId, ct: ct))
            .Select(a => a.OfficeId).ToHashSet();
        var readyForReview = await IsReadyForReviewAsync(jobSeekerId, ct);

        foreach (var office in offices)
        {
            if (existingOfficeIds.Contains(office.Id)) continue;
            await uow.Repository<JobSeekerOfficeApplication>().AddAsync(new JobSeekerOfficeApplication
            {
                Id = Guid.NewGuid(),
                JobSeekerId = jobSeekerId,
                OfficeId = office.Id,
                ReviewStatus = readyForReview ? JobSeekerReviewStatus.Submitted : JobSeekerReviewStatus.Draft,
            }, ct);
        }
        return null;
    }

    private async Task<bool> IsReadyForReviewAsync(Guid jobSeekerId, CancellationToken ct)
    {
        var presentTypes = (await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == jobSeekerId && d.IsActive, ct: ct))
            .Select(d => d.DocumentType).ToHashSet();
        return Enum.GetValues<JobSeekerDocumentType>().All(presentTypes.Contains);
    }

    private async Task<List<JobSeekerOfficeStatusDto>> BuildOfficeStatusListAsync(Guid jobSeekerId, CancellationToken ct)
    {
        var applications = await uow.Repository<JobSeekerOfficeApplication>().ListAsync(a => a.JobSeekerId == jobSeekerId, ct: ct);
        var appliedOfficeIds = applications.Select(a => a.OfficeId).ToHashSet();

        // Every currently-active office, plus any office already applied to even if it later
        // became inactive — history stays visible instead of silently disappearing.
        var offices = await uow.Repository<EmploymentOffice>().ListAsync(
            o => o.Status == OfficeStatus.Active || appliedOfficeIds.Contains(o.Id), ct: ct);
        var applicationsByOffice = applications.ToDictionary(a => a.OfficeId);

        return offices
            .Select(o => applicationsByOffice.TryGetValue(o.Id, out var a)
                ? a.ToDto(o)
                : new JobSeekerOfficeStatusDto { OfficeId = o.Id, OfficeName = o.ArName ?? o.Name, City = o.City, ReviewStatus = "NotSubmitted" })
            .OrderBy(x => x.OfficeName)
            .ToList();
    }

    private async Task RevertApprovalIfNeededAsync(Guid jobSeekerId, CancellationToken ct)
    {
        var approved = await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => a.JobSeekerId == jobSeekerId && a.ReviewStatus == JobSeekerReviewStatus.Approved, ct: ct);
        foreach (var application in approved)
        {
            application.ReviewStatus = JobSeekerReviewStatus.UnderReview;
            uow.Repository<JobSeekerOfficeApplication>().Update(application);
        }
    }
}
