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

    /// <summary>The PlatformSettingKeys.SupportedCurrencies setting, split on commas — exposed
    /// anonymously (PlatformSettingsController.GetSupportedCurrencies) so a currency dropdown
    /// never has to hardcode its options.</summary>
    Task<ServiceResult<List<string>>> GetSupportedCurrenciesAsync(CancellationToken ct = default);
}
