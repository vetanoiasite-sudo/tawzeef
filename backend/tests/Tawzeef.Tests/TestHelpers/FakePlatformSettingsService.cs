using Tawzeef.Application.Common;
using Tawzeef.Application.Features.PlatformSettings;

namespace Tawzeef.Tests.TestHelpers;

/// <summary>Always returns the caller's fallback — these tests don't exercise platform-setting
/// overrides, just the default-duration/currency behavior JobAdvertisementReviewService falls
/// back to when no row exists.</summary>
public class FakePlatformSettingsService : IPlatformSettingsService
{
    public Task<ServiceResult<List<PlatformSettingDto>>> ListAsync(CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<ServiceResult<PlatformSettingDto>> UpdateAsync(string key, Guid updatedBy, UpdatePlatformSettingDto dto, CancellationToken ct = default) =>
        throw new NotImplementedException();

    public Task<string> GetValueAsync(string key, string fallback, CancellationToken ct = default) =>
        Task.FromResult(fallback);
}
