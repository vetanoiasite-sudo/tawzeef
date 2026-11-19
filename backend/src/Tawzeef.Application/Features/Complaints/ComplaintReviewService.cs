using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Employers;
using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Complaints;

public class ComplaintReviewService(
    IUnitOfWork uow,
    IJobSeekerReviewService jobSeekerReviewService,
    IEmployerReviewService employerReviewService,
    INotificationService notificationService) : IComplaintReviewService
{
    private const int ComplaintThreshold = 3;

    public async Task<ServiceResult<PagedResult<ComplaintDto>>> ListQueueAsync(
        Guid officeId, ComplaintQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<Complaint>().PageAsync(
            pageIndex,
            pageSize,
            c => c.OfficeId == officeId
                && (!query.Status.HasValue || c.Status == query.Status.Value)
                && (!query.ValidityDecision.HasValue || c.ValidityDecision == query.ValidityDecision.Value)
                && (!query.SubjectType.HasValue || c.SubjectType == query.SubjectType.Value),
            ct: ct);

        var names = await ComplaintSubjectResolver.ResolveNamesAsync(uow, page.Items, ct);

        return ServiceResult<PagedResult<ComplaintDto>>.Success(new PagedResult<ComplaintDto>
        {
            Items = page.Items.Select(c => c.ToDto(names.GetValueOrDefault(c.SubjectId, string.Empty))).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<ComplaintDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var complaint = await uow.Repository<Complaint>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (complaint is null) return ServiceResult<ComplaintDto>.NotFound("Complaint not found.");

        // Same implicit-transition convention as the job seeker review queue: opening a New
        // complaint is "Office opens it" (WORKFLOWS_AND_STATE_MACHINES.md §10.1) — no separate
        // endpoint for it.
        if (complaint.Status == ComplaintStatus.New)
        {
            complaint.Status = ComplaintStatus.UnderReview;
            uow.Repository<Complaint>().Update(complaint);
            await uow.SaveChangesAsync(ct);
        }

        var subjectName = await ComplaintSubjectResolver.ResolveOneNameAsync(uow, complaint.SubjectType, complaint.SubjectId, ct);
        return ServiceResult<ComplaintDto>.Success(complaint.ToDto(subjectName));
    }

    public async Task<ServiceResult<ComplaintDto>> TriageAsync(
        Guid officeId, Guid reviewerId, Guid id, ComplaintValidityDecision validityDecision, CancellationToken ct = default)
    {
        if (validityDecision == ComplaintValidityDecision.Pending)
            return ServiceResult<ComplaintDto>.Invalid("A triage decision must be Valid or Invalid.");

        var complaint = await uow.Repository<Complaint>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (complaint is null) return ServiceResult<ComplaintDto>.NotFound("Complaint not found.");

        if (complaint.Status is not (ComplaintStatus.UnderReview or ComplaintStatus.WaitingForInformation))
            return ServiceResult<ComplaintDto>.Conflict("Only a complaint under review can be triaged.");

        complaint.ValidityDecision = validityDecision;
        complaint.ReviewedBy = reviewerId;
        complaint.ReviewedAt = DateTime.UtcNow;
        uow.Repository<Complaint>().Update(complaint);

        // WORKFLOWS_AND_STATE_MACHINES.md §10.3: hitting the threshold only ever raises an
        // alert/task for the office — never an automatic suspension. Fired exactly once, the
        // triage that makes the count *reach* 3 (not every triage past it), so OfficeAdmins get one
        // notification per subject, not one per complaint.
        if (validityDecision == ComplaintValidityDecision.Valid)
        {
            // Counts against the database, which doesn't see this complaint's own pending change
            // yet (SaveChangesAsync hasn't run) — exclude it by id and add 1 for it explicitly,
            // rather than counting first and saving second, which would need a second commit.
            var otherValidCount = await uow.Repository<Complaint>().CountAsync(
                c => c.Id != complaint.Id && c.SubjectType == complaint.SubjectType && c.SubjectId == complaint.SubjectId && c.ValidityDecision == ComplaintValidityDecision.Valid, ct);
            if (otherValidCount + 1 == ComplaintThreshold)
            {
                var subjectNameForAlert = await ComplaintSubjectResolver.ResolveOneNameAsync(uow, complaint.SubjectType, complaint.SubjectId, ct);
                await notificationService.CreateForOfficeAdminsAsync(
                    officeId, "Complaint.ThresholdReached",
                    $"وصلت الشكاوى الصحيحة ضد \"{subjectNameForAlert}\" إلى 3 — يحتاج قرار من إدارة المكتب.",
                    complaint.SubjectType.ToString(), complaint.SubjectId, ct);
            }
        }

        await uow.SaveChangesAsync(ct);

        var subjectName = await ComplaintSubjectResolver.ResolveOneNameAsync(uow, complaint.SubjectType, complaint.SubjectId, ct);
        return ServiceResult<ComplaintDto>.Success(complaint.ToDto(subjectName));
    }

    public async Task<ServiceResult<ComplaintDto>> ResolveAsync(
        Guid officeId, Guid reviewerId, Guid id, ComplaintStatus outcome, string resolutionNotes, CancellationToken ct = default)
    {
        if (outcome is not (ComplaintStatus.Resolved or ComplaintStatus.Rejected))
            return ServiceResult<ComplaintDto>.Invalid("The outcome must be Resolved or Rejected.");

        var complaint = await uow.Repository<Complaint>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (complaint is null) return ServiceResult<ComplaintDto>.NotFound("Complaint not found.");

        if (complaint.Status is not (ComplaintStatus.UnderReview or ComplaintStatus.WaitingForInformation))
            return ServiceResult<ComplaintDto>.Conflict("Only a complaint under review can be resolved.");

        complaint.Status = outcome;
        complaint.ResolutionNotes = resolutionNotes.Trim();
        complaint.ReviewedBy = reviewerId;
        complaint.ReviewedAt = DateTime.UtcNow;
        uow.Repository<Complaint>().Update(complaint);
        await uow.SaveChangesAsync(ct);

        var subjectName = await ComplaintSubjectResolver.ResolveOneNameAsync(uow, complaint.SubjectType, complaint.SubjectId, ct);
        return ServiceResult<ComplaintDto>.Success(complaint.ToDto(subjectName));
    }

    public async Task<ServiceResult<DecideResultDto>> DecideAsync(
        Guid officeId, Guid reviewerId, Guid id, OfficeDecisionType decisionType, string reason, CancellationToken ct = default)
    {
        var complaint = await uow.Repository<Complaint>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (complaint is null) return ServiceResult<DecideResultDto>.NotFound("Complaint not found.");

        // BUSINESS_REQUIREMENTS.md §11.5 step 3/RBAC_PERMISSIONS.md §0 item 5: only a
        // confirmed-valid complaint may lead to an enforcement decision.
        if (complaint.ValidityDecision != ComplaintValidityDecision.Valid)
            return ServiceResult<DecideResultDto>.Invalid("Only a complaint confirmed valid can lead to an enforcement decision.");

        // Reuses JobSeekerReviewService.Suspend/ReinstateAsync and EmployerReviewService.Stop/
        // ResumeCooperationAsync exactly — those are where the real status transition happens and
        // the OfficeDecision audit row is written (API_DOCUMENTATION.md §3.10/§3.5: "the only
        // endpoint that may change ..." is read here as "the only code path", not literally one
        // URL — the direct /office/job-seekers|employers/{id}/... routes and this one both funnel
        // through it).
        if (complaint.SubjectType == ComplaintSubjectType.JobSeeker)
        {
            var result = decisionType switch
            {
                OfficeDecisionType.Suspend => await jobSeekerReviewService.SuspendAsync(officeId, reviewerId, complaint.SubjectId, reason, ct),
                OfficeDecisionType.Reinstate => await jobSeekerReviewService.ReinstateAsync(officeId, reviewerId, complaint.SubjectId, reason, ct),
                _ => ServiceResult<JobSeekerProfileDto>.Invalid("That decision type doesn't apply to a Job Seeker subject."),
            };
            if (!result.IsSuccess) return PropagateError<JobSeekerProfileDto>(result);
            return ServiceResult<DecideResultDto>.Success(new DecideResultDto
            {
                SubjectId = result.Data!.Id,
                SubjectName = result.Data.FullName,
                Status = result.Data.ReviewStatus,
            });
        }
        else
        {
            var result = decisionType switch
            {
                OfficeDecisionType.StopCooperation => await employerReviewService.StopCooperationAsync(officeId, reviewerId, complaint.SubjectId, reason, ct),
                OfficeDecisionType.ResumeCooperation => await employerReviewService.ResumeCooperationAsync(officeId, reviewerId, complaint.SubjectId, reason, ct),
                _ => ServiceResult<EmployerProfileDto>.Invalid("That decision type doesn't apply to an Employer subject."),
            };
            if (!result.IsSuccess) return PropagateError<EmployerProfileDto>(result);
            return ServiceResult<DecideResultDto>.Success(new DecideResultDto
            {
                SubjectId = result.Data!.Id,
                SubjectName = result.Data.DisplayName,
                Status = result.Data.CooperationStatus,
            });
        }
    }

    /// <summary>ServiceResult.Fail is protected, so a failed ServiceResult&lt;TSource&gt; can't be
    /// reshaped into a ServiceResult&lt;DecideResultDto&gt; directly — re-derive it from the public
    /// factory that matches the source's Error category instead.</summary>
    private static ServiceResult<DecideResultDto> PropagateError<TSource>(ServiceResult<TSource> source) => source.Error switch
    {
        ResultError.NotFound => ServiceResult<DecideResultDto>.NotFound(source.ErrorMessages.ToArray()),
        ResultError.Conflict => ServiceResult<DecideResultDto>.Conflict(source.ErrorMessages.ToArray()),
        ResultError.Forbidden => ServiceResult<DecideResultDto>.Forbidden(source.ErrorMessages.ToArray()),
        _ => ServiceResult<DecideResultDto>.Invalid(source.ErrorMessages.ToArray()),
    };
}
