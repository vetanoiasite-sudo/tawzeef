using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

public class JobSeekerReviewService(IUnitOfWork uow) : IJobSeekerReviewService
{
    public async Task<ServiceResult<PagedResult<JobSeekerReviewListItemDto>>> ListQueueAsync(
        Guid officeId, JobSeekerReviewQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<JobSeekerProfile>().PageAsync(
            pageIndex,
            pageSize,
            p => p.OfficeId == officeId
                && (!query.ReviewStatus.HasValue || p.ReviewStatus == query.ReviewStatus.Value)
                && (string.IsNullOrWhiteSpace(query.Name) || p.FullName.Contains(query.Name))
                && (string.IsNullOrWhiteSpace(query.City) || (p.City != null && p.City.Contains(query.City))),
            ct: ct);

        var profileIds = page.Items.Select(p => p.Id).ToList();
        var validComplaints = profileIds.Count == 0
            ? []
            : await uow.Repository<Complaint>().ListAsync(
                c => c.SubjectType == ComplaintSubjectType.JobSeeker
                    && profileIds.Contains(c.SubjectId)
                    && c.ValidityDecision == ComplaintValidityDecision.Valid,
                ct: ct);
        var countsByProfile = validComplaints.GroupBy(c => c.SubjectId).ToDictionary(g => g.Key, g => g.Count());

        return ServiceResult<PagedResult<JobSeekerReviewListItemDto>>.Success(new PagedResult<JobSeekerReviewListItemDto>
        {
            Items = page.Items.Select(p => p.ToListItemDto(countsByProfile.GetValueOrDefault(p.Id, 0))).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == id && p.OfficeId == officeId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("Job seeker profile not found.");

        // WORKFLOWS_AND_STATE_MACHINES.md §1: "Submitted -> UnderReview ... may be implicit the
        // moment a reviewer opens the record" — this is that moment.
        if (profile.ReviewStatus == JobSeekerReviewStatus.Submitted)
        {
            profile.ReviewStatus = JobSeekerReviewStatus.UnderReview;
            uow.Repository<JobSeekerProfile>().Update(profile);
            await uow.SaveChangesAsync(ct);
        }

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        var validComplaintCount = await CountValidComplaintsAsync(profile.Id, ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, validComplaintCount));
    }

    private async Task<int> CountValidComplaintsAsync(Guid jobSeekerProfileId, CancellationToken ct) =>
        await uow.Repository<Complaint>().CountAsync(
            c => c.SubjectType == ComplaintSubjectType.JobSeeker
                && c.SubjectId == jobSeekerProfileId
                && c.ValidityDecision == ComplaintValidityDecision.Valid,
            ct);

    public async Task<ServiceResult<JobSeekerDocumentDto>> ApproveDocumentAsync(
        Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, CancellationToken ct = default)
    {
        var (document, error) = await LoadReviewableDocumentAsync(officeId, profileId, documentId, ct);
        if (error is not null) return ServiceResult<JobSeekerDocumentDto>.NotFound(error);
        if (document!.ReviewStatus != DocumentReviewStatus.Pending)
            return ServiceResult<JobSeekerDocumentDto>.Conflict("This document has already been reviewed.");

        document.ReviewStatus = DocumentReviewStatus.Approved;
        document.ReviewedBy = reviewerId;
        document.ReviewedAt = DateTime.UtcNow;
        document.RejectionReason = null;
        uow.Repository<JobSeekerDocument>().Update(document);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerDocumentDto>.Success(document.ToDto());
    }

    public async Task<ServiceResult<JobSeekerDocumentDto>> RejectDocumentAsync(
        Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, string reason, CancellationToken ct = default)
    {
        var (document, error) = await LoadReviewableDocumentAsync(officeId, profileId, documentId, ct);
        if (error is not null) return ServiceResult<JobSeekerDocumentDto>.NotFound(error);
        if (document!.ReviewStatus != DocumentReviewStatus.Pending)
            return ServiceResult<JobSeekerDocumentDto>.Conflict("This document has already been reviewed.");

        document.ReviewStatus = DocumentReviewStatus.Rejected;
        document.ReviewedBy = reviewerId;
        document.ReviewedAt = DateTime.UtcNow;
        document.RejectionReason = reason.Trim();
        uow.Repository<JobSeekerDocument>().Update(document);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerDocumentDto>.Success(document.ToDto());
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> ApproveProfileAsync(
        Guid officeId, Guid reviewerId, Guid profileId, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == profileId && p.OfficeId == officeId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("Job seeker profile not found.");
        if (profile.ReviewStatus != JobSeekerReviewStatus.UnderReview)
            return ServiceResult<JobSeekerProfileDto>.Conflict("The profile is not under review.");

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);

        // WORKFLOWS_AND_STATE_MACHINES.md §1: can only reach Approved once every active document
        // is individually Approved.
        var approvedTypes = documents.Where(d => d.ReviewStatus == DocumentReviewStatus.Approved).Select(d => d.DocumentType).ToHashSet();
        var notReady = Enum.GetValues<JobSeekerDocumentType>().Where(t => !approvedTypes.Contains(t)).ToList();
        if (notReady.Count > 0)
            return ServiceResult<JobSeekerProfileDto>.Invalid(
                $"Every required document must be approved first — not yet approved: {string.Join(", ", notReady)}.");

        profile.ReviewStatus = JobSeekerReviewStatus.Approved;
        profile.ApprovedBy = reviewerId;
        profile.ApprovedAt = DateTime.UtcNow;
        profile.RejectionReason = null;
        uow.Repository<JobSeekerProfile>().Update(profile);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, await CountValidComplaintsAsync(profile.Id, ct)));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> RejectProfileAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == profileId && p.OfficeId == officeId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("Job seeker profile not found.");
        if (profile.ReviewStatus != JobSeekerReviewStatus.UnderReview)
            return ServiceResult<JobSeekerProfileDto>.Conflict("The profile is not under review.");

        profile.ReviewStatus = JobSeekerReviewStatus.Rejected;
        profile.RejectionReason = reason.Trim();
        uow.Repository<JobSeekerProfile>().Update(profile);
        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, await CountValidComplaintsAsync(profile.Id, ct)));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> SuspendAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == profileId && p.OfficeId == officeId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("Job seeker profile not found.");

        // WORKFLOWS_AND_STATE_MACHINES.md §1: Approved -> Suspended only.
        if (profile.ReviewStatus != JobSeekerReviewStatus.Approved)
            return ServiceResult<JobSeekerProfileDto>.Conflict("Only an approved profile can be suspended.");

        profile.ReviewStatus = JobSeekerReviewStatus.Suspended;
        profile.SuspendedAt = DateTime.UtcNow;
        profile.SuspensionReason = reason.Trim();
        uow.Repository<JobSeekerProfile>().Update(profile);

        await uow.Repository<OfficeDecision>().AddAsync(new OfficeDecision
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            TargetType = OfficeDecisionTargetType.JobSeeker,
            TargetId = profile.Id,
            DecisionType = OfficeDecisionType.Suspend,
            Reason = reason.Trim(),
            DecidedBy = reviewerId,
        }, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, await CountValidComplaintsAsync(profile.Id, ct)));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> ReinstateAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == profileId && p.OfficeId == officeId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerProfileDto>.NotFound("Job seeker profile not found.");

        // WORKFLOWS_AND_STATE_MACHINES.md §1: Suspended -> Approved only.
        if (profile.ReviewStatus != JobSeekerReviewStatus.Suspended)
            return ServiceResult<JobSeekerProfileDto>.Conflict("Only a suspended profile can be reinstated.");

        profile.ReviewStatus = JobSeekerReviewStatus.Approved;
        profile.SuspendedAt = null;
        profile.SuspensionReason = null;
        uow.Repository<JobSeekerProfile>().Update(profile);

        await uow.Repository<OfficeDecision>().AddAsync(new OfficeDecision
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            TargetType = OfficeDecisionTargetType.JobSeeker,
            TargetId = profile.Id,
            DecisionType = OfficeDecisionType.Reinstate,
            Reason = reason.Trim(),
            DecidedBy = reviewerId,
        }, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, await CountValidComplaintsAsync(profile.Id, ct)));
    }

    private async Task<(JobSeekerDocument? Document, string? Error)> LoadReviewableDocumentAsync(
        Guid officeId, Guid profileId, Guid documentId, CancellationToken ct)
    {
        var profileExists = await uow.Repository<JobSeekerProfile>().AnyAsync(p => p.Id == profileId && p.OfficeId == officeId, ct);
        if (!profileExists) return (null, "Job seeker profile not found.");

        var document = await uow.Repository<JobSeekerDocument>().FindAsync(
            d => d.Id == documentId && d.JobSeekerId == profileId && d.IsActive, ct: ct);
        return document is null ? (null, "Document not found.") : (document, null);
    }
}
