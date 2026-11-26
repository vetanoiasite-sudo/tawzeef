using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

public class JobSeekerReviewService(IUnitOfWork uow, IFileStorage fileStorage, INotificationService notificationService) : IJobSeekerReviewService
{
    private static string DocumentTypeLabel(JobSeekerDocumentType t) => t switch
    {
        JobSeekerDocumentType.RecommendationLetter => "جواب التزكية",
        JobSeekerDocumentType.NationalId => "صورة البطاقة الشخصية",
        JobSeekerDocumentType.Cv => "السيرة الذاتية",
        _ => t.ToString(),
    };

    /// <summary>jobseekers.viewdocuments — a reviewer streams the actual file, not just its
    /// review-status metadata. Reuses the same office+profile ownership check every other
    /// document action on this profile already goes through.</summary>
    public async Task<ServiceResult<JobSeekerDocumentFileDto>> GetDocumentFileAsync(
        Guid officeId, Guid profileId, Guid documentId, CancellationToken ct = default)
    {
        var (document, error) = await LoadReviewableDocumentAsync(officeId, profileId, documentId, ct);
        if (error is not null) return ServiceResult<JobSeekerDocumentFileDto>.NotFound(error);

        var stream = await fileStorage.OpenReadAsync(document!.SecureFileReference, ct);
        return ServiceResult<JobSeekerDocumentFileDto>.Success(new JobSeekerDocumentFileDto
        {
            Content = stream,
            FileName = document.OriginalFileName,
            ContentType = document.MimeType,
        });
    }

    public async Task<ServiceResult<JobSeekerDocumentFileDto>> GetPhotoAsync(
        Guid officeId, Guid profileId, CancellationToken ct = default)
    {
        var (profile, error) = await LoadReviewableProfileAsync(officeId, profileId, ct);
        if (error is not null) return ServiceResult<JobSeekerDocumentFileDto>.NotFound(error);
        if (profile!.PhotoStorageKey is null) return ServiceResult<JobSeekerDocumentFileDto>.NotFound("لا توجد صورة محفوظة.");

        var stream = await fileStorage.OpenReadAsync(profile.PhotoStorageKey, ct);
        return ServiceResult<JobSeekerDocumentFileDto>.Success(new JobSeekerDocumentFileDto
        {
            Content = stream,
            FileName = profile.PhotoOriginalFileName ?? "photo",
            ContentType = profile.PhotoContentType ?? "application/octet-stream",
        });
    }

    public async Task<ServiceResult<PagedResult<JobSeekerReviewListItemDto>>> ListQueueAsync(
        Guid officeId, JobSeekerReviewQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        // No cross-entity join in IRepository (DATABASE_DOCUMENTATION.md's repository pattern is
        // plain predicates, no Include/Join) — composed in memory instead, same trade-off
        // EmployerReviewService.ListQueueAsync already makes for its complaint counts. Office
        // review queues are bounded per office, not expected to need DB-level paging.
        var applications = await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => a.OfficeId == officeId && (!query.ReviewStatus.HasValue || a.ReviewStatus == query.ReviewStatus.Value), ct: ct);
        if (applications.Count == 0)
            return ServiceResult<PagedResult<JobSeekerReviewListItemDto>>.Success(new PagedResult<JobSeekerReviewListItemDto>
            { Items = [], PageIndex = pageIndex, PageSize = pageSize, TotalCount = 0 });

        var applicationsByProfile = applications.ToDictionary(a => a.JobSeekerId);
        var profileIds = applicationsByProfile.Keys.ToList();

        var profiles = await uow.Repository<JobSeekerProfile>().ListAsync(
            p => profileIds.Contains(p.Id)
                && (string.IsNullOrWhiteSpace(query.Name) || p.FullName.Contains(query.Name))
                && (string.IsNullOrWhiteSpace(query.City) || (p.City != null && p.City.Contains(query.City))),
            ct: ct);

        var ordered = profiles
            .OrderByDescending(p => applicationsByProfile[p.Id].ModifiedDate ?? applicationsByProfile[p.Id].CreationDate)
            .ToList();

        var validComplaints = ordered.Count == 0
            ? []
            : await uow.Repository<Complaint>().ListAsync(
                c => c.SubjectType == ComplaintSubjectType.JobSeeker
                    && profileIds.Contains(c.SubjectId)
                    && c.ValidityDecision == ComplaintValidityDecision.Valid,
                ct: ct);
        var countsByProfile = validComplaints.GroupBy(c => c.SubjectId).ToDictionary(g => g.Key, g => g.Count());

        var page = ordered.Skip(pageIndex * pageSize).Take(pageSize).ToList();

        return ServiceResult<PagedResult<JobSeekerReviewListItemDto>>.Success(new PagedResult<JobSeekerReviewListItemDto>
        {
            Items = page.Select(p => p.ToListItemDto(applicationsByProfile[p.Id], countsByProfile.GetValueOrDefault(p.Id, 0))).ToList(),
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = ordered.Count,
        });
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var (profile, application, error) = await LoadReviewableProfileWithApplicationAsync(officeId, id, ct);
        if (error is not null) return ServiceResult<JobSeekerProfileDto>.NotFound(error);

        // WORKFLOWS_AND_STATE_MACHINES.md §1: "Submitted -> UnderReview ... may be implicit the
        // moment a reviewer opens the record" — this is that moment.
        if (application!.ReviewStatus == JobSeekerReviewStatus.Submitted)
        {
            application.ReviewStatus = JobSeekerReviewStatus.UnderReview;
            uow.Repository<JobSeekerOfficeApplication>().Update(application);
            await uow.SaveChangesAsync(ct);
        }

        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == officeId, ct: ct);
        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile!.Id && d.IsActive, ct: ct);
        var validComplaintCount = await CountValidComplaintsAsync(profile!.Id, ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile!.ToDto(documents, validComplaintCount, application, office));
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

        // Re-review is allowed from any prior status (Pending, Approved, or Rejected) — an office
        // reviewer must be able to correct a mistaken decision, not just a Pending one.
        document!.ReviewStatus = DocumentReviewStatus.Approved;
        document.ReviewedBy = reviewerId;
        document.ReviewedAt = DateTime.UtcNow;
        document.RejectionReason = null;
        uow.Repository<JobSeekerDocument>().Update(document);

        var (profile, application, _) = await LoadReviewableProfileWithApplicationAsync(officeId, profileId, ct);
        if (profile is not null)
        {
            await notificationService.CreateAsync(
                officeId, profile.UserId, "JobSeekerDocument.Approved",
                $"تمت الموافقة على مستند \"{DocumentTypeLabel(document.DocumentType)}\" في ملفك.",
                nameof(JobSeekerDocument), document.Id, ct);
        }

        // No separate manual "approve the whole profile" click — this office's application
        // auto-advances the instant the last required document type is individually approved
        // (WORKFLOWS_AND_STATE_MACHINES.md §1's "every document Approved" gate still applies, just
        // checked here instead of behind a redundant extra button). Documents are shared, so every
        // other office still independently reviewing this job seeker is unaffected.
        if (profile is not null && application is not null && application.ReviewStatus == JobSeekerReviewStatus.UnderReview)
        {
            var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
            var approvedTypes = documents
                .Where(d => d.ReviewStatus == DocumentReviewStatus.Approved)
                .Select(d => d.DocumentType)
                .ToHashSet();
            var allApproved = Enum.GetValues<JobSeekerDocumentType>().All(approvedTypes.Contains);
            if (allApproved)
            {
                application.ReviewStatus = JobSeekerReviewStatus.Approved;
                application.ApprovedBy = reviewerId;
                application.ApprovedAt = DateTime.UtcNow;
                application.RejectionReason = null;
                application.NextEmploymentCheckDueAt = application.ApprovedAt.Value.AddMonths(1);
                uow.Repository<JobSeekerOfficeApplication>().Update(application);

                await notificationService.CreateAsync(
                    officeId, profile.UserId, "JobSeekerProfile.Approved",
                    "تمت الموافقة على ملفك الشخصي بالكامل — يمكنك الآن التقديم على وظائف هذا المكتب.",
                    nameof(JobSeekerProfile), profile.Id, ct);
            }
        }

        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerDocumentDto>.Success(document.ToDto());
    }

    public async Task<ServiceResult<JobSeekerDocumentDto>> RejectDocumentAsync(
        Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, string reason, CancellationToken ct = default)
    {
        var (document, error) = await LoadReviewableDocumentAsync(officeId, profileId, documentId, ct);
        if (error is not null) return ServiceResult<JobSeekerDocumentDto>.NotFound(error);

        // Re-review allowed from any prior status — see ApproveDocumentAsync's comment above.
        document!.ReviewStatus = DocumentReviewStatus.Rejected;
        document.ReviewedBy = reviewerId;
        document.ReviewedAt = DateTime.UtcNow;
        document.RejectionReason = reason.Trim();
        uow.Repository<JobSeekerDocument>().Update(document);

        var (profile, _, _) = await LoadReviewableProfileWithApplicationAsync(officeId, profileId, ct);
        if (profile is not null)
        {
            await notificationService.CreateAsync(
                officeId, profile.UserId, "JobSeekerDocument.Rejected",
                $"تم رفض مستند \"{DocumentTypeLabel(document.DocumentType)}\" — السبب: {document.RejectionReason}",
                nameof(JobSeekerDocument), document.Id, ct);
        }

        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerDocumentDto>.Success(document.ToDto());
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> ApproveProfileAsync(
        Guid officeId, Guid reviewerId, Guid profileId, CancellationToken ct = default)
    {
        var (profile, application, error) = await LoadReviewableProfileWithApplicationAsync(officeId, profileId, ct);
        if (error is not null) return ServiceResult<JobSeekerProfileDto>.NotFound(error);
        if (application!.ReviewStatus != JobSeekerReviewStatus.UnderReview)
            return ServiceResult<JobSeekerProfileDto>.Conflict("الملف ليس قيد المراجعة.");

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile!.Id && d.IsActive, ct: ct);

        // WORKFLOWS_AND_STATE_MACHINES.md §1: can only reach Approved once every active document
        // is individually Approved.
        var approvedTypes = documents.Where(d => d.ReviewStatus == DocumentReviewStatus.Approved).Select(d => d.DocumentType).ToHashSet();
        var notReady = Enum.GetValues<JobSeekerDocumentType>().Where(t => !approvedTypes.Contains(t)).ToList();
        if (notReady.Count > 0)
            return ServiceResult<JobSeekerProfileDto>.Invalid(
                $"يجب الموافقة على كل المستندات المطلوبة أولًا — لم تتم الموافقة على: {string.Join(", ", notReady)}.");

        application.ReviewStatus = JobSeekerReviewStatus.Approved;
        application.ApprovedBy = reviewerId;
        application.ApprovedAt = DateTime.UtcNow;
        application.RejectionReason = null;
        application.NextEmploymentCheckDueAt = application.ApprovedAt.Value.AddMonths(1);
        uow.Repository<JobSeekerOfficeApplication>().Update(application);

        await notificationService.CreateAsync(
            officeId, profile!.UserId, "JobSeekerProfile.Approved",
            "تمت الموافقة على ملفك الشخصي بالكامل — يمكنك الآن التقديم على وظائف هذا المكتب.",
            nameof(JobSeekerProfile), profile.Id, ct);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, await CountValidComplaintsAsync(profile.Id, ct), application));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> RejectProfileAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default)
    {
        var (profile, application, error) = await LoadReviewableProfileWithApplicationAsync(officeId, profileId, ct);
        if (error is not null) return ServiceResult<JobSeekerProfileDto>.NotFound(error);
        if (application!.ReviewStatus != JobSeekerReviewStatus.UnderReview)
            return ServiceResult<JobSeekerProfileDto>.Conflict("الملف ليس قيد المراجعة.");

        application.ReviewStatus = JobSeekerReviewStatus.Rejected;
        application.RejectionReason = reason.Trim();
        uow.Repository<JobSeekerOfficeApplication>().Update(application);

        await notificationService.CreateAsync(
            officeId, profile!.UserId, "JobSeekerProfile.Rejected",
            $"تم رفض ملفك الشخصي — السبب: {application.RejectionReason}",
            nameof(JobSeekerProfile), profile.Id, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, await CountValidComplaintsAsync(profile.Id, ct), application));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> SuspendAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default)
    {
        var (profile, application, error) = await LoadReviewableProfileWithApplicationAsync(officeId, profileId, ct);
        if (error is not null) return ServiceResult<JobSeekerProfileDto>.NotFound(error);

        // WORKFLOWS_AND_STATE_MACHINES.md §1: Approved -> Suspended only.
        if (application!.ReviewStatus != JobSeekerReviewStatus.Approved)
            return ServiceResult<JobSeekerProfileDto>.Conflict("لا يمكن تجميد إلا ملف معتمد.");

        application.ReviewStatus = JobSeekerReviewStatus.Suspended;
        application.SuspendedAt = DateTime.UtcNow;
        application.SuspensionReason = reason.Trim();
        uow.Repository<JobSeekerOfficeApplication>().Update(application);

        await uow.Repository<OfficeDecision>().AddAsync(new OfficeDecision
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            TargetType = OfficeDecisionTargetType.JobSeeker,
            TargetId = profile!.Id,
            DecisionType = OfficeDecisionType.Suspend,
            Reason = reason.Trim(),
            DecidedBy = reviewerId,
        }, ct);

        await notificationService.CreateAsync(
            officeId, profile.UserId, "JobSeekerProfile.Suspended",
            $"تم تجميد ملفك في هذا المكتب — السبب: {application.SuspensionReason}",
            nameof(JobSeekerProfile), profile.Id, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, await CountValidComplaintsAsync(profile.Id, ct), application));
    }

    public async Task<ServiceResult<JobSeekerProfileDto>> ReinstateAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default)
    {
        var (profile, application, error) = await LoadReviewableProfileWithApplicationAsync(officeId, profileId, ct);
        if (error is not null) return ServiceResult<JobSeekerProfileDto>.NotFound(error);

        // WORKFLOWS_AND_STATE_MACHINES.md §1: Suspended -> Approved only.
        if (application!.ReviewStatus != JobSeekerReviewStatus.Suspended)
            return ServiceResult<JobSeekerProfileDto>.Conflict("لا يمكن إعادة تفعيل إلا ملف مُجمَّد.");

        application.ReviewStatus = JobSeekerReviewStatus.Approved;
        application.SuspendedAt = null;
        application.SuspensionReason = null;
        uow.Repository<JobSeekerOfficeApplication>().Update(application);

        await uow.Repository<OfficeDecision>().AddAsync(new OfficeDecision
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            TargetType = OfficeDecisionTargetType.JobSeeker,
            TargetId = profile!.Id,
            DecisionType = OfficeDecisionType.Reinstate,
            Reason = reason.Trim(),
            DecidedBy = reviewerId,
        }, ct);

        await notificationService.CreateAsync(
            officeId, profile.UserId, "JobSeekerProfile.Reinstated",
            "تم إعادة تفعيل ملفك في هذا المكتب.", nameof(JobSeekerProfile), profile.Id, ct);

        await uow.SaveChangesAsync(ct);

        var documents = await uow.Repository<JobSeekerDocument>().ListAsync(d => d.JobSeekerId == profile.Id && d.IsActive, ct: ct);
        return ServiceResult<JobSeekerProfileDto>.Success(profile.ToDto(documents, await CountValidComplaintsAsync(profile.Id, ct), application));
    }

    public async Task<ServiceResult<PagedResult<EmployedJobSeekerListItemDto>>> ListEmployedAsync(
        Guid officeId, EmployedJobSeekersQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var applications = await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => a.OfficeId == officeId && a.EmployedAt != null, ct: ct);
        if (applications.Count == 0)
            return ServiceResult<PagedResult<EmployedJobSeekerListItemDto>>.Success(new PagedResult<EmployedJobSeekerListItemDto>
            { Items = [], PageIndex = pageIndex, PageSize = pageSize, TotalCount = 0 });

        var profileIds = applications.Select(a => a.JobSeekerId).ToList();
        var profiles = await uow.Repository<JobSeekerProfile>().ListAsync(p => profileIds.Contains(p.Id), ct: ct);
        var profilesById = profiles.ToDictionary(p => p.Id);

        var ordered = applications
            .OrderByDescending(a => a.EmployedAt)
            .Where(a => profilesById.ContainsKey(a.JobSeekerId))
            .ToList();

        var page = ordered.Skip(pageIndex * pageSize).Take(pageSize).ToList();

        return ServiceResult<PagedResult<EmployedJobSeekerListItemDto>>.Success(new PagedResult<EmployedJobSeekerListItemDto>
        {
            Items = page.Select(a =>
            {
                var profile = profilesById[a.JobSeekerId];
                return new EmployedJobSeekerListItemDto
                {
                    JobSeekerId = profile.Id,
                    FullName = profile.FullName,
                    MobileNumber = profile.MobileNumber,
                    EmployedAt = a.EmployedAt!.Value,
                    LeftJobAt = a.LeftJobAt,
                };
            }).ToList(),
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = ordered.Count,
        });
    }

    private async Task<(JobSeekerProfile? Profile, string? Error)> LoadReviewableProfileAsync(Guid officeId, Guid profileId, CancellationToken ct)
    {
        var (profile, _, error) = await LoadReviewableProfileWithApplicationAsync(officeId, profileId, ct);
        return (profile, error);
    }

    /// <summary>The tenant-isolation boundary for this whole file (TenantIsolationTests.cs): an
    /// office only sees a job seeker it has its own JobSeekerOfficeApplication row for — the
    /// profile itself no longer carries a single OfficeId.</summary>
    private async Task<(JobSeekerProfile? Profile, JobSeekerOfficeApplication? Application, string? Error)> LoadReviewableProfileWithApplicationAsync(
        Guid officeId, Guid profileId, CancellationToken ct)
    {
        var application = await uow.Repository<JobSeekerOfficeApplication>().FindAsync(a => a.JobSeekerId == profileId && a.OfficeId == officeId, ct: ct);
        if (application is null) return (null, null, "ملف الباحث عن عمل غير موجود.");

        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == profileId, ct: ct);
        return profile is null ? (null, null, "ملف الباحث عن عمل غير موجود.") : (profile, application, null);
    }

    private async Task<(JobSeekerDocument? Document, string? Error)> LoadReviewableDocumentAsync(
        Guid officeId, Guid profileId, Guid documentId, CancellationToken ct)
    {
        var hasAccess = await uow.Repository<JobSeekerOfficeApplication>().AnyAsync(a => a.JobSeekerId == profileId && a.OfficeId == officeId, ct);
        if (!hasAccess) return (null, "ملف الباحث عن عمل غير موجود.");

        var document = await uow.Repository<JobSeekerDocument>().FindAsync(
            d => d.Id == documentId && d.JobSeekerId == profileId && d.IsActive, ct: ct);
        return document is null ? (null, "الملف غير موجود.") : (document, null);
    }
}
