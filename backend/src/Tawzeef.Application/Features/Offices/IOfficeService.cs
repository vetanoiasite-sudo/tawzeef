using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Offices;

public interface IOfficeService
{
    Task<ServiceResult<IReadOnlyList<OfficeDto>>> ListAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<PublicOfficeDto>>> ListPublicAsync(CancellationToken ct = default);

    /// <summary>The office's own "my office profile" GET — same shape as the SuperAdmin's, scoped
    /// by the caller (controller passes currentUser.OfficeId, never a client-supplied id).</summary>
    Task<ServiceResult<OfficeDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Anonymous "view this office's profile" — same anonymity rule as a paused course or
    /// job ad (API_DOCUMENTATION.md §3.7's pattern): an Inactive office's id confirms nothing to
    /// an anonymous caller, plain NotFound either way.</summary>
    Task<ServiceResult<OfficeDto>> GetPublicByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<OfficeDto>> CreateAsync(CreateOfficeDto dto, CancellationToken ct = default);
    Task<ServiceResult<OfficeDto>> UpdateAsync(Guid id, UpdateOfficeDto dto, CancellationToken ct = default);
    Task<ServiceResult<OfficeDto>> ActivateAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<OfficeDto>> DeactivateAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<OfficePlatformStatsDto>> GetStatsAsync(CancellationToken ct = default);

    Task<ServiceResult<OfficeDto>> UploadLogoAsync(
        Guid officeId, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default);

    Task<OfficeLogoFileDto?> GetLogoFileAsync(Guid officeId, CancellationToken ct = default);
}
