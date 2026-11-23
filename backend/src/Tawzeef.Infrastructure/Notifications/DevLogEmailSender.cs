using Microsoft.Extensions.Logging;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Infrastructure.Notifications;

/// <summary>
/// Dev-only stand-in for <see cref="IEmailSender"/> — logs the message instead of sending a real
/// email. No real provider was ever chosen; replace this before any non-dev environment relies on
/// email OTP or notification emails.
/// </summary>
public class DevLogEmailSender(ILogger<DevLogEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        logger.LogWarning("[DEV EMAIL] To: {ToEmail} | Subject: {Subject} | Body: {Body}", toEmail, subject, body);
        return Task.CompletedTask;
    }
}
