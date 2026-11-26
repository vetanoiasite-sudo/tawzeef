using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Interviews;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobApplications;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Interviews;

public class HiringFollowUpService(IUnitOfWork uow) : IHiringFollowUpService
{
    private static HiringFollowUpDto ToDto(HiringFollowUpReport r) => new()
    {
        Id = r.Id,
        JobApplicationId = r.JobApplicationId,
        SelfReported = r.SelfReported,
        HiredAt = r.HiredAt,
        DueAt = r.DueAt,
        Status = EffectiveStatus(r).ToString(),
        StillEmployed = r.StillEmployed,
        EmployerSatisfied = r.EmployerSatisfied,
        JobSeekerSatisfied = r.JobSeekerSatisfied,
        Notes = r.Notes,
        CompletedAt = r.CompletedAt,
    };

    private static FollowUpReportStatus EffectiveStatus(HiringFollowUpReport r) =>
        r.Status == FollowUpReportStatus.PendingDue && r.DueAt < DateTime.UtcNow ? FollowUpReportStatus.Overdue : r.Status;

    public async Task<ServiceResult<HiringFollowUpDto>> CreateAsync(
        Guid officeId, Guid reviewerId, Guid applicationId, CreateHiringFollowUpDto dto, CancellationToken ct = default)
    {
        var application = await uow.Repository<JobApplication>().FindAsync(a => a.Id == applicationId && a.OfficeId == officeId, ct: ct);
        if (application is null) return ServiceResult<HiringFollowUpDto>.NotFound("Application not found.");

        var alreadyExists = await uow.Repository<HiringFollowUpReport>().AnyAsync(r => r.JobApplicationId == applicationId && r.IsActive, ct);
        if (alreadyExists) return ServiceResult<HiringFollowUpDto>.Conflict("This application has already been marked hired.");

        var hiredAt = dto.HiredAt ?? DateTime.UtcNow;

        // WORKFLOWS_AND_STATE_MACHINES.md §9.1: this POST is both the "mark hired" action and the
        // follow-up report's creation — JobApplication.HiredAt is stamped here.
        application.HiredAt = hiredAt;
        uow.Repository<JobApplication>().Update(application);

        var report = new HiringFollowUpReport
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            JobApplicationId = application.Id,
            HiredAt = hiredAt,
            DueAt = hiredAt.AddMonths(1),
            Status = FollowUpReportStatus.PendingDue,
        };
        await uow.Repository<HiringFollowUpReport>().AddAsync(report, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<HiringFollowUpDto>.Success(ToDto(report));
    }

    public async Task<ServiceResult<HiringFollowUpDto>> CompleteAsync(
        Guid officeId, Guid reviewerId, Guid applicationId, CompleteHiringFollowUpDto dto, CancellationToken ct = default)
    {
        var report = await uow.Repository<HiringFollowUpReport>().FindAsync(r => r.JobApplicationId == applicationId && r.OfficeId == officeId && r.IsActive, ct: ct);
        if (report is null) return ServiceResult<HiringFollowUpDto>.NotFound("No follow-up report exists for this application.");
        if (report.Status == FollowUpReportStatus.Completed)
            return ServiceResult<HiringFollowUpDto>.Conflict("This follow-up report has already been completed.");

        report.StillEmployed = dto.StillEmployed;
        report.EmployerSatisfied = dto.EmployerSatisfied;
        report.JobSeekerSatisfied = dto.JobSeekerSatisfied;
        report.Notes = dto.Notes?.Trim();
        report.Status = FollowUpReportStatus.Completed;
        report.CompletedAt = DateTime.UtcNow;
        report.CompletedBy = reviewerId;
        uow.Repository<HiringFollowUpReport>().Update(report);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<HiringFollowUpDto>.Success(ToDto(report));
    }

    public async Task<ServiceResult<HiringFollowUpDto>> GetAsync(Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct = default)
    {
        var application = await uow.Repository<JobApplication>().GetByIdAsync(applicationId, ct);
        if (application is null) return ServiceResult<HiringFollowUpDto>.NotFound("Application not found.");

        bool allowed;
        if (officeId.HasValue)
        {
            allowed = application.OfficeId == officeId.Value;
        }
        else
        {
            var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
            allowed = employer is not null && await uow.Repository<JobAdvertisement>().AnyAsync(
                a => a.Id == application.JobAdvertisementId && a.EmployerId == employer.Id, ct);
        }
        if (!allowed) return ServiceResult<HiringFollowUpDto>.NotFound("Application not found.");

        var report = await uow.Repository<HiringFollowUpReport>().FindAsync(r => r.JobApplicationId == applicationId && r.IsActive, ct: ct);
        if (report is null) return ServiceResult<HiringFollowUpDto>.NotFound("No follow-up report exists for this application.");

        return ServiceResult<HiringFollowUpDto>.Success(ToDto(report));
    }

    public async Task<ServiceResult<PagedResult<HiringFollowUpListItemDto>>> ListDueAsync(
        Guid officeId, HiringFollowUpQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<HiringFollowUpReport>().PageAsync(
            pageIndex, pageSize,
            r => r.OfficeId == officeId && r.Status == FollowUpReportStatus.PendingDue,
            orderBy: r => r.DueAt, ct: ct);

        var applicationIds = page.Items.Where(r => r.JobApplicationId is not null).Select(r => r.JobApplicationId!.Value).ToList();
        var applications = applicationIds.Count == 0 ? [] : await uow.Repository<JobApplication>().ListAsync(a => applicationIds.Contains(a.Id), ct: ct);
        var applicationsById = applications.ToDictionary(a => a.Id);

        var adIds = applications.Select(a => a.JobAdvertisementId).Distinct().ToList();
        var ads = adIds.Count == 0 ? [] : await uow.Repository<JobAdvertisement>().ListAsync(a => adIds.Contains(a.Id), ct: ct);
        var adsById = ads.ToDictionary(a => a.Id);

        var employerIds = ads.Select(a => a.EmployerId).Distinct().ToList();
        var employers = employerIds.Count == 0 ? [] : await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var employersById = employers.ToDictionary(e => e.Id);

        // Self-reported rows (no JobApplicationId) reach their job seeker through
        // JobSeekerOfficeApplicationId instead.
        var officeApplicationIds = page.Items.Where(r => r.JobSeekerOfficeApplicationId is not null)
            .Select(r => r.JobSeekerOfficeApplicationId!.Value).ToList();
        var officeApplications = officeApplicationIds.Count == 0 ? [] : await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => officeApplicationIds.Contains(a.Id), ct: ct);
        var officeApplicationsById = officeApplications.ToDictionary(a => a.Id);

        var jobSeekerIds = applications.Select(a => a.JobSeekerId)
            .Concat(officeApplications.Select(a => a.JobSeekerId))
            .Distinct().ToList();
        var profiles = jobSeekerIds.Count == 0 ? [] : await uow.Repository<JobSeekerProfile>().ListAsync(p => jobSeekerIds.Contains(p.Id), ct: ct);
        var profilesById = profiles.ToDictionary(p => p.Id);

        return ServiceResult<PagedResult<HiringFollowUpListItemDto>>.Success(new PagedResult<HiringFollowUpListItemDto>
        {
            Items = page.Items.Select(r =>
            {
                var application = r.JobApplicationId is null ? null : applicationsById.GetValueOrDefault(r.JobApplicationId.Value);
                var ad = application is null ? null : adsById.GetValueOrDefault(application.JobAdvertisementId);
                var officeApplication = r.JobSeekerOfficeApplicationId is null ? null : officeApplicationsById.GetValueOrDefault(r.JobSeekerOfficeApplicationId.Value);
                var jobSeekerId = application?.JobSeekerId ?? officeApplication?.JobSeekerId;
                return new HiringFollowUpListItemDto
                {
                    Id = r.Id,
                    JobApplicationId = r.JobApplicationId,
                    SelfReported = r.SelfReported,
                    JobSeekerName = jobSeekerId is null ? string.Empty : profilesById.GetValueOrDefault(jobSeekerId.Value)?.FullName ?? string.Empty,
                    EmployerDisplayName = ad is null ? string.Empty : employersById.GetValueOrDefault(ad.EmployerId)?.DisplayName ?? string.Empty,
                    JobTitle = ad?.Title ?? string.Empty,
                    HiredAt = r.HiredAt,
                    DueAt = r.DueAt,
                    Status = EffectiveStatus(r).ToString(),
                };
            }).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }
}
