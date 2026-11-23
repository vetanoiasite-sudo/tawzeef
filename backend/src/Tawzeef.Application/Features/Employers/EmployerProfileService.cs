using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Employers;

public class EmployerProfileService(IUnitOfWork uow, IFileStorage fileStorage, IConfiguration configuration) : IEmployerProfileService
{
    // Same confirmed decision as the job seeker's documents (RBAC_PERMISSIONS.md §0 item 24):
    // PDF/JPG/PNG, 2 MB, configurable.
    private static readonly string[] DefaultAllowedMimeTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // .xlsx
        "application/vnd.ms-excel", // .xls
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
        // reverts it to UnderReview — same rule as §1 for the job seeker.
        RevertApprovalIfNeeded(employer);

        uow.Repository<Employer>().Update(employer);
        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents));
    }

    public async Task<ServiceResult<EmployerProfileDto>> SelectOfficeAsync(Guid userId, Guid officeId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        if (employer.OfficeId is not null)
            return ServiceResult<EmployerProfileDto>.Conflict("تم اختيار مكتب التوظيف بالفعل.");

        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == officeId && o.Status == OfficeStatus.Active, ct: ct);
        if (office is null) return ServiceResult<EmployerProfileDto>.Invalid("اختر مكتب توظيف صالح.");

        employer.OfficeId = office.Id;
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
        if (employer.OfficeId is null)
            return ServiceResult<EmployerDocumentDto>.Invalid("اختر مكتب التوظيف الذي ستتقدم إليه أولًا.");

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
            OfficeId = employer.OfficeId.Value,
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
        RevertApprovalIfNeeded(employer);
        uow.Repository<Employer>().Update(employer);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerDocumentDto>.Success(document.ToDto());
    }

    public async Task<ServiceResult<EmployerProfileDto>> SubmitForReviewAsync(Guid userId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("لا يوجد حساب صاحب عمل لهذا المستخدم.");

        if (employer.OfficeId is null)
            return ServiceResult<EmployerProfileDto>.Invalid("اختر مكتب التوظيف الذي ستتقدم إليه أولًا.");

        if (employer.VerificationStatus != EmployerVerificationStatus.Draft && employer.VerificationStatus != EmployerVerificationStatus.Rejected)
            return ServiceResult<EmployerProfileDto>.Conflict("لا يمكن تقديم ملف الشركة من حالته الحالية.");

        var activeDocuments = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        var presentTypes = activeDocuments.Select(d => d.DocumentType).ToHashSet();
        var missing = Enum.GetValues<EmployerDocumentType>().Where(t => !presentTypes.Contains(t)).ToList();
        if (missing.Count > 0)
            return ServiceResult<EmployerProfileDto>.Invalid($"ارفع كل المستندات المطلوبة أولًا — الناقص: {string.Join(", ", missing)}.");

        employer.VerificationStatus = EmployerVerificationStatus.Submitted;
        employer.RejectionReason = null; // Rejected -> Submitted clears the reason, same as §1.
        uow.Repository<Employer>().Update(employer);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(activeDocuments));
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

    private static void RevertApprovalIfNeeded(Employer employer)
    {
        if (employer.VerificationStatus == EmployerVerificationStatus.Approved)
            employer.VerificationStatus = EmployerVerificationStatus.UnderReview;
    }
}
