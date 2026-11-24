using Tawzeef.Application.Features.PublicJobs;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.PublicJobs;

/// <summary>JobTitleSynonyms' doc comment: a job seeker searching "شيف" should also find a job ad
/// titled "طباخ" — same real job, different name. Covers both the standalone expansion helper and
/// the end-to-end search (confirming PredicateCombinator's dynamically-built OR actually translates
/// through EF Core's InMemory provider, not just that the C# compiles).</summary>
public class JobTitleSynonymSearchTests
{
    [Fact]
    public void Expand_KnownSynonym_IncludesTheOtherTermsInItsGroup()
    {
        var terms = JobTitleSynonyms.Expand("شيف");

        Assert.Contains("شيف", terms);
        Assert.Contains("طباخ", terms);
        Assert.Contains("كوك", terms);
    }

    [Fact]
    public void Expand_UnknownTerm_ReturnsOnlyTheOriginalKeyword()
    {
        var terms = JobTitleSynonyms.Expand("كلمة غريبة مش موجودة");

        Assert.Single(terms);
        Assert.Equal("كلمة غريبة مش موجودة", terms[0]);
    }

    [Fact]
    public async Task SearchAsync_KeywordMatchesSynonymOfAdTitle_AdIsIncluded()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var employer = new Employer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "Acme", DisplayName = "Acme" };
        var ad = new JobAdvertisement
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            EmployerId = employer.Id,
            Title = "طباخ في مطعم",
            Status = JobAdvertisementStatus.Published,
            PublishedAt = DateTime.UtcNow,
        };

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.SaveChangesAsync();

        var service = new PublicJobSearchService(uow);

        // Searching "شيف" (chef) must find an ad titled "طباخ" (cook) — different word, same job.
        var result = await service.SearchAsync(new JobSearchQueryDto { Keyword = "شيف", PageIndex = 0, PageSize = 20 });

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Data!.Items);
        Assert.Equal(ad.Id, item.Id);
    }

    [Fact]
    public async Task SearchAsync_KeywordDoesNotMatchAnySynonymOrTitle_AdIsExcluded()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var employer = new Employer { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), LegalName = "Acme", DisplayName = "Acme" };
        var ad = new JobAdvertisement
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            EmployerId = employer.Id,
            Title = "طباخ في مطعم",
            Status = JobAdvertisementStatus.Published,
            PublishedAt = DateTime.UtcNow,
        };

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.SaveChangesAsync();

        var service = new PublicJobSearchService(uow);

        var result = await service.SearchAsync(new JobSearchQueryDto { Keyword = "محاسب", PageIndex = 0, PageSize = 20 });

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!.Items);
    }
}
