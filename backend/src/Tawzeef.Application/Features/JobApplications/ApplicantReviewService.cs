using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobApplications;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobApplications;

public class ApplicantReviewService(IUnitOfWork uow, IFileStorage fileStorage, INotificationService notificationService) : IApplicantReviewService
{
    /// <summary>WORKFLOWS_AND_STATE_MACHINES.md §7's mapping table. NoResponseAfterContact maps
    /// to the same seeker-facing bucket as a refusal — BUSINESS_REQUIREMENTS.md §24.E.37 leaves
    /// that case undefined, and showing a false "still pending" state is worse than this default.</summary>
    private static JobSeekerApplicationStatus MapToSeekerFacing(EmployerApplicationOutcome outcome) => outcome switch
    {
        EmployerApplicationOutcome.NotYetContacted => JobSeekerApplicationStatus.NotContacted,
        EmployerApplicationOutcome.ContactedInterviewScheduled => JobSeekerApplicationStatus.ContactedInterviewScheduled,
        EmployerApplicationOutcome.ContactedRefused => JobSeekerApplicationStatus.ContactedNotSuitable,
        EmployerApplicationOutcome.NoResponseAfterContact => JobSeekerApplicationStatus.ContactedNotSuitable,
        _ => JobSeekerApplicationStatus.NotContacted,
    };

    private static string SeekerStatusLabel(JobSeekerApplicationStatus status) => status switch
    {
        JobSeekerApplicationStatus.NotContacted => "لم يتم التواصل بعد",
        JobSeekerApplicationStatus.ContactedNotSuitable => "تم التواصل — غير مناسب",
        JobSeekerApplicationStatus.ContactedInterviewScheduled => "تم التواصل — مقابلة مجدولة",
        _ => status.ToString(),
    };

    public async Task<ServiceResult<PagedResult<ApplicantListItemDto>>> ListForEmployerAsync(
        Guid userId, Guid adId, JobApplicationQueryDto query, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<PagedResult<ApplicantListItemDto>>.NotFound("No employer account for this user.");

        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == adId && a.EmployerId == employer.Id, ct: ct);
        if (ad is null) return ServiceResult<PagedResult<ApplicantListItemDto>>.NotFound("Advertisement not found.");

        // RBAC_PERMISSIONS.md §5 — the payment-gating rule, precisely.
        var isPaid = await uow.Repository<AdvertisementPayment>().AnyAsync(
            p => p.JobAdvertisementId == ad.Id && p.PaymentStatus == PaymentStatus.Paid, ct);

        return await BuildPageAsync(adId, query, includeGatedFields: isPaid, ct);
    }

    public async Task<ServiceResult<PagedResult<ApplicantListItemDto>>> ListForOfficeAsync(
        Guid officeId, Guid adId, JobApplicationQueryDto query, CancellationToken ct = default)
    {
        var adExists = await uow.Repository<JobAdvertisement>().AnyAsync(a => a.Id == adId && a.OfficeId == officeId, ct);
        if (!adExists) return ServiceResult<PagedResult<ApplicantListItemDto>>.NotFound("Advertisement not found.");

        // RBAC_PERMISSIONS.md §3 footnote on jobapplications.view: never payment-gated for office staff.
        return await BuildPageAsync(adId, query, includeGatedFields: true, ct);
    }

    private async Task<ServiceResult<PagedResult<ApplicantListItemDto>>> BuildPageAsync(
        Guid adId, JobApplicationQueryDto query, bool includeGatedFields, CancellationToken ct)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<JobApplication>().PageAsync(
            pageIndex, pageSize, a => a.JobAdvertisementId == adId,
            orderBy: a => a.AppliedAt, descending: true, ct: ct);

        var jobSeekerIds = page.Items.Select(a => a.JobSeekerId).Distinct().ToList();
        var profiles = jobSeekerIds.Count == 0 ? [] : await uow.Repository<JobSeekerProfile>().ListAsync(p => jobSeekerIds.Contains(p.Id), ct: ct);
        var profilesById = profiles.ToDictionary(p => p.Id);

        return ServiceResult<PagedResult<ApplicantListItemDto>>.Success(new PagedResult<ApplicantListItemDto>
        {
            Items = page.Items.Select(a =>
            {
                var profile = profilesById.GetValueOrDefault(a.JobSeekerId);
                var item = new ApplicantListItemDto
                {
                    ApplicationId = a.Id,
                    JobSeekerName = profile?.FullName ?? string.Empty,
                    AppliedAt = a.AppliedAt,
                    EmployerFacingStatus = a.EmployerFacingStatus.ToString(),
                    WithdrawnAt = a.WithdrawnAt,
                };
                if (includeGatedFields && profile is not null)
                {
                    item.Phone = profile.MobileNumber;
                    item.Email = profile.Email;
                    item.CvDownloadUrl = $"/api/employer/applications/{a.Id}/cv";
                }
                return item;
            }).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult> UpdateOutcomeAsync(Guid userId, Guid applicationId, UpdateApplicationOutcomeDto dto, CancellationToken ct = default)
    {
        var (application, _, error) = await LoadOwnedByEmployerAsync(userId, applicationId, ct);
        if (error is not null) return ServiceResult.NotFound(error);

        var oldEmployerStatus = (byte)application!.EmployerFacingStatus;
        var oldSeekerStatus = (byte)application.JobSeekerFacingStatus;
        var newSeekerStatus = MapToSeekerFacing(dto.Outcome);

        application.EmployerFacingStatus = dto.Outcome;
        application.JobSeekerFacingStatus = newSeekerStatus;
        application.LastStatusChangedAt = DateTime.UtcNow;
        uow.Repository<JobApplication>().Update(application);

        await uow.Repository<ApplicationStatusHistory>().AddAsync(new ApplicationStatusHistory
        {
            Id = Guid.NewGuid(),
            JobApplicationId = application.Id,
            StatusDomain = ApplicationStatusDomain.EmployerFacing,
            OldStatus = oldEmployerStatus,
            NewStatus = (byte)dto.Outcome,
            ChangedBy = userId,
            ActorRole = ApplicationActorRole.Employer,
            Notes = dto.Notes?.Trim(),
        }, ct);

        if (oldSeekerStatus != (byte)newSeekerStatus)
        {
            await uow.Repository<ApplicationStatusHistory>().AddAsync(new ApplicationStatusHistory
            {
                Id = Guid.NewGuid(),
                JobApplicationId = application.Id,
                StatusDomain = ApplicationStatusDomain.JobSeekerFacing,
                OldStatus = oldSeekerStatus,
                NewStatus = (byte)newSeekerStatus,
                ChangedBy = userId,
                ActorRole = ApplicationActorRole.Employer,
            }, ct);

            var jobSeeker = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == application.JobSeekerId, ct: ct);
            var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == application.JobAdvertisementId, ct: ct);
            if (jobSeeker is not null)
            {
                await notificationService.CreateAsync(
                    application.OfficeId, jobSeeker.UserId, "JobApplication.StatusChanged",
                    $"تحديث على طلبك لوظيفة \"{ad?.Title}\": {SeekerStatusLabel(newSeekerStatus)}.",
                    nameof(JobApplication), application.Id, ct);
            }
        }

        await uow.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<CvFileDto>> GetCvAsync(Guid userId, Guid applicationId, CancellationToken ct = default)
    {
        var (application, ad, error) = await LoadOwnedByEmployerAsync(userId, applicationId, ct);
        if (error is not null) return ServiceResult<CvFileDto>.NotFound(error);

        var isPaid = await uow.Repository<AdvertisementPayment>().AnyAsync(
            p => p.JobAdvertisementId == ad!.Id && p.PaymentStatus == PaymentStatus.Paid, ct);
        if (!isPaid)
            return ServiceResult<CvFileDto>.Forbidden("Applicant data unlocks only after this advertisement's payment is confirmed.");

        var document = await uow.Repository<JobSeekerDocument>().FindAsync(
            d => d.JobSeekerId == application!.JobSeekerId && d.DocumentType == JobSeekerDocumentType.Cv && d.IsActive, ct: ct);
        if (document is null) return ServiceResult<CvFileDto>.NotFound("This applicant has no CV on file.");

        var stream = await fileStorage.OpenReadAsync(document.SecureFileReference, ct);
        return ServiceResult<CvFileDto>.Success(new CvFileDto
        {
            Content = stream,
            FileName = document.OriginalFileName,
            ContentType = document.MimeType,
        });
    }

    private async Task<(JobApplication? Application, JobAdvertisement? Ad, string? Error)> LoadOwnedByEmployerAsync(Guid userId, Guid applicationId, CancellationToken ct)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return (null, null, "No employer account for this user.");

        var application = await uow.Repository<JobApplication>().GetByIdAsync(applicationId, ct);
        if (application is null) return (null, null, "Application not found.");

        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == application.JobAdvertisementId && a.EmployerId == employer.Id, ct: ct);
        return ad is null ? (null, null, "Application not found.") : (application, ad, null);
    }
}
