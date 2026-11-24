using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Interviews;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobApplications;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Interviews;

public class InterviewService(IUnitOfWork uow, INotificationService notificationService) : IInterviewService
{
    private static string InterviewStatusLabel(InterviewStatus status) => status switch
    {
        InterviewStatus.Scheduled => "مجدولة",
        InterviewStatus.Completed => "تمت",
        InterviewStatus.Cancelled => "ملغاة",
        InterviewStatus.NoShow => "لم يحضر المتقدم",
        _ => status.ToString(),
    };

    public async Task<ServiceResult<InterviewDto>> ScheduleAsync(Guid userId, Guid applicationId, CreateInterviewDto dto, CancellationToken ct = default)
    {
        var (application, error) = await LoadOwnedByEmployerAsync(userId, applicationId, ct);
        if (error is not null) return ServiceResult<InterviewDto>.NotFound(error);

        var existing = await uow.Repository<Interview>().AnyAsync(i => i.JobApplicationId == applicationId && i.IsActive, ct);
        if (existing) return ServiceResult<InterviewDto>.Conflict("An interview is already scheduled for this application.");

        var interview = new Interview
        {
            Id = Guid.NewGuid(),
            OfficeId = application!.OfficeId,
            JobApplicationId = application.Id,
            ScheduledAt = dto.ScheduledAt,
            LocationOrMeetingDetails = dto.LocationOrMeetingDetails?.Trim(),
        };
        await uow.Repository<Interview>().AddAsync(interview, ct);

        var jobSeeker = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == application.JobSeekerId, ct: ct);
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == application.JobAdvertisementId, ct: ct);
        if (jobSeeker is not null)
        {
            var when = dto.ScheduledAt?.ToString("yyyy-MM-dd HH:mm") ?? "—";
            await notificationService.CreateAsync(
                application.OfficeId, jobSeeker.UserId, "Interview.Scheduled",
                $"تم تحديد موعد مقابلة لك بخصوص وظيفة \"{ad?.Title}\" بتاريخ {when}.",
                nameof(Interview), interview.Id, ct);
        }

        await uow.SaveChangesAsync(ct);

        return ServiceResult<InterviewDto>.Success(interview.ToDto());
    }

    public async Task<ServiceResult<InterviewDto>> UpdateAsync(Guid userId, Guid applicationId, UpdateInterviewDto dto, CancellationToken ct = default)
    {
        var (application, error) = await LoadOwnedByEmployerAsync(userId, applicationId, ct);
        if (error is not null) return ServiceResult<InterviewDto>.NotFound(error);

        var interview = await uow.Repository<Interview>().FindAsync(i => i.JobApplicationId == application!.Id && i.IsActive, ct: ct);
        if (interview is null) return ServiceResult<InterviewDto>.NotFound("No interview is scheduled for this application.");

        if (dto.Status.HasValue)
        {
            // WORKFLOWS_AND_STATE_MACHINES.md §8: Scheduled -> one of the three terminal states; no
            // transition out of a terminal state once reached.
            if (interview.Status != InterviewStatus.Scheduled)
                return ServiceResult<InterviewDto>.Conflict("This interview has already reached a final state.");
            if (dto.Status.Value == InterviewStatus.Scheduled)
                return ServiceResult<InterviewDto>.Invalid("Status can only move to Completed, Cancelled, or NoShow.");
            interview.Status = dto.Status.Value;

            var jobSeeker = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == application!.JobSeekerId, ct: ct);
            var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == application!.JobAdvertisementId, ct: ct);
            if (jobSeeker is not null)
            {
                await notificationService.CreateAsync(
                    application!.OfficeId, jobSeeker.UserId, "Interview.StatusChanged",
                    $"تحديث على مقابلتك لوظيفة \"{ad?.Title}\": {InterviewStatusLabel(interview.Status)}.",
                    nameof(Interview), interview.Id, ct);
            }
        }

        if (dto.ScheduledAt.HasValue) interview.ScheduledAt = dto.ScheduledAt;
        if (dto.LocationOrMeetingDetails is not null) interview.LocationOrMeetingDetails = dto.LocationOrMeetingDetails.Trim();

        uow.Repository<Interview>().Update(interview);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<InterviewDto>.Success(interview.ToDto());
    }

    public async Task<ServiceResult<InterviewDto>> GetAsync(Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct = default)
    {
        var (application, error) = await LoadForViewingAsync(userId, officeId, applicationId, ct);
        if (error is not null) return ServiceResult<InterviewDto>.NotFound(error);

        var interview = await uow.Repository<Interview>().FindAsync(i => i.JobApplicationId == application!.Id && i.IsActive, ct: ct);
        if (interview is null) return ServiceResult<InterviewDto>.NotFound("No interview is scheduled for this application.");

        return ServiceResult<InterviewDto>.Success(interview.ToDto());
    }

    public async Task<ServiceResult<InterviewReportDto>> CreateReportAsync(
        Guid userId, Guid applicationId, CreateInterviewReportDto dto, CancellationToken ct = default)
    {
        var (application, error) = await LoadOwnedByEmployerAsync(userId, applicationId, ct);
        if (error is not null) return ServiceResult<InterviewReportDto>.NotFound(error);

        // WORKFLOWS_AND_STATE_MACHINES.md §9.1: a report follows a Completed or NoShow interview.
        var interview = await uow.Repository<Interview>().FindAsync(i => i.JobApplicationId == application!.Id && i.IsActive, ct: ct);
        if (interview is null || interview.Status is not (InterviewStatus.Completed or InterviewStatus.NoShow))
            return ServiceResult<InterviewReportDto>.Conflict("The interview must be marked Completed or NoShow before writing this report.");

        var alreadyExists = await uow.Repository<InterviewReport>().AnyAsync(r => r.JobApplicationId == application!.Id && r.IsActive, ct);
        if (alreadyExists) return ServiceResult<InterviewReportDto>.Conflict("An interview report already exists for this application.");

        var report = new InterviewReport
        {
            Id = Guid.NewGuid(),
            OfficeId = application!.OfficeId,
            JobApplicationId = application.Id,
            InterviewId = interview.Id,
            Attendance = dto.Attendance,
            InterviewResult = dto.InterviewResult?.Trim(),
            EmployerEvaluation = dto.EmployerEvaluation?.Trim(),
            Strengths = dto.Strengths?.Trim(),
            Concerns = dto.Concerns?.Trim(),
            Recommendation = dto.Recommendation?.Trim(),
            Notes = dto.Notes?.Trim(),
        };
        await uow.Repository<InterviewReport>().AddAsync(report, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<InterviewReportDto>.Success(report.ToDto());
    }

    public async Task<ServiceResult<InterviewReportDto>> GetReportAsync(Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct = default)
    {
        // interviewreports.view has no JobSeeker row in RBAC_PERMISSIONS.md §3's matrix — Office or
        // Employer only; LoadForViewingAsync's JobSeeker branch would still refuse here since no
        // report-type permission is granted to JobSeekerOwnPermissions for this code.
        var (application, error) = await LoadForViewingAsync(userId, officeId, applicationId, ct);
        if (error is not null) return ServiceResult<InterviewReportDto>.NotFound(error);

        var report = await uow.Repository<InterviewReport>().FindAsync(r => r.JobApplicationId == application!.Id && r.IsActive, ct: ct);
        if (report is null) return ServiceResult<InterviewReportDto>.NotFound("No interview report exists for this application.");

        return ServiceResult<InterviewReportDto>.Success(report.ToDto());
    }

    private async Task<(JobApplication? Application, string? Error)> LoadOwnedByEmployerAsync(Guid userId, Guid applicationId, CancellationToken ct)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return (null, "No employer account for this user.");

        var application = await uow.Repository<JobApplication>().GetByIdAsync(applicationId, ct);
        if (application is null) return (null, "Application not found.");

        var adExists = await uow.Repository<JobAdvertisement>().AnyAsync(a => a.Id == application.JobAdvertisementId && a.EmployerId == employer.Id, ct);
        return adExists ? (application, null) : (null, "Application not found.");
    }

    /// <summary>Resolves whichever of the three actors is calling: Office (officeId set), Employer
    /// (own ad), or JobSeeker (own application, read-only) — RBAC_PERMISSIONS.md §3.</summary>
    private async Task<(JobApplication? Application, string? Error)> LoadForViewingAsync(
        Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct)
    {
        var application = await uow.Repository<JobApplication>().GetByIdAsync(applicationId, ct);
        if (application is null) return (null, "Application not found.");

        if (officeId.HasValue)
            return application.OfficeId == officeId.Value ? (application, null) : (null, "Application not found.");

        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is not null)
        {
            var ownsAd = await uow.Repository<JobAdvertisement>().AnyAsync(a => a.Id == application.JobAdvertisementId && a.EmployerId == employer.Id, ct);
            return ownsAd ? (application, null) : (null, "Application not found.");
        }

        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is not null)
            return application.JobSeekerId == profile.Id ? (application, null) : (null, "Application not found.");

        return (null, "Application not found.");
    }
}
