namespace Tawzeef.Domain.Entities.Platform;

/// <summary>Simple key-value platform-wide configuration, edited by SuperAdmin only
/// (platformsettings.edit). BUSINESS_REQUIREMENTS.md never enumerates concrete settings, so this is
/// seeded with the handful of values this app already hardcodes in business logic — see
/// <see cref="PlatformSettingKeys"/> — rather than invented decorative fields: editing one of these
/// actually changes behavior, since the services that used the hardcoded value now read it from
/// here instead.</summary>
public class PlatformSetting
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid? UpdatedBy { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
