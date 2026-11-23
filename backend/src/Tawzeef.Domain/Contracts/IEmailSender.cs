namespace Tawzeef.Domain.Contracts;

/// <summary>
/// Added 2026-11-23 for email OTP verification at registration and "something about your account
/// changed" notification emails (NotificationService.CreateAsync). Which real provider sends it
/// was never decided (same open question as IOtpSender's doc comment) — the dev implementation
/// only logs the message; swap it for a real provider before any non-dev environment goes live.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default);
}
