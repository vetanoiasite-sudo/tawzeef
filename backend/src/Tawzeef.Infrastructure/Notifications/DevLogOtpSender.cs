using Microsoft.Extensions.Logging;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Infrastructure.Notifications;

/// <summary>
/// Dev-only stand-in for <see cref="IOtpSender"/> — logs the code instead of sending an SMS.
/// No real provider was ever chosen (BUSINESS_REQUIREMENTS.md §24.H.55); replace this before any
/// non-dev environment relies on OTP.
/// </summary>
public class DevLogOtpSender(ILogger<DevLogOtpSender> logger) : IOtpSender
{
    public Task SendAsync(string phoneNumber, string code, CancellationToken ct = default)
    {
        logger.LogWarning("[DEV OTP] {PhoneNumber} -> {Code} (never logs like this once a real SMS provider is wired up)", phoneNumber, code);
        return Task.CompletedTask;
    }
}
