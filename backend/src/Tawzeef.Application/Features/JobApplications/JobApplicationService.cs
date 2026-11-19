using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobApplications;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobApplications;

public class JobApplicationService(IUnitOfWork uow) : IJobApplicationService
{
    public async Task<ServiceResult<JobApplicationIdDto>> CreateAsync(Guid userId, CreateJobApplicationDto dto, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobApplicationIdDto>.NotFound("No job seeker profile for this user.");

        // API_DOCUMENTATION.md §3.8, confirmed §0 item 13: must already be Approved to apply.
        if (profile.ReviewStatus != JobSeekerReviewStatus.Approved)
            return ServiceResult<JobApplicationIdDto>.Forbidden("Your profile must be approved before you can apply.");

        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == dto.JobAdvertisementId, ct: ct);
        if (ad is null || ad.Status != JobAdvertisementStatus.Published)
            return ServiceResult<JobApplicationIdDto>.NotFound("This advertisement is not available.");

        var hasActiveApplication = await uow.Repository<JobApplication>().AnyAsync(
            a => a.JobAdvertisementId == ad.Id && a.JobSeekerId == profile.Id && a.WithdrawnAt == null, ct);
        if (hasActiveApplication)
            return ServiceResult<JobApplicationIdDto>.Conflict("You have already applied to this advertisement.");

        // The ad's own office administers this application — DATABASE_DOCUMENTATION.md §6; a Job
        // Seeker may apply to a published ad from any office, not only their registering office.
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            OfficeId = ad.OfficeId,
            JobAdvertisementId = ad.Id,
            JobSeekerId = profile.Id,
        };
        await uow.Repository<JobApplication>().AddAsync(application, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobApplicationIdDto>.Success(new JobApplicationIdDto { Id = application.Id });
    }

    public async Task<ServiceResult<PagedResult<MyApplicationListItemDto>>> ListMineAsync(
        Guid userId, JobApplicationQueryDto query, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<PagedResult<MyApplicationListItemDto>>.NotFound("No job seeker profile for this user.");

        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<JobApplication>().PageAsync(
            pageIndex, pageSize, a => a.JobSeekerId == profile.Id,
            orderBy: a => a.AppliedAt, descending: true, ct: ct);

        var adIds = page.Items.Select(a => a.JobAdvertisementId).Distinct().ToList();
        var ads = adIds.Count == 0 ? [] : await uow.Repository<JobAdvertisement>().ListAsync(a => adIds.Contains(a.Id), ct: ct);
        var adsById = ads.ToDictionary(a => a.Id);

        var employerIds = ads.Select(a => a.EmployerId).Distinct().ToList();
        var employers = employerIds.Count == 0 ? [] : await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var namesByEmployer = employers.ToDictionary(e => e.Id, e => e.DisplayName);

        return ServiceResult<PagedResult<MyApplicationListItemDto>>.Success(new PagedResult<MyApplicationListItemDto>
        {
            Items = page.Items.Select(a =>
            {
                var ad = adsById.GetValueOrDefault(a.JobAdvertisementId);
                return new MyApplicationListItemDto
                {
                    Id = a.Id,
                    JobAdvertisementId = a.JobAdvertisementId,
                    JobTitle = ad?.Title ?? string.Empty,
                    EmployerDisplayName = ad is null ? string.Empty : namesByEmployer.GetValueOrDefault(ad.EmployerId, string.Empty),
                    AppliedAt = a.AppliedAt,
                    JobSeekerFacingStatus = a.JobSeekerFacingStatus.ToString(),
                    WithdrawnAt = a.WithdrawnAt,
                };
            }).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult> WithdrawAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult.NotFound("No job seeker profile for this user.");

        var application = await uow.Repository<JobApplication>().FindAsync(a => a.Id == id && a.JobSeekerId == profile.Id, ct: ct);
        if (application is null) return ServiceResult.NotFound("Application not found.");
        if (application.WithdrawnAt is not null) return ServiceResult.Conflict("This application has already been withdrawn.");

        application.WithdrawnAt = DateTime.UtcNow;
        uow.Repository<JobApplication>().Update(application);
        await uow.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
