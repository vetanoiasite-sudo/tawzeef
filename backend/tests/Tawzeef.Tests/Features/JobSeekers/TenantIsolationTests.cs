using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.JobSeekers;

/// <summary>BUSINESS_REQUIREMENTS.md §21: tenant isolation is one of the explicitly called-out
/// "must have tests" business rules. A profile that belongs to Office B must be invisible through
/// Office A's own review-service calls — GetByIdAsync here, the thing every detail-page and every
/// document/approve/reject action is built on (JobSeekerReviewService.cs's own helper reuses the
/// exact same "Id == x && OfficeId == officeId" shape for every one of them).</summary>
public class TenantIsolationTests
{
    [Fact]
    public async Task GetByIdAsync_ProfileBelongsToAnotherOffice_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var profileInOfficeB = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            OfficeId = officeB,
            UserId = Guid.NewGuid(),
            FullName = "Office B's Applicant",
            MobileNumber = "01000000000",
            ReviewStatus = JobSeekerReviewStatus.Submitted,
        };
        await uow.Repository<JobSeekerProfile>().AddAsync(profileInOfficeB);
        await uow.SaveChangesAsync();

        var service = new JobSeekerReviewService(uow, new FakeFileStorage());

        var result = await service.GetByIdAsync(officeA, profileInOfficeB.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_ProfileBelongsToTheCallingOffice_Succeeds()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            OfficeId = officeA,
            UserId = Guid.NewGuid(),
            FullName = "Office A's Applicant",
            MobileNumber = "01000000001",
            ReviewStatus = JobSeekerReviewStatus.Submitted,
        };
        await uow.Repository<JobSeekerProfile>().AddAsync(profile);
        await uow.SaveChangesAsync();

        var service = new JobSeekerReviewService(uow, new FakeFileStorage());

        var result = await service.GetByIdAsync(officeA, profile.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(profile.FullName, result.Data!.FullName);
    }

    [Fact]
    public async Task ApproveDocumentAsync_DocumentBelongsToAnotherOfficesProfile_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var profileInOfficeB = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            OfficeId = officeB,
            UserId = Guid.NewGuid(),
            FullName = "Office B's Applicant",
            MobileNumber = "01000000002",
        };
        var document = new JobSeekerDocument
        {
            Id = Guid.NewGuid(),
            OfficeId = officeB,
            JobSeekerId = profileInOfficeB.Id,
            DocumentType = JobSeekerDocumentType.Cv,
            SecureFileReference = "key",
            OriginalFileName = "cv.pdf",
            MimeType = "application/pdf",
            ReviewStatus = DocumentReviewStatus.Pending,
        };
        await uow.Repository<JobSeekerProfile>().AddAsync(profileInOfficeB);
        await uow.Repository<JobSeekerDocument>().AddAsync(document);
        await uow.SaveChangesAsync();

        var service = new JobSeekerReviewService(uow, new FakeFileStorage());

        // Office A's reviewer, armed with the correct document id (e.g. guessed or leaked),
        // must not be able to act on it just because the id is right.
        var result = await service.ApproveDocumentAsync(officeA, Guid.NewGuid(), profileInOfficeB.Id, document.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }
}
