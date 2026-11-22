using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Platform;

namespace Tawzeef.Application.Features.PlatformSettings;

public class PlatformSettingsService(IUnitOfWork uow) : IPlatformSettingsService
{
    public async Task<ServiceResult<List<PlatformSettingDto>>> ListAsync(CancellationToken ct = default)
    {
        var settings = await uow.Repository<PlatformSetting>().ListAsync(ct: ct);
        return ServiceResult<List<PlatformSettingDto>>.Success(settings
            .OrderBy(s => s.Key)
            .Select(ToDto)
            .ToList());
    }

    public async Task<ServiceResult<PlatformSettingDto>> UpdateAsync(string key, Guid updatedBy, UpdatePlatformSettingDto dto, CancellationToken ct = default)
    {
        var setting = await uow.Repository<PlatformSetting>().FindAsync(s => s.Key == key, ct: ct);
        if (setting is null) return ServiceResult<PlatformSettingDto>.NotFound("Platform setting not found.");

        setting.Value = dto.Value.Trim();
        setting.UpdatedBy = updatedBy;
        setting.UpdatedAt = DateTime.UtcNow;
        uow.Repository<PlatformSetting>().Update(setting);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<PlatformSettingDto>.Success(ToDto(setting));
    }

    public async Task<string> GetValueAsync(string key, string fallback, CancellationToken ct = default)
    {
        var setting = await uow.Repository<PlatformSetting>().FindAsync(s => s.Key == key, ct: ct);
        return setting?.Value ?? fallback;
    }

    private static PlatformSettingDto ToDto(PlatformSetting s) => new()
    {
        Key = s.Key,
        Value = s.Value,
        Description = s.Description,
        UpdatedAt = s.UpdatedAt,
    };
}
