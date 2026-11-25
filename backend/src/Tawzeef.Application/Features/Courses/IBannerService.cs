using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Courses;

/// <summary>Office-authored home-page banners — image only, no review workflow, same gatekeeping
/// model the earlier office-authored Course concept used (replaced 2026-11-25 by the job seeker's
/// own course history, which has no office-facing surface at all).</summary>
public interface IBannerService
{
    Task<ServiceResult<List<OfficeBannerDto>>> ListForOfficeAsync(Guid officeId, CancellationToken ct = default);
    Task<ServiceResult<OfficeBannerDto>> CreateAsync(
        Guid officeId, Guid createdBy, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default);
    Task<ServiceResult<OfficeBannerDto>> PauseAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<OfficeBannerDto>> ResumeAsync(Guid officeId, Guid id, CancellationToken ct = default);

    /// <summary>Anonymous public surface — every Active banner across every office, for the home
    /// page slider.</summary>
    Task<ServiceResult<List<PublicBannerDto>>> ListPublicAsync(CancellationToken ct = default);
    Task<ImageFileDto?> GetImageFileAsync(Guid id, CancellationToken ct = default);
}
