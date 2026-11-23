using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Infrastructure.Notifications;

/// <summary>
/// Real <see cref="IEmailSender"/> implementation using .NET's built-in SmtpClient — no extra
/// package needed. Reads the Smtp:* configuration section (Host, Port, Username, Password,
/// FromEmail, FromName, EnableSsl). Registered instead of <see cref="DevLogEmailSender"/> only
/// when Smtp:Host is actually configured (InfrastructureDI.AddInfrastructure) — so an environment
/// with no SMTP configured keeps logging instead of failing to send.
/// </summary>
public class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct = default)
    {
        var host = configuration["Smtp:Host"]
            ?? throw new InvalidOperationException("Smtp:Host is not configured.");
        var port = int.TryParse(configuration["Smtp:Port"], out var configuredPort) ? configuredPort : 587;
        var username = configuration["Smtp:Username"];
        var password = configuration["Smtp:Password"];
        var fromEmail = configuration["Smtp:FromEmail"] ?? username
            ?? throw new InvalidOperationException("Smtp:FromEmail (or Smtp:Username) is not configured.");
        var fromName = configuration["Smtp:FromName"] ?? "توظيف";
        var enableSsl = !bool.TryParse(configuration["Smtp:EnableSsl"], out var configuredSsl) || configuredSsl;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
        };
        if (!string.IsNullOrWhiteSpace(username))
            client.Credentials = new NetworkCredential(username, password);

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            BodyEncoding = System.Text.Encoding.UTF8,
            SubjectEncoding = System.Text.Encoding.UTF8,
        };
        message.To.Add(toEmail);

        try
        {
            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex)
        {
            // Never let a failed notification email break the request that triggered it (e.g. a
            // document approval) — log and move on, same "best effort" tradeoff the dev sender has.
            logger.LogError(ex, "Failed to send email to {ToEmail} via SMTP {Host}:{Port}", toEmail, host, port);
        }
    }
}
