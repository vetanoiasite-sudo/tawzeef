using Tawzeef.Application.Features.Employers;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.Employers;

/// <summary>Mirrors JobSeekers.TenantIsolationTests exactly — same rule, the other office-scoped
/// entity type (BUSINESS_REQUIREMENTS.md §21).</summary>
public class TenantIsolationTests
{
    [Fact]
    public async Task GetByIdAsync_EmployerBelongsToAnotherOffice_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var employerInOfficeB = new Employer
        {
            Id = Guid.NewGuid(),
            OfficeId = officeB,
            UserId = Guid.NewGuid(),
            LegalName = "Office B Co.",
            DisplayName = "Office B Co.",
            VerificationStatus = EmployerVerificationStatus.Submitted,
        };
        await uow.Repository<Employer>().AddAsync(employerInOfficeB);
        await uow.SaveChangesAsync();

        var service = new EmployerReviewService(uow, new FakeFileStorage());

        var result = await service.GetByIdAsync(officeA, employerInOfficeB.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }

    [Fact]
    public async Task StopCooperationAsync_EmployerBelongsToAnotherOffice_ReturnsNotFound()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);

        var officeA = Guid.NewGuid();
        var officeB = Guid.NewGuid();
        var employerInOfficeB = new Employer
        {
            Id = Guid.NewGuid(),
            OfficeId = officeB,
            UserId = Guid.NewGuid(),
            LegalName = "Office B Co.",
            DisplayName = "Office B Co.",
            VerificationStatus = EmployerVerificationStatus.Approved,
            CooperationStatus = EmployerCooperationStatus.Active,
        };
        await uow.Repository<Employer>().AddAsync(employerInOfficeB);
        await uow.SaveChangesAsync();

        var service = new EmployerReviewService(uow, new FakeFileStorage());

        // An Office A admin must not be able to stop cooperation with a company that isn't theirs.
        var result = await service.StopCooperationAsync(officeA, Guid.NewGuid(), employerInOfficeB.Id, "reason");

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }
}
