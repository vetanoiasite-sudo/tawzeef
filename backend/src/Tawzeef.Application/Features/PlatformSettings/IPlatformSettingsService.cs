using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.PlatformSettings;

public interface IPlatformSettingsService
{
    Task<ServiceResult<List<PlatformSettingDto>>> ListAsync(CancellationToken ct = default);

    Task<ServiceResult<PlatformSettingDto>> UpdateAsync(string key, Guid updatedBy, UpdatePlatformSettingDto dto, CancellationToken ct = default);

    /// <summary>For other services to read a setting's current value (falls back to the seeded
    /// default if the row is somehow missing) — e.g. JobAdvertisementReviewService's advertisement
    /// duration/fee. Not exposed over the API.</summary>
    Task<string> GetValueAsync(string key, string fallback, CancellationToken ct = default);
}
