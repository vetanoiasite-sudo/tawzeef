using Tawzeef.Application.Features.PublicJobs;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.PublicJobs;

/// <summary>BUSINESS_REQUIREMENTS.md §6.5/§21: an advertisement runs exactly 15 days then must
/// never appear in public results again. PublicJobSearchService's own comment explains this is a
/// read-time filter, not (until this session) a status flip — these tests cover that filter
/// directly: SearchAsync and GetByIdAsync must both exclude a Published-but-overdue ad, and must
/// still include one that's Published with no expiry pressure yet.</summary>
public class AdvertisementExpiryTests
{
    private static JobAdvertisement MakeAd(Guid officeId, Guid employerId, JobAdvertisementStatus status, DateTime? expiresAt) => new()
    {
        Id = Guid.NewGuid(),
        OfficeId = officeId,
        EmployerId = employerId,
        Title = "Chef",
        Status = status,
        PublishedAt = DateTime.UtcNow.AddDays(-20),
        ExpiresAt = expiresAt,
    };

    [Fact]
    public async Task SearchAsync_PublishedAdPastExpiresAt_IsExcludedFromResults()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var employer = new Employer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "Acme", DisplayName = "Acme" };
        var expiredAd = MakeAd(officeId, employer.Id, JobAdvertisementStatus.Published, DateTime.UtcNow.AddDays(-1));

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(expiredAd);
        await uow.SaveChangesAsync();

        var service = new PublicJobSearchService(uow);

        var result = await service.SearchAsync(new JobSearchQueryDto { PageIndex = 0, PageSize = 20 });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!.Items);
    }

    [Fact]
    public async Task SearchAsync_PublishedAdNotYetExpired_IsIncludedInResults()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var employer = new Employer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "Acme", DisplayName = "Acme" };
        var liveAd = MakeAd(officeId, employer.Id, JobAdvertisementStatus.Published, DateTime.UtcNow.AddDays(5));

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(liveAd);
        await uow.SaveChangesAsync();

        var service = new PublicJobSearchService(uow);

        var result = await service.SearchAsync(new JobSearchQueryDto { PageIndex = 0, PageSize = 20 });

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!.Items);
    }

    [Fact]
    public async Task GetByIdAsync_PublishedAdPastExpiresAt_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var employer = new Employer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "Acme", DisplayName = "Acme" };
        var expiredAd = MakeAd(officeId, employer.Id, JobAdvertisementStatus.Published, DateTime.UtcNow.AddHours(-1));

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(expiredAd);
        await uow.SaveChangesAsync();

        var service = new PublicJobSearchService(uow);

        // API_DOCUMENTATION.md §3.7: an expired ad must not even confirm its own existence.
        var result = await service.GetByIdAsync(expiredAd.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }

    [Fact]
    public async Task SearchAsync_AdAlreadyFlippedToExpiredStatus_IsExcludedRegardlessOfDate()
    {
        // Belt-and-suspenders: once JobAdvertisementExpiryService (the background job) has flipped
        // Status to Expired, the read-time filter's own Status == Published check must still hold —
        // ExpiresAt alone is not what gates visibility.
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var employer = new Employer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "Acme", DisplayName = "Acme" };
        var alreadyExpiredAd = MakeAd(officeId, employer.Id, JobAdvertisementStatus.Expired, DateTime.UtcNow.AddDays(-1));

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(alreadyExpiredAd);
        await uow.SaveChangesAsync();

        var service = new PublicJobSearchService(uow);

        var result = await service.SearchAsync(new JobSearchQueryDto { PageIndex = 0, PageSize = 20 });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!.Items);
    }
}
