using Tawzeef.Application.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Employers;

public interface IEmployerProfileService
{
    Task<ServiceResult<EmployerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateEmployerProfileDto dto, CancellationToken ct = default);

    /// <summary>The employer's one-time choice of which office will review their profile —
    /// registration no longer collects this (RegisterEmployerDto's doc comment). Fails once
    /// already set; an office can't be changed through self-service after that.</summary>
    Task<ServiceResult<EmployerProfileDto>> SelectOfficeAsync(Guid userId, Guid officeId, CancellationToken ct = default);

    Task<ServiceResult<EmployerDocumentDto>> UploadDocumentAsync(
        Guid userId,
        EmployerDocumentType documentType,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> SubmitForReviewAsync(Guid userId, CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> UploadLogoAsync(
        Guid userId, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default);

    /// <summary>Public, no auth — the one file this app serves to an anonymous caller. Null if the
    /// employer has no logo, or doesn't exist.</summary>
    Task<EmployerDocumentFileDto?> GetLogoFileAsync(Guid employerId, CancellationToken ct = default);
}
