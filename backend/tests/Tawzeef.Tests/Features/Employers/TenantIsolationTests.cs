using Tawzeef.Application.Features.Employers;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.Employers;

/// <summary>Mirrors JobSeekers.TenantIsolationTests exactly — same rule, the other office-scoped
/// entity type (BUSINESS_REQUIREMENTS.md §21). Confirmed 2026-11-24: an employer may now have rows
/// for several offices at once, so the boundary is "does an EmployerOfficeApplication row for THIS
/// office exist", not a single employer-level OfficeId.</summary>
public class TenantIsolationTests
{
    [Fact]
    public async Task GetByIdAsync_EmployerHasNoApplicationForCallingOffice_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var employer = new Employer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LegalName = "Office B Co.",
            DisplayName = "Office B Co.",
        };
        var applicationToOfficeB = new EmployerOfficeApplication
        {
            Id = Guid.NewGuid(),
            EmployerId = employer.Id,
            OfficeId = officeB,
            VerificationStatus = EmployerVerificationStatus.Submitted,
        };
        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<EmployerOfficeApplication>().AddAsync(applicationToOfficeB);
        await uow.SaveChangesAsync();

        var service = new EmployerReviewService(uow, new FakeFileStorage(), new FakeNotificationService());

        var result = await service.GetByIdAsync(officeA, employer.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }

    [Fact]
    public async Task StopCooperationAsync_EmployerHasNoApplicationForCallingOffice_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var employer = new Employer
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LegalName = "Office B Co.",
            DisplayName = "Office B Co.",
        };
        var applicationToOfficeB = new EmployerOfficeApplication
        {
            Id = Guid.NewGuid(),
            EmployerId = employer.Id,
            OfficeId = officeB,
            VerificationStatus = EmployerVerificationStatus.Approved,
            CooperationStatus = EmployerCooperationStatus.Active,
        };
        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<EmployerOfficeApplication>().AddAsync(applicationToOfficeB);
        await uow.SaveChangesAsync();

        var service = new EmployerReviewService(uow, new FakeFileStorage(), new FakeNotificationService());

        // An Office A admin must not be able to stop cooperation with a company that isn't theirs.
        var result = await service.StopCooperationAsync(officeA, Guid.NewGuid(), employer.Id, "reason");

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }
}
