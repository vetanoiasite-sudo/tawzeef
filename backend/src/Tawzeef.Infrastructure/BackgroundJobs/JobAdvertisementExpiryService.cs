using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Infrastructure.BackgroundJobs;

/// <summary>
/// BUSINESS_REQUIREMENTS.md §6.5: a job advertisement runs for exactly 15 days then must expire.
/// PublicJobSearchService already excludes a past-ExpiresAt ad from public results as a read-time
/// filter, but nothing ever flips its actual Status — so an employer's/office's own listings (which
/// filter by Status, not by ExpiresAt) would show it as "Published" forever. This is the one place
/// that actually transitions Status: Published -> Expired, polling every 5 minutes (this project's
/// scale has no need for a precise-to-the-second expiry or a configurable schedule).
/// </summary>
public class JobAdvertisementExpiryService(
    IServiceScopeFactory scopeFactory,
    ILogger<JobAdvertisementExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        // Run once immediately on startup, then on every tick after.
        await ExpireDueAdvertisementsAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExpireDueAdvertisementsAsync(stoppingToken);
        }
    }

    private async Task ExpireDueAdvertisementsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var now = DateTime.UtcNow;
            var dueAds = await uow.Repository<JobAdvertisement>().ListAsync(
                a => a.Status == JobAdvertisementStatus.Published && a.ExpiresAt != null && a.ExpiresAt <= now, ct: ct);

            if (dueAds.Count == 0) return;

            foreach (var ad in dueAds)
            {
                ad.Status = JobAdvertisementStatus.Expired;
                uow.Repository<JobAdvertisement>().Update(ad);
            }

            await uow.SaveChangesAsync(ct);
            logger.LogInformation("Expired {Count} job advertisement(s) past their 15-day ExpiresAt.", dueAds.Count);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutting down — not an error.
        }
        catch (Exception ex)
        {
            // A transient DB failure must not take the whole app down; just skip this tick.
            logger.LogError(ex, "Job advertisement expiry tick failed — will retry on the next interval.");
        }
    }
}
