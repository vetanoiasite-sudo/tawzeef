using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Employers;

public class EmployerReviewService(IUnitOfWork uow, IFileStorage fileStorage, INotificationService notificationService) : IEmployerReviewService
{
    private static string DocumentTypeLabel(EmployerDocumentType t) => t switch
    {
        EmployerDocumentType.CommercialRegister => "السجل التجاري",
        EmployerDocumentType.TaxCard => "البطاقة الضريبية",
        EmployerDocumentType.OwnerNationalId => "بطاقة الرقم القومي للمالك",
        _ => t.ToString(),
    };

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

        // No cross-entity join in IRepository — composed in memory, same trade-off as
        // JobSeekerReviewService.ListQueueAsync.
        var applications = await uow.Repository<EmployerOfficeApplication>().ListAsync(
            a => a.OfficeId == officeId && (!query.VerificationStatus.HasValue || a.VerificationStatus == query.VerificationStatus.Value), ct: ct);
        if (applications.Count == 0)
            return ServiceResult<PagedResult<EmployerReviewListItemDto>>.Success(new PagedResult<EmployerReviewListItemDto>
            { Items = [], PageIndex = pageIndex, PageSize = pageSize, TotalCount = 0 });

        var applicationsByEmployer = applications.ToDictionary(a => a.EmployerId);
        var employerIds = applicationsByEmployer.Keys.ToList();

        var employers = await uow.Repository<Employer>().ListAsync(
            e => employerIds.Contains(e.Id)
                && (string.IsNullOrWhiteSpace(query.DisplayName) || e.DisplayName.Contains(query.DisplayName))
                && (string.IsNullOrWhiteSpace(query.City) || (e.City != null && e.City.Contains(query.City))),
            ct: ct);

        var ordered = employers
            .OrderByDescending(e => applicationsByEmployer[e.Id].ModifiedDate ?? applicationsByEmployer[e.Id].CreationDate)
            .ToList();

        var validComplaints = ordered.Count == 0
            ? []
            : await uow.Repository<Complaint>().ListAsync(
                c => c.SubjectType == ComplaintSubjectType.Employer
                    && employerIds.Contains(c.SubjectId)
                    && c.ValidityDecision == ComplaintValidityDecision.Valid,
                ct: ct);
        var countsByEmployer = validComplaints.GroupBy(c => c.SubjectId).ToDictionary(g => g.Key, g => g.Count());

        var page = ordered.Skip(pageIndex * pageSize).Take(pageSize).ToList();

        return ServiceResult<PagedResult<EmployerReviewListItemDto>>.Success(new PagedResult<EmployerReviewListItemDto>
        {
            Items = page.Select(e => e.ToListItemDto(applicationsByEmployer[e.Id], countsByEmployer.GetValueOrDefault(e.Id, 0))).ToList(),
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = ordered.Count,
        });
    }

    public async Task<ServiceResult<EmployerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var (employer, application, error) = await LoadReviewableEmployerWithApplicationAsync(officeId, id, ct);
        if (error is not null) return ServiceResult<EmployerProfileDto>.NotFound(error);

        // Same implicit-transition convention as the job seeker review queue.
        if (application!.VerificationStatus == EmployerVerificationStatus.Submitted)
        {
            application.VerificationStatus = EmployerVerificationStatus.UnderReview;
            uow.Repository<EmployerOfficeApplication>().Update(application);
            await uow.SaveChangesAsync(ct);
        }

        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == officeId, ct: ct);
        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer!.Id && d.IsActive, ct: ct);
        var validComplaintCount = await CountValidComplaintsAsync(employer!.Id, ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, validComplaintCount, application, office));
    }

    public async Task<ServiceResult<EmployerProfileDto>> UpdateAsync(
        Guid officeId, Guid employerId, UpdateEmployerProfileDto dto, CancellationToken ct = default)
    {
        var (employer, application, error) = await LoadReviewableEmployerWithApplicationAsync(officeId, employerId, ct);
        if (error is not null) return ServiceResult<EmployerProfileDto>.NotFound(error);

        // Same field set as EmployerProfileService.UpdateMyProfileAsync — deliberately no
        // RevertApprovalIfNeeded here (this method's own doc comment explains why).
        employer!.BusinessActivity = dto.BusinessActivity?.Trim();
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
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, validComplaintCount, application));
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

        var (employer, application, _) = await LoadReviewableEmployerWithApplicationAsync(officeId, employerId, ct);
        if (employer is not null)
        {
            await notificationService.CreateAsync(
                officeId, employer.UserId, "EmployerDocument.Approved",
                $"تمت الموافقة على مستند \"{DocumentTypeLabel(document.DocumentType)}\" في ملف شركتك.",
                nameof(EmployerDocument), document.Id, ct);
        }

        // No separate manual "approve the whole profile" click — this office's application
        // auto-advances the instant the last required document type is individually approved
        // (WORKFLOWS_AND_STATE_MACHINES.md §3's "every document Approved" gate still applies, just
        // checked here instead of behind a redundant extra button). Documents are shared, so every
        // other office still independently reviewing this employer is unaffected.
        if (employer is not null && application is not null && application.VerificationStatus == EmployerVerificationStatus.UnderReview)
        {
            var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
            var approvedTypes = documents
                .Where(d => d.ReviewStatus == DocumentReviewStatus.Approved)
                .Select(d => d.DocumentType)
                .ToHashSet();
            var allApproved = Enum.GetValues<EmployerDocumentType>().All(approvedTypes.Contains);
            if (allApproved)
            {
                application.VerificationStatus = EmployerVerificationStatus.Approved;
                application.ApprovedBy = reviewerId;
                application.ApprovedAt = DateTime.UtcNow;
                application.RejectionReason = null;
                uow.Repository<EmployerOfficeApplication>().Update(application);

                await notificationService.CreateAsync(
                    officeId, employer.UserId, "EmployerProfile.Approved",
                    "تمت الموافقة على ملف شركتك بالكامل في هذا المكتب.",
                    nameof(Employer), employer.Id, ct);
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

        var (employer, _, _) = await LoadReviewableEmployerWithApplicationAsync(officeId, employerId, ct);
        if (employer is not null)
        {
            await notificationService.CreateAsync(
                officeId, employer.UserId, "EmployerDocument.Rejected",
                $"تم رفض مستند \"{DocumentTypeLabel(document.DocumentType)}\" — السبب: {document.RejectionReason}",
                nameof(EmployerDocument), document.Id, ct);
        }

        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerDocumentDto>.Success(document.ToDto());
    }

    public async Task<ServiceResult<EmployerProfileDto>> ApproveProfileAsync(
        Guid officeId, Guid reviewerId, Guid employerId, CancellationToken ct = default)
    {
        var (employer, application, error) = await LoadReviewableEmployerWithApplicationAsync(officeId, employerId, ct);
        if (error is not null) return ServiceResult<EmployerProfileDto>.NotFound(error);
        if (application!.VerificationStatus != EmployerVerificationStatus.UnderReview)
            return ServiceResult<EmployerProfileDto>.Conflict("ملف الشركة ليس قيد المراجعة.");

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer!.Id && d.IsActive, ct: ct);

        // WORKFLOWS_AND_STATE_MACHINES.md §3: can only reach Approved once every active document
        // is individually Approved.
        var approvedTypes = documents.Where(d => d.ReviewStatus == DocumentReviewStatus.Approved).Select(d => d.DocumentType).ToHashSet();
        var notReady = Enum.GetValues<EmployerDocumentType>().Where(t => !approvedTypes.Contains(t)).ToList();
        if (notReady.Count > 0)
            return ServiceResult<EmployerProfileDto>.Invalid(
                $"يجب الموافقة على كل المستندات المطلوبة أولًا — لم تتم الموافقة على: {string.Join(", ", notReady)}.");

        application.VerificationStatus = EmployerVerificationStatus.Approved;
        application.ApprovedBy = reviewerId;
        application.ApprovedAt = DateTime.UtcNow;
        application.RejectionReason = null;
        uow.Repository<EmployerOfficeApplication>().Update(application);

        await notificationService.CreateAsync(
            officeId, employer!.UserId, "EmployerProfile.Approved",
            "تمت الموافقة على ملف شركتك بالكامل في هذا المكتب.",
            nameof(Employer), employer.Id, ct);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, await CountValidComplaintsAsync(employer.Id, ct), application));
    }

    public async Task<ServiceResult<EmployerProfileDto>> RejectProfileAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default)
    {
        var (employer, application, error) = await LoadReviewableEmployerWithApplicationAsync(officeId, employerId, ct);
        if (error is not null) return ServiceResult<EmployerProfileDto>.NotFound(error);
        if (application!.VerificationStatus != EmployerVerificationStatus.UnderReview)
            return ServiceResult<EmployerProfileDto>.Conflict("ملف الشركة ليس قيد المراجعة.");

        application.VerificationStatus = EmployerVerificationStatus.Rejected;
        application.RejectionReason = reason.Trim();
        uow.Repository<EmployerOfficeApplication>().Update(application);

        await notificationService.CreateAsync(
            officeId, employer!.UserId, "EmployerProfile.Rejected",
            $"تم رفض ملف شركتك — السبب: {application.RejectionReason}",
            nameof(Employer), employer.Id, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, await CountValidComplaintsAsync(employer.Id, ct), application));
    }

    public async Task<ServiceResult<EmployerProfileDto>> StopCooperationAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default)
    {
        var (employer, application, error) = await LoadReviewableEmployerWithApplicationAsync(officeId, employerId, ct);
        if (error is not null) return ServiceResult<EmployerProfileDto>.NotFound(error);

        // WORKFLOWS_AND_STATE_MACHINES.md §4: Active -> Stopped only.
        if (application!.CooperationStatus != EmployerCooperationStatus.Active)
            return ServiceResult<EmployerProfileDto>.Conflict("التعاون متوقف بالفعل.");

        application.CooperationStatus = EmployerCooperationStatus.Stopped;
        application.CooperationStoppedAt = DateTime.UtcNow;
        application.CooperationStoppedReason = reason.Trim();
        uow.Repository<EmployerOfficeApplication>().Update(application);

        await uow.Repository<OfficeDecision>().AddAsync(new OfficeDecision
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            TargetType = OfficeDecisionTargetType.Employer,
            TargetId = employer!.Id,
            DecisionType = OfficeDecisionType.StopCooperation,
            Reason = reason.Trim(),
            DecidedBy = reviewerId,
        }, ct);

        await notificationService.CreateAsync(
            officeId, employer.UserId, "EmployerCooperation.Stopped",
            $"تم إيقاف التعاون بينك وبين المكتب — السبب: {application.CooperationStoppedReason}",
            nameof(Employer), employer.Id, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, await CountValidComplaintsAsync(employer.Id, ct), application));
    }

    public async Task<ServiceResult<EmployerProfileDto>> ResumeCooperationAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default)
    {
        var (employer, application, error) = await LoadReviewableEmployerWithApplicationAsync(officeId, employerId, ct);
        if (error is not null) return ServiceResult<EmployerProfileDto>.NotFound(error);

        // WORKFLOWS_AND_STATE_MACHINES.md §4: Stopped -> Active only.
        if (application!.CooperationStatus != EmployerCooperationStatus.Stopped)
            return ServiceResult<EmployerProfileDto>.Conflict("التعاون غير متوقف حاليًا.");

        application.CooperationStatus = EmployerCooperationStatus.Active;
        application.CooperationStoppedAt = null;
        application.CooperationStoppedReason = null;
        uow.Repository<EmployerOfficeApplication>().Update(application);

        await uow.Repository<OfficeDecision>().AddAsync(new OfficeDecision
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            TargetType = OfficeDecisionTargetType.Employer,
            TargetId = employer!.Id,
            DecisionType = OfficeDecisionType.ResumeCooperation,
            Reason = reason.Trim(),
            DecidedBy = reviewerId,
        }, ct);

        await notificationService.CreateAsync(
            officeId, employer.UserId, "EmployerCooperation.Resumed",
            "تم استئناف التعاون بينك وبين المكتب.", nameof(Employer), employer.Id, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<EmployerDocument>().ListAsync(d => d.EmployerId == employer.Id && d.IsActive, ct: ct);
        return ServiceResult<EmployerProfileDto>.Success(employer.ToDto(documents, await CountValidComplaintsAsync(employer.Id, ct), application));
    }

    /// <summary>The tenant-isolation boundary for this whole file (TenantIsolationTests.cs): an
    /// office only sees an employer it has its own EmployerOfficeApplication row for — the
    /// employer itself no longer carries a single OfficeId.</summary>
    private async Task<(Employer? Employer, EmployerOfficeApplication? Application, string? Error)> LoadReviewableEmployerWithApplicationAsync(
        Guid officeId, Guid employerId, CancellationToken ct)
    {
        var application = await uow.Repository<EmployerOfficeApplication>().FindAsync(a => a.EmployerId == employerId && a.OfficeId == officeId, ct: ct);
        if (application is null) return (null, null, "صاحب العمل غير موجود.");

        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == employerId, ct: ct);
        return employer is null ? (null, null, "صاحب العمل غير موجود.") : (employer, application, null);
    }

    private async Task<(EmployerDocument? Document, string? Error)> LoadReviewableDocumentAsync(
        Guid officeId, Guid employerId, Guid documentId, CancellationToken ct)
    {
        var hasAccess = await uow.Repository<EmployerOfficeApplication>().AnyAsync(a => a.EmployerId == employerId && a.OfficeId == officeId, ct);
        if (!hasAccess) return (null, "صاحب العمل غير موجود.");

        var document = await uow.Repository<EmployerDocument>().FindAsync(
            d => d.Id == documentId && d.EmployerId == employerId && d.IsActive, ct: ct);
        return document is null ? (null, "الملف غير موجود.") : (document, null);
    }
}
