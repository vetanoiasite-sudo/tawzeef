using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

public class JobSeekerProfileService(IUnitOfWork uow, IFileStorage fileStorage, IConfiguration configuration, ICvParser cvParser) : IJobSeekerProfileService
{
    // BUSINESS_REQUIREMENTS.md §5.1 Step 4 — PDF/JPG/PNG, confirmed 2026-11-18, size limit lowered
    // to 2 MB 2026-11-23, configurable either way.
    private static readonly string[] DefaultAllowedMimeTypes = ["application/pdf", "image/jpeg", "image/png"];
    private const long DefaultMaxFileSizeBytes = 2 * 1024 * 1024;

    public async Task<ServiceResult<JobSeekerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("No job seeker profile for this account.");

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateJobSeekerProfileDto dto, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("No job seeker profile for this account.");

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
        // to UnderReview — not a client choice.
        RevertApprovalIfNeeded(profile);

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
        if (profile is null) return ServiceResult<JobSeekerDocumentDto>.NotFound("No job seeker profile for this account.");

        var allowedMimeTypes = configuration.GetSection("Uploads:AllowedMimeTypes").Get<string[]>() ?? DefaultAllowedMimeTypes;
        if (!allowedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<JobSeekerDocumentDto>.Invalid("Unsupported file type — only PDF, JPG and PNG are allowed.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<JobSeekerDocumentDto>.Invalid($"File is too large — the limit is {maxSizeBytes / (1024 * 1024)} MB.");

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
            OfficeId = profile.OfficeId,
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
        RevertApprovalIfNeeded(profile);
        uow.Repository<JobSeekerProfile>().Update(profile);

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
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("No job seeker profile for this account.");

        if (profile.ReviewStatus != JobSeekerReviewStatus.Draft && profile.ReviewStatus != JobSeekerReviewStatus.Rejected)
            return ServiceResult<JobSeekerProfileDto>.Conflict("The profile cannot be submitted from its current status.");

        var activeDocuments = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        var presentTypes = activeDocuments.Select(d => d.DocumentType).ToHashSet();
        var missing = Enum.GetValues<JobSeekerDocumentType>().Where(t => !presentTypes.Contains(t)).ToList();
        if (missing.Count > 0)
            return ServiceResult<JobSeekerProfileDto>.Invalid($"Upload every required document first — missing: {string.Join(", ", missing)}.");

        profile.ReviewStatus = JobSeekerReviewStatus.Submitted;
        profile.RejectionReason = null; // WORKFLOWS_AND_STATE_MACHINES.md §1: Rejected -> Submitted clears the reason.
        uow.Repository<JobSeekerProfile>().Update(profile);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(activeDocuments));
    }

    private static void RevertApprovalIfNeeded(JobSeekerProfile profile)
    {
        if (profile.ReviewStatus == JobSeekerReviewStatus.Approved)
            profile.ReviewStatus = JobSeekerReviewStatus.UnderReview;
    }
}
