using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Employers;

public class EmployerProfileService(IUnitOfWork uow, IFileStorage fileStorage, IConfiguration configuration) : IEmployerProfileService
{
    // Same confirmed decision as the job seeker's documents (RBAC_PERMISSIONS.md §0 item 24):
    // PDF/JPG/PNG, 2 MB, configurable.
    private static readonly string[] DefaultAllowedMimeTypes = ["application/pdf", "image/jpeg", "image/png"];
    private const long DefaultMaxFileSizeBytes = 2 * 1024 * 1024;

    public async Task<ServiceResult<EmployerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("No employer account for this user.");

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents));
    }

    public async Task<ServiceResult<EmployerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateEmployerProfileDto dto, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("No employer account for this user.");

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
        if (employer is null) return ServiceResult<EmployerDocumentDto>.NotFound("No employer account for this user.");

        var allowedMimeTypes = configuration.GetSection("Uploads:AllowedMimeTypes").Get<string[]>() ?? DefaultAllowedMimeTypes;
        if (!allowedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<EmployerDocumentDto>.Invalid("Unsupported file type — only PDF, JPG and PNG are allowed.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<EmployerDocumentDto>.Invalid($"File is too large — the limit is {maxSizeBytes / (1024 * 1024)} MB.");

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
            OfficeId = employer.OfficeId,
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
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("No employer account for this user.");

        if (employer.VerificationStatus != EmployerVerificationStatus.Draft && employer.VerificationStatus != EmployerVerificationStatus.Rejected)
            return ServiceResult<EmployerProfileDto>.Conflict("The company profile cannot be submitted from its current status.");

        var activeDocuments = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        var presentTypes = activeDocuments.Select(d => d.DocumentType).ToHashSet();
        var missing = Enum.GetValues<EmployerDocumentType>().Where(t => !presentTypes.Contains(t)).ToList();
        if (missing.Count > 0)
            return ServiceResult<EmployerProfileDto>.Invalid($"Upload every required document first — missing: {string.Join(", ", missing)}.");

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
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("No employer account for this user.");

        if (!LogoAllowedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<EmployerProfileDto>.Invalid("Unsupported file type — only JPG and PNG are allowed.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<EmployerProfileDto>.Invalid($"File is too large — the limit is {maxSizeBytes / (1024 * 1024)} MB.");

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
