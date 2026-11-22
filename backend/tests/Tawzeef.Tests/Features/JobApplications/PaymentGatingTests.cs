using Tawzeef.Application.Features.JobApplications;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobApplications;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.JobApplications;

/// <summary>BUSINESS_REQUIREMENTS.md §6.6/§21: applicant CVs and contact details stay locked to
/// the employer until that specific advertisement's payment is confirmed Paid. GetCvAsync is the
/// literal download gate (the one byte-serving endpoint this rule protects); ListForEmployerAsync
/// is the list-level mirror of the same rule (RBAC_PERMISSIONS.md §5).</summary>
public class PaymentGatingTests
{
    private static (Employer Employer, JobAdvertisement Ad, JobSeekerProfile JobSeeker, JobApplication Application, JobSeekerDocument Cv) BuildScenario(Guid officeId)
    {
        var employerUserId = Guid.NewGuid();
        var employer = new Employer
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            UserId = employerUserId,
            LegalName = "Acme",
            DisplayName = "Acme",
            VerificationStatus = EmployerVerificationStatus.Approved,
        };
        var ad = new JobAdvertisement
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            EmployerId = employer.Id,
            Title = "Chef",
            Status = JobAdvertisementStatus.Published,
        };
        var jobSeeker = new JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            UserId = Guid.NewGuid(),
            FullName = "Ali",
            MobileNumber = "01000000003",
        };
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            JobAdvertisementId = ad.Id,
            JobSeekerId = jobSeeker.Id,
        };
        var cv = new JobSeekerDocument
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            JobSeekerId = jobSeeker.Id,
            DocumentType = JobSeekerDocumentType.Cv,
            SecureFileReference = "cv-key",
            OriginalFileName = "ali-cv.pdf",
            MimeType = "application/pdf",
            ReviewStatus = DocumentReviewStatus.Approved,
        };
        return (employer, ad, jobSeeker, application, cv);
    }

    [Fact]
    public async Task GetCvAsync_AdvertisementNotPaid_ReturnsForbidden()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var (employer, ad, jobSeeker, application, cv) = BuildScenario(officeId);

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.Repository<JobSeekerProfile>().AddAsync(jobSeeker);
        await uow.Repository<JobApplication>().AddAsync(application);
        await uow.Repository<JobSeekerDocument>().AddAsync(cv);
        await uow.SaveChangesAsync();
        // No AdvertisementPayment row at all — unpaid.

        var service = new ApplicantReviewService(uow, new FakeFileStorage());

        var result = await service.GetCvAsync(employer.UserId, application.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.Forbidden, result.Error);
    }

    [Fact]
    public async Task GetCvAsync_AdvertisementPaymentPendingNotPaid_ReturnsForbidden()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var (employer, ad, jobSeeker, application, cv) = BuildScenario(officeId);

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.Repository<JobSeekerProfile>().AddAsync(jobSeeker);
        await uow.Repository<JobApplication>().AddAsync(application);
        await uow.Repository<JobSeekerDocument>().AddAsync(cv);
        await uow.Repository<AdvertisementPayment>().AddAsync(new AdvertisementPayment
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            EmployerId = employer.Id,
            JobAdvertisementId = ad.Id,
            Amount = 500,
            PaymentStatus = PaymentStatus.Pending, // recorded, but not yet confirmed Paid
            RecordedBy = Guid.NewGuid(),
        });
        await uow.SaveChangesAsync();

        var service = new ApplicantReviewService(uow, new FakeFileStorage());

        var result = await service.GetCvAsync(employer.UserId, application.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.Forbidden, result.Error);
    }

    [Fact]
    public async Task GetCvAsync_AdvertisementPaid_Succeeds()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var (employer, ad, jobSeeker, application, cv) = BuildScenario(officeId);

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.Repository<JobSeekerProfile>().AddAsync(jobSeeker);
        await uow.Repository<JobApplication>().AddAsync(application);
        await uow.Repository<JobSeekerDocument>().AddAsync(cv);
        await uow.Repository<AdvertisementPayment>().AddAsync(new AdvertisementPayment
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            EmployerId = employer.Id,
            JobAdvertisementId = ad.Id,
            Amount = 500,
            PaymentStatus = PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow,
            RecordedBy = Guid.NewGuid(),
        });
        await uow.SaveChangesAsync();

        var service = new ApplicantReviewService(uow, new FakeFileStorage());

        var result = await service.GetCvAsync(employer.UserId, application.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(cv.OriginalFileName, result.Data!.FileName);
    }

    [Fact]
    public async Task ListForEmployerAsync_AdvertisementNotPaid_HidesPhoneEmailAndCvLink()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var (employer, ad, jobSeeker, application, cv) = BuildScenario(officeId);

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.Repository<JobSeekerProfile>().AddAsync(jobSeeker);
        await uow.Repository<JobApplication>().AddAsync(application);
        await uow.Repository<JobSeekerDocument>().AddAsync(cv);
        await uow.SaveChangesAsync();

        var service = new ApplicantReviewService(uow, new FakeFileStorage());

        var result = await service.ListForEmployerAsync(employer.UserId, ad.Id, new JobApplicationQueryDto { PageIndex = 0, PageSize = 20 });

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Data!.Items);
        Assert.Null(item.Phone);
        Assert.Null(item.Email);
        Assert.Null(item.CvDownloadUrl);
    }

    [Fact]
    public async Task ListForEmployerAsync_AdvertisementPaid_IncludesPhoneEmailAndCvLink()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var (employer, ad, jobSeeker, application, cv) = BuildScenario(officeId);

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.Repository<JobSeekerProfile>().AddAsync(jobSeeker);
        await uow.Repository<JobApplication>().AddAsync(application);
        await uow.Repository<JobSeekerDocument>().AddAsync(cv);
        await uow.Repository<AdvertisementPayment>().AddAsync(new AdvertisementPayment
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            EmployerId = employer.Id,
            JobAdvertisementId = ad.Id,
            Amount = 500,
            PaymentStatus = PaymentStatus.Paid,
            PaidAt = DateTime.UtcNow,
            RecordedBy = Guid.NewGuid(),
        });
        await uow.SaveChangesAsync();

        var service = new ApplicantReviewService(uow, new FakeFileStorage());

        var result = await service.ListForEmployerAsync(employer.UserId, ad.Id, new JobApplicationQueryDto { PageIndex = 0, PageSize = 20 });

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Data!.Items);
        Assert.NotNull(item.CvDownloadUrl);
    }

    [Fact]
    public async Task ListForOfficeAsync_NeverPaymentGated_IncludesGatedFieldsRegardlessOfPayment()
    {
        await using var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var officeId = Guid.NewGuid();
        var (employer, ad, jobSeeker, application, cv) = BuildScenario(officeId);

        await uow.Repository<Employer>().AddAsync(employer);
        await uow.Repository<JobAdvertisement>().AddAsync(ad);
        await uow.Repository<JobSeekerProfile>().AddAsync(jobSeeker);
        await uow.Repository<JobApplication>().AddAsync(application);
        await uow.Repository<JobSeekerDocument>().AddAsync(cv);
        await uow.SaveChangesAsync();
        // Still no payment at all.

        var service = new ApplicantReviewService(uow, new FakeFileStorage());

        // RBAC_PERMISSIONS.md §3 footnote: office staff reviewing applications is never gated by
        // the employer's own payment status.
        var result = await service.ListForOfficeAsync(officeId, ad.Id, new JobApplicationQueryDto { PageIndex = 0, PageSize = 20 });

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Data!.Items);
        Assert.NotNull(item.CvDownloadUrl);
    }
}
