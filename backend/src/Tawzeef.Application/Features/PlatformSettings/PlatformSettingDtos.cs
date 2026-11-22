using System.ComponentModel.DataAnnotations;

namespace Tawzeef.Application.Features.PlatformSettings;

public class PlatformSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public class UpdatePlatformSettingDto
{
    [Required, MaxLength(500)]
    public string Value { get; set; } = string.Empty;
}
