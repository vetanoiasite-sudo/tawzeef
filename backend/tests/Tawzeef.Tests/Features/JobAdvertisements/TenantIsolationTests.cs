using Tawzeef.Application.Features.JobAdvertisements;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.JobAdvertisements;

/// <summary>Third office-scoped entity type covered for BUSINESS_REQUIREMENTS.md §21's tenant
/// isolation rule, alongside JobSeekers/Employers.TenantIsolationTests.</summary>
public class TenantIsolationTests
{
    [Fact]
    public async Task GetByIdAsync_AdvertisementBelongsToAnotherOffice_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var adInOfficeB = new JobAdvertisement
        {
            Id = Guid.NewGuid(),
            OfficeId = officeB,
            EmployerId = Guid.NewGuid(),
            Title = "Office B's Job",
            Status = JobAdvertisementStatus.SubmittedForReview,
        };
        await uow.Repository<JobAdvertisement>().AddAsync(adInOfficeB);
        await uow.SaveChangesAsync();

        var service = new JobAdvertisementReviewService(uow, new RecordingNotificationService());

        var result = await service.GetByIdAsync(officeA, adInOfficeB.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_AdvertisementBelongsToTheCallingOffice_SucceedsAndMovesUnderReview()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var ad = new JobAdvertisement
        {
            Id = Guid.NewGuid(),
            OfficeId = officeA,
            EmployerId = Guid.NewGuid(),
            Title = "Office A's Job",
            Status = JobAdvertisementStatus.SubmittedForReview,
        };
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.SaveChangesAsync();

        var service = new JobAdvertisementReviewService(uow, new RecordingNotificationService());

        var result = await service.GetByIdAsync(officeA, ad.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal("UnderReview", result.Data!.Status);
    }
}
