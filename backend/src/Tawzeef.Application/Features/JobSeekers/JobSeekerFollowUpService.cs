using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Interviews;
using Tawzeef.Domain.Entities.JobApplications;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

public class JobSeekerFollowUpService(IUnitOfWork uow, INotificationService notificationService) : IJobSeekerFollowUpService
{
    public async Task<ServiceResult> RespondToEmploymentCheckAsync(
        Guid userId, Guid officeApplicationId, RespondEmploymentCheckDto dto, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult.NotFound("No job seeker profile for this user.");

        var application = await uow.Repository<JobSeekerOfficeApplication>().FindAsync(
            a => a.Id == officeApplicationId && a.JobSeekerId == profile.Id, ct: ct);
        if (application is null) return ServiceResult.NotFound("Office application not found.");

        if (application.EmployedAt is not null)
            return ServiceResult.Conflict("Already marked employed.");

        if (!dto.FoundJob)
        {
            // Still searching — ask again in a month, same cadence as the first check.
            application.NextEmploymentCheckDueAt = DateTime.UtcNow.AddMonths(1);
            uow.Repository<JobSeekerOfficeApplication>().Update(application);
            await uow.SaveChangesAsync(ct);
            return ServiceResult.Success();
        }

        var now = DateTime.UtcNow;
        application.EmployedAt = now;
        application.NextEmploymentCheckDueAt = null;
        uow.Repository<JobSeekerOfficeApplication>().Update(application);

        var report = new HiringFollowUpReport
        {
            Id = Guid.NewGuid(),
            OfficeId = application.OfficeId,
            JobSeekerOfficeApplicationId = application.Id,
            HiredAt = now,
            DueAt = now.AddMonths(1),
            Status = FollowUpReportStatus.PendingDue,
            SelfReported = true,
        };
        await uow.Repository<HiringFollowUpReport>().AddAsync(report, ct);

        await notificationService.CreateForOfficeAdminsAsync(
            application.OfficeId, "JobSeekerEmployment.Started",
            $"الباحث \"{profile.FullName}\" أبلغ أنه حصل على عمل.", nameof(JobSeekerProfile), profile.Id, ct);

        await uow.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RespondToHiringFollowUpAsync(
        Guid userId, Guid reportId, RespondHiringFollowUpDto dto, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult.NotFound("No job seeker profile for this user.");

        var report = await uow.Repository<HiringFollowUpReport>().FindAsync(r => r.Id == reportId && r.IsActive, ct: ct);
        if (report is null) return ServiceResult.NotFound("Follow-up report not found.");
        if (report.Status == FollowUpReportStatus.Completed) return ServiceResult.Conflict("This follow-up report has already been completed.");

        var (ownsReport, officeApplication) = await ResolveOwnershipAsync(report, profile.Id, ct);
        if (!ownsReport) return ServiceResult.NotFound("Follow-up report not found.");

        report.StillEmployed = dto.StillEmployed;
        report.Notes = dto.Notes?.Trim();
        report.Status = FollowUpReportStatus.Completed;
        report.CompletedAt = DateTime.UtcNow;
        uow.Repository<HiringFollowUpReport>().Update(report);

        if (!dto.StillEmployed && officeApplication is not null)
        {
            officeApplication.LeftJobAt = DateTime.UtcNow;
            officeApplication.LeftJobNotes = dto.Notes?.Trim();
            officeApplication.NextEmploymentCheckDueAt = DateTime.UtcNow.AddMonths(1);
            uow.Repository<JobSeekerOfficeApplication>().Update(officeApplication);

            await notificationService.CreateForOfficeAdminsAsync(
                officeApplication.OfficeId, "JobSeekerEmployment.LeftJob",
                $"الباحث \"{profile.FullName}\" ترك عمله وعاد للبحث عن وظيفة.", nameof(JobSeekerProfile), profile.Id, ct);
        }

        await uow.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RespondToInterviewFeedbackAsync(
        Guid userId, Guid interviewId, RespondInterviewFeedbackDto dto, CancellationToken ct = default)
    {
        if (dto.Outcome == InterviewSeekerFeedback.Pending)
            return ServiceResult.Invalid("Outcome must be StillWaiting, Accepted, or Rejected.");

        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult.NotFound("No job seeker profile for this user.");

        var interview = await uow.Repository<Interview>().FindAsync(i => i.Id == interviewId && i.IsActive, ct: ct);
        if (interview is null) return ServiceResult.NotFound("Interview not found.");

        var application = await uow.Repository<JobApplication>().FindAsync(
            a => a.Id == interview.JobApplicationId && a.JobSeekerId == profile.Id, ct: ct);
        if (application is null) return ServiceResult.NotFound("Interview not found.");

        interview.JobSeekerFeedbackOutcome = dto.Outcome;
        interview.JobSeekerFeedbackNotes = dto.Notes?.Trim();
        interview.JobSeekerFeedbackAt = DateTime.UtcNow;
        uow.Repository<Interview>().Update(interview);

        await uow.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    /// <summary>A report reaches this job seeker either through JobSeekerOfficeApplicationId
    /// (self-reported) or through JobApplicationId -> JobApplication.JobSeekerId (office-recorded
    /// hire) — resolve whichever is set and confirm it is actually this profile's own.</summary>
    private async Task<(bool Owns, JobSeekerOfficeApplication? OfficeApplication)> ResolveOwnershipAsync(
        HiringFollowUpReport report, Guid profileId, CancellationToken ct)
    {
        if (report.JobSeekerOfficeApplicationId is not null)
        {
            var officeApplication = await uow.Repository<JobSeekerOfficeApplication>().FindAsync(
                a => a.Id == report.JobSeekerOfficeApplicationId && a.JobSeekerId == profileId, ct: ct);
            return officeApplication is null ? (false, null) : (true, officeApplication);
        }

        if (report.JobApplicationId is not null)
        {
            var jobApplication = await uow.Repository<JobApplication>().FindAsync(
                a => a.Id == report.JobApplicationId && a.JobSeekerId == profileId, ct: ct);
            if (jobApplication is null) return (false, null);

            var officeApplication = await uow.Repository<JobSeekerOfficeApplication>().FindAsync(
                a => a.JobSeekerId == profileId && a.OfficeId == jobApplication.OfficeId, ct: ct);
            return (true, officeApplication);
        }

        return (false, null);
    }
}
