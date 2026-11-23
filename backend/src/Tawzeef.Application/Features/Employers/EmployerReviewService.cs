using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Employers;

public class EmployerReviewService(IUnitOfWork uow, IFileStorage fileStorage) : IEmployerReviewService
{
    /// <summary>employers.viewdocuments — a reviewer streams the actual file, not just its
    /// review-status metadata. Reuses the same office+employer ownership check every other
    /// document action on this employer already goes through.</summary>
    public async Task<ServiceResult<EmployerDocumentFileDto>> GetDocumentFileAsync(
        Guid officeId, Guid employerId, Guid documentId, CancellationToken ct = default)
    {
        var (document, error) = await LoadReviewableDocumentAsync(officeId, employerId, documentId, ct);
        if (error is not null) return ServiceResult<EmployerDocumentFileDto>.NotFound(error);

        var stream = await fileStorage.OpenReadAsync(document!.SecureFileReference, ct);
        return ServiceResult<EmployerDocumentFileDto>.Success(new EmployerDocumentFileDto
        {
            Content = stream,
            FileName = document.OriginalFileName,
            ContentType = document.MimeType,
        });
    }

    public async Task<ServiceResult<PagedResult<EmployerReviewListItemDto>>> ListQueueAsync(
        Guid officeId, EmployerReviewQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<Employer>().PageAsync(
            pageIndex,
            pageSize,
            e => e.OfficeId == officeId
                && (!query.VerificationStatus.HasValue || e.VerificationStatus == query.VerificationStatus.Value)
                && (string.IsNullOrWhiteSpace(query.DisplayName) || e.DisplayName.Contains(query.DisplayName))
                && (string.IsNullOrWhiteSpace(query.City) || (e.City != null && e.City.Contains(query.City))),
            ct: ct);

        var employerIds = page.Items.Select(e => e.Id).ToList();
        var validComplaints = employerIds.Count == 0
            ? []
            : await uow.Repository<Complaint>().ListAsync(
                c => c.SubjectType == ComplaintSubjectType.Employer
                    && employerIds.Contains(c.SubjectId)
                    && c.ValidityDecision == ComplaintValidityDecision.Valid,
                ct: ct);
        var countsByEmployer = validComplaints.GroupBy(c => c.SubjectId).ToDictionary(g => g.Key, g => g.Count());

        return ServiceResult<PagedResult<EmployerReviewListItemDto>>.Success(new PagedResult<EmployerReviewListItemDto>
        {
            Items = page.Items.Select(e => e.ToListItemDto(countsByEmployer.GetValueOrDefault(e.Id, 0))).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<EmployerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == id && e.OfficeId == officeId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("صاحب العمل غير موجود.");

        // Same implicit-transition convention as the job seeker review queue.
        if (employer.VerificationStatus == EmployerVerificationStatus.Submitted)
        {
            employer.VerificationStatus = EmployerVerificationStatus.UnderReview;
            uow.Repository<Employer>().Update(employer);
            await uow.SaveChangesAsync(ct);
        }

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        var validComplaintCount = await CountValidComplaintsAsync(employer.Id, ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, validComplaintCount));
    }

    public async Task<ServiceResult<EmployerProfileDto>> UpdateAsync(
        Guid officeId, Guid employerId, UpdateEmployerProfileDto dto, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == employerId && e.OfficeId == officeId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("صاحب العمل غير موجود.");

        // Same field set as EmployerProfileService.UpdateMyProfileAsync — deliberately no
        // RevertApprovalIfNeeded here (this method's own doc comment explains why).
        employer.BusinessActivity = dto.BusinessActivity?.Trim();
        employer.Address = dto.Address?.Trim();
        employer.City = dto.City?.Trim();
        employer.CompanyEmail = dto.CompanyEmail?.Trim();
        employer.CompanyPhone = dto.CompanyPhone?.Trim();
        employer.RepresentativeName = dto.RepresentativeName?.Trim();
        employer.RepresentativeRole = dto.RepresentativeRole?.Trim();

        uow.Repository<Employer>().Update(employer);
        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        var validComplaintCount = await CountValidComplaintsAsync(employer.Id, ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, validComplaintCount));
    }

    private async Task<int> CountValidComplaintsAsync(Guid employerId, CancellationToken ct) =>
        await uow.Repository<Complaint>().CountAsync(
            c => c.SubjectType == ComplaintSubjectType.Employer
                && c.SubjectId == employerId
                && c.ValidityDecision == ComplaintValidityDecision.Valid,
            ct);

    public async Task<ServiceResult<EmployerDocumentDto>> ApproveDocumentAsync(
        Guid officeId, Guid reviewerId, Guid employerId, Guid documentId, CancellationToken ct = default)
    {
        var (document, error) = await LoadReviewableDocumentAsync(officeId, employerId, documentId, ct);
        if (error is not null) return ServiceResult<EmployerDocumentDto>.NotFound(error);

        // Re-review is allowed from any prior status (Pending, Approved, or Rejected) — an office
        // reviewer must be able to correct a mistaken decision, not just a Pending one.
        document!.ReviewStatus = DocumentReviewStatus.Approved;
        document.ReviewedBy = reviewerId;
        document.ReviewedAt = DateTime.UtcNow;
        document.RejectionReason = null;
        uow.Repository<EmployerDocument>().Update(document);

        // No separate manual "approve the whole profile" click — the profile auto-advances the
        // instant the last required document type is individually approved (WORKFLOWS_AND_STATE_
        // MACHINES.md §3's "every document Approved" gate still applies, just checked here instead
        // of behind a redundant extra button).
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == employerId && e.OfficeId == officeId, ct: ct);
        if (employer is not null && employer.VerificationStatus == EmployerVerificationStatus.UnderReview)
        {
            var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
            var approvedTypes = documents
                .Where(d => d.ReviewStatus == DocumentReviewStatus.Approved)
                .Select(d => d.DocumentType)
                .ToHashSet();
            var allApproved = Enum.GetValues<EmployerDocumentType>().All(approvedTypes.Contains);
            if (allApproved)
            {
                employer.VerificationStatus = EmployerVerificationStatus.Approved;
                employer.ApprovedBy = reviewerId;
                employer.ApprovedAt = DateTime.UtcNow;
                employer.RejectionReason = null;
                uow.Repository<Employer>().Update(employer);
            }
        }

        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerDocumentDto>.Success(document.ToDto());
    }

    public async Task<ServiceResult<EmployerDocumentDto>> RejectDocumentAsync(
        Guid officeId, Guid reviewerId, Guid employerId, Guid documentId, string reason, CancellationToken ct = default)
    {
        var (document, error) = await LoadReviewableDocumentAsync(officeId, employerId, documentId, ct);
        if (error is not null) return ServiceResult<EmployerDocumentDto>.NotFound(error);

        // Re-review allowed from any prior status — see ApproveDocumentAsync's comment above.
        document!.ReviewStatus = DocumentReviewStatus.Rejected;
        document.ReviewedBy = reviewerId;
        document.ReviewedAt = DateTime.UtcNow;
        document.RejectionReason = reason.Trim();
        uow.Repository<EmployerDocument>().Update(document);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerDocumentDto>.Success(document.ToDto());
    }

    public async Task<ServiceResult<EmployerProfileDto>> ApproveProfileAsync(
        Guid officeId, Guid reviewerId, Guid employerId, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == employerId && e.OfficeId == officeId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("صاحب العمل غير موجود.");
        if (employer.VerificationStatus != EmployerVerificationStatus.UnderReview)
            return ServiceResult<EmployerProfileDto>.Conflict("ملف الشركة ليس قيد المراجعة.");

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);

        // WORKFLOWS_AND_STATE_MACHINES.md §3: can only reach Approved once every active document
        // is individually Approved.
        var approvedTypes = documents.Where(d => d.ReviewStatus == DocumentReviewStatus.Approved).Select(d => d.DocumentType).ToHashSet();
        var notReady = Enum.GetValues<EmployerDocumentType>().Where(t => !approvedTypes.Contains(t)).ToList();
        if (notReady.Count > 0)
            return ServiceResult<EmployerProfileDto>.Invalid(
                $"يجب الموافقة على كل المستندات المطلوبة أولًا — لم تتم الموافقة على: {string.Join(", ", notReady)}.");

        employer.VerificationStatus = EmployerVerificationStatus.Approved;
        employer.ApprovedBy = reviewerId;
        employer.ApprovedAt = DateTime.UtcNow;
        employer.RejectionReason = null;
        uow.Repository<Employer>().Update(employer);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, await CountValidComplaintsAsync(employer.Id, ct)));
    }

    public async Task<ServiceResult<EmployerProfileDto>> RejectProfileAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == employerId && e.OfficeId == officeId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("صاحب العمل غير موجود.");
        if (employer.VerificationStatus != EmployerVerificationStatus.UnderReview)
            return ServiceResult<EmployerProfileDto>.Conflict("ملف الشركة ليس قيد المراجعة.");

        employer.VerificationStatus = EmployerVerificationStatus.Rejected;
        employer.RejectionReason = reason.Trim();
        uow.Repository<Employer>().Update(employer);
        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, await CountValidComplaintsAsync(employer.Id, ct)));
    }

    public async Task<ServiceResult<EmployerProfileDto>> StopCooperationAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == employerId && e.OfficeId == officeId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("صاحب العمل غير موجود.");

        // WORKFLOWS_AND_STATE_MACHINES.md §4: Active -> Stopped only.
        if (employer.CooperationStatus != EmployerCooperationStatus.Active)
            return ServiceResult<EmployerProfileDto>.Conflict("التعاون متوقف بالفعل.");

        employer.CooperationStatus = EmployerCooperationStatus.Stopped;
        employer.CooperationStoppedAt = DateTime.UtcNow;
        employer.CooperationStoppedReason = reason.Trim();
        uow.Repository<Employer>().Update(employer);

        await uow.Repository<OfficeDecision>().AddAsync(new OfficeDecision
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            TargetType = OfficeDecisionTargetType.Employer,
            TargetId = employer.Id,
            DecisionType = OfficeDecisionType.StopCooperation,
            Reason = reason.Trim(),
            DecidedBy = reviewerId,
        }, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, await CountValidComplaintsAsync(employer.Id, ct)));
    }

    public async Task<ServiceResult<EmployerProfileDto>> ResumeCooperationAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == employerId && e.OfficeId == officeId, ct: ct);
        if (employer is null) return ServiceResult<EmployerProfileDto>.NotFound("صاحب العمل غير موجود.");

        // WORKFLOWS_AND_STATE_MACHINES.md §4: Stopped -> Active only.
        if (employer.CooperationStatus != EmployerCooperationStatus.Stopped)
            return ServiceResult<EmployerProfileDto>.Conflict("التعاون غير متوقف حاليًا.");

        employer.CooperationStatus = EmployerCooperationStatus.Active;
        employer.CooperationStoppedAt = null;
        employer.CooperationStoppedReason = null;
        uow.Repository<Employer>().Update(employer);

        await uow.Repository<OfficeDecision>().AddAsync(new OfficeDecision
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            TargetType = OfficeDecisionTargetType.Employer,
            TargetId = employer.Id,
            DecisionType = OfficeDecisionType.ResumeCooperation,
            Reason = reason.Trim(),
            DecidedBy = reviewerId,
        }, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, await CountValidComplaintsAsync(employer.Id, ct)));
    }

    private async Task<(EmployerDocument? Document, string? Error)> LoadReviewableDocumentAsync(
        Guid officeId, Guid employerId, Guid documentId, CancellationToken ct)
    {
        var employerExists = await uow.Repository<Employer>().AnyAsync(e => e.Id == employerId && e.OfficeId == officeId, ct);
        if (!employerExists) return (null, "صاحب العمل غير موجود.");

        var document = await uow.Repository<EmployerDocument>().FindAsync(
            d => d.Id == documentId && d.EmployerId == employerId && d.IsActive, ct: ct);
        return document is null ? (null, "الملف غير موجود.") : (document, null);
    }
}
