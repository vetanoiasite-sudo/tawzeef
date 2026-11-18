namespace Tawzeef.Domain.Contracts;

/// <summary>
/// Confirmed required for registration (RBAC_PERMISSIONS.md §0 item 10), but which real SMS
/// provider sends it was never decided (BUSINESS_REQUIREMENTS.md §24.H.55 — notification channels
/// are still open). The dev implementation only logs the code; swap it for a real provider before
/// any non-dev environment goes live.
/// </summary>
public interface IOtpSender
{
    Task SendAsync(string phoneNumber, string code, CancellationToken ct = default);
}
