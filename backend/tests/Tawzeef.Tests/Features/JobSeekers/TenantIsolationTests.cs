using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.JobSeekers;

/// <summary>BUSINESS_REQUIREMENTS.md §21: tenant isolation is one of the explicitly called-out
/// "must have tests" business rules. A job seeker with no JobSeekerOfficeApplication row for Office
/// A must be invisible through Office A's own review-service calls — GetByIdAsync here, the thing
/// every detail-page and every document/approve/reject action is built on
/// (JobSeekerReviewService.cs's own helper reuses the exact same lookup for every one of them).
/// Confirmed 2026-11-24: a job seeker may now have rows for several offices at once, so the
/// boundary is "does a row for THIS office exist", not a single profile-level OfficeId.</summary>
public class TenantIsolationTests
{
    [Fact]
    public async Task GetByIdAsync_ProfileHasNoApplicationForCallingOffice_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FullName = "Office B's Applicant",
            MobileNumber = "01000000000",
        };
        var applicationToOfficeB = new JobSeekerOfficeApplication
        {
            Id = Guid.NewGuid(),
            JobSeekerId = profile.Id,
            OfficeId = officeB,
            ReviewStatus = JobSeekerReviewStatus.Submitted,
        };
        await uow.Repository<JobSeekerProfile>().AddAsync(profile);
        await uow.Repository<JobSeekerOfficeApplication>().AddAsync(applicationToOfficeB);
        await uow.SaveChangesAsync();

        var service = new JobSeekerReviewService(uow, new FakeFileStorage(), new FakeNotificationService());

        var result = await service.GetByIdAsync(officeA, profile.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }

    [Fact]
    public async Task GetByIdAsync_ProfileHasApplicationForCallingOffice_Succeeds()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FullName = "Office A's Applicant",
            MobileNumber = "01000000001",
        };
        var application = new JobSeekerOfficeApplication
        {
            Id = Guid.NewGuid(),
            JobSeekerId = profile.Id,
            OfficeId = officeA,
            ReviewStatus = JobSeekerReviewStatus.Submitted,
        };
        await uow.Repository<JobSeekerProfile>().AddAsync(profile);
        await uow.Repository<JobSeekerOfficeApplication>().AddAsync(application);
        await uow.SaveChangesAsync();

        var service = new JobSeekerReviewService(uow, new FakeFileStorage(), new FakeNotificationService());

        var result = await service.GetByIdAsync(officeA, profile.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(profile.FullName, result.Data!.FullName);
    }

    [Fact]
    public async Task ApproveDocumentAsync_ProfileHasNoApplicationForCallingOffice_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var profile = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            FullName = "Office B's Applicant",
            MobileNumber = "01000000002",
        };
        var applicationToOfficeB = new JobSeekerOfficeApplication
        {
            Id = Guid.NewGuid(),
            JobSeekerId = profile.Id,
            OfficeId = officeB,
        };
        var document = new JobSeekerDocument
        {
            Id = Guid.NewGuid(),
            JobSeekerId = profile.Id,
            DocumentType = JobSeekerDocumentType.Cv,
            SecureFileReference = "key",
            OriginalFileName = "cv.pdf",
            MimeType = "application/pdf",
            ReviewStatus = DocumentReviewStatus.Pending,
        };
        await uow.Repository<JobSeekerProfile>().AddAsync(profile);
        await uow.Repository<JobSeekerOfficeApplication>().AddAsync(applicationToOfficeB);
        await uow.Repository<JobSeekerDocument>().AddAsync(document);
        await uow.SaveChangesAsync();

        var service = new JobSeekerReviewService(uow, new FakeFileStorage(), new FakeNotificationService());

        // Office A's reviewer, armed with the correct document id (e.g. guessed or leaked),
        // must not be able to act on it just because the id is right — documents are shared, but
        // only an office with its own application row for this job seeker may touch them.
        var result = await service.ApproveDocumentAsync(officeA, Guid.NewGuid(), profile.Id, document.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }
}
