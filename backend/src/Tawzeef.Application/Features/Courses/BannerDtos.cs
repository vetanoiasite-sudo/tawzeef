namespace Tawzeef.Application.Features.Courses;

public class OfficeBannerDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}

/// <summary>Anonymous public listing — image only, no link (explicit product decision).</summary>
public class PublicBannerDto
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
