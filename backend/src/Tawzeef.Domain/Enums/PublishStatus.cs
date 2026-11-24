namespace Tawzeef.Domain.Enums;

/// <summary>Shared by Course and OfficeBanner — both are office-authored content with no review
/// workflow (the office is its own gatekeeper), just a simple visible/hidden toggle.</summary>
public enum PublishStatus : byte
{
    Active = 0,
    Paused = 1,
}
