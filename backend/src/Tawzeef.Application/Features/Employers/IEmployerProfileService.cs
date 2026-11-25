using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Employers;

public interface IEmployerProfileService
{
    Task<ServiceResult<EmployerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateEmployerProfileDto dto, CancellationToken ct = default);

    Task<ServiceResult<EmployerDocumentDto>> UploadDocumentAsync(
        Guid userId,
        EmployerDocumentType documentType,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken ct = default);

    /// <summary>Lets the employer view/download their own already-uploaded document.</summary>
    Task<ServiceResult<EmployerDocumentFileDto>> GetDocumentFileAsync(Guid userId, Guid documentId, CancellationToken ct = default);

    /// <summary>Requires every required document type to already be uploaded, and at least one
    /// office chosen (SubmitToOfficesAsync) — advances every one of this employer's currently
    /// Draft/Rejected office applications to Submitted at once, same shared-documents design as
    /// JobSeekerProfileService.SubmitForReviewAsync.</summary>
    Task<ServiceResult<EmployerProfileDto>> SubmitForReviewAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Every office (active or not), each paired with this employer's own status there —
    /// "NotSubmitted" when no application row exists yet for that office.</summary>
    Task<ServiceResult<List<EmployerOfficeStatusDto>>> GetMyOfficeApplicationsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>The employer's old one-time SelectOfficeAsync, now repeatable and multi-office —
    /// submits to specific offices, or every currently-active office, skipping any already applied
    /// to. A new row starts Submitted instead of Draft when the required documents are already all
    /// uploaded (SubmitForReviewAsync's own gate already satisfied).</summary>
    Task<ServiceResult<List<EmployerOfficeStatusDto>>> SubmitToOfficesAsync(Guid userId, SubmitEmployerOfficesDto dto, CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> UploadLogoAsync(
        Guid userId, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default);

    /// <summary>Public, no auth — the one file this app serves to an anonymous caller. Null if the
    /// employer has no logo, or doesn't exist.</summary>
    Task<EmployerDocumentFileDto?> GetLogoFileAsync(Guid employerId, CancellationToken ct = default);

    /// <summary>The home page's "cooperating companies" showcase, and its "view all" page —
    /// public, no auth. Approved + Active cooperation with at least one office only
    /// (2026-11-25).</summary>
    Task<ServiceResult<PagedResult<PublicEmployerListItemDto>>> SearchCooperatingAsync(PublicEmployerQueryDto query, CancellationToken ct = default);

    /// <summary>The public "view this company's profile" page — public, no auth. Same anonymity
    /// rule as a paused job ad: a non-cooperating employer's id confirms nothing (2026-11-25).</summary>
    Task<ServiceResult<PublicEmployerDetailDto>> GetPublicCooperatingByIdAsync(Guid id, CancellationToken ct = default);
}
