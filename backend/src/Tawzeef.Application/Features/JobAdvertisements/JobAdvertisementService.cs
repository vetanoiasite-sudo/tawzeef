using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobAdvertisements;

public class JobAdvertisementService(IUnitOfWork uow) : IJobAdvertisementService
{
    public async Task<ServiceResult<PagedResult<JobAdvertisementListItemDto>>> ListMineAsync(
        Guid userId, JobAdvertisementQueryDto query, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<PagedResult<JobAdvertisementListItemDto>>.NotFound("No employer account for this user.");

        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<JobAdvertisement>().PageAsync(
            pageIndex,
            pageSize,
            a => a.EmployerId == employer.Id && (!query.Status.HasValue || a.Status == query.Status.Value),
            orderBy: a => a.CreationDate,
            descending: true,
            ct: ct);

        return ServiceResult<PagedResult<JobAdvertisementListItemDto>>.Success(new PagedResult<JobAdvertisementListItemDto>
        {
            Items = page.Items.Select(a => a.ToListItemDto()).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<JobAdvertisementDto>> GetMineByIdAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var (ad, error) = await LoadMineAsync(userId, id, ct);
        if (error is not null) return ServiceResult<JobAdvertisementDto>.NotFound(error);

        var payment = await uow.Repository<AdvertisementPayment>().FindAsync(p => p.JobAdvertisementId == ad!.Id, ct: ct);
        return ServiceResult<JobAdvertisementDto>.Success(ad!.ToDto(payment));
    }

    public async Task<ServiceResult<JobAdvertisementDto>> CreateAsync(Guid userId, CreateJobAdvertisementDto dto, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<JobAdvertisementDto>.NotFound("No employer account for this user.");

        // Can't even start a draft ad until the company profile is complete (filled in and
        // submitted) — SubmitForReviewAsync still separately requires VerificationStatus.Approved
        // before the ad itself can go to the office's review queue; this is the earlier gate.
        if (employer.VerificationStatus == EmployerVerificationStatus.Draft)
            return ServiceResult<JobAdvertisementDto>.Forbidden("أكمل ملف الشركة وقدّمه للمراجعة قبل نشر إعلانات وظائف.");

        // Guaranteed set: VerificationStatus only leaves Draft once SubmitForReviewAsync has
        // confirmed an office is chosen (EmployerProfileService.SubmitForReviewAsync).
        var ad = new JobAdvertisement { Id = Guid.NewGuid(), OfficeId = employer.OfficeId!.Value, EmployerId = employer.Id }.ApplyFrom(dto);
        await uow.Repository<JobAdvertisement>().AddAsync(ad, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto());
    }

    public async Task<ServiceResult<JobAdvertisementDto>> UpdateAsync(Guid userId, Guid id, UpdateJobAdvertisementDto dto, CancellationToken ct = default)
    {
        var (ad, error) = await LoadMineAsync(userId, id, ct);
        if (error is not null) return ServiceResult<JobAdvertisementDto>.NotFound(error);

        // API_DOCUMENTATION.md §3.6: edit is allowed on Draft, Rejected, or a live Published ad.
        if (ad!.Status is not (JobAdvertisementStatus.Draft or JobAdvertisementStatus.Rejected or JobAdvertisementStatus.Published))
            return ServiceResult<JobAdvertisementDto>.Conflict("This advertisement cannot be edited in its current state.");

        ad.ApplyFrom(dto);

        // WORKFLOWS_AND_STATE_MACHINES.md §5: editing a live ad sends it back for re-approval
        // without touching PublishedAt/ExpiresAt; editing a Rejected ad clears the rejection and
        // returns it to Draft (the diagram's "Rejected -> Draft: Employer edits" edge) so the
        // normal Draft -> SubmittedForReview submit action applies next.
        if (ad.Status == JobAdvertisementStatus.Published)
            ad.Status = JobAdvertisementStatus.UnderReview;
        else if (ad.Status == JobAdvertisementStatus.Rejected)
        {
            ad.Status = JobAdvertisementStatus.Draft;
            ad.RejectionReason = null;
        }

        uow.Repository<JobAdvertisement>().Update(ad);
        await uow.SaveChangesAsync(ct);

        var payment = await uow.Repository<AdvertisementPayment>().FindAsync(p => p.JobAdvertisementId == ad.Id, ct: ct);
        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto(payment));
    }

    public async Task<ServiceResult<JobAdvertisementDto>> SubmitForReviewAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<JobAdvertisementDto>.NotFound("No employer account for this user.");

        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.EmployerId == employer.Id, ct: ct);
        if (ad is null) return ServiceResult<JobAdvertisementDto>.NotFound("Advertisement not found.");

        if (ad.Status != JobAdvertisementStatus.Draft)
            return ServiceResult<JobAdvertisementDto>.Conflict("Only a draft advertisement can be submitted.");

        // WORKFLOWS_AND_STATE_MACHINES.md §5 transition table row 1.
        if (employer.VerificationStatus != EmployerVerificationStatus.Approved || employer.CooperationStatus != EmployerCooperationStatus.Active)
            return ServiceResult<JobAdvertisementDto>.Forbidden("Your company must be approved and cooperation must be active before posting advertisements.");

        ad.Status = JobAdvertisementStatus.SubmittedForReview;
        ad.SubmittedAt = DateTime.UtcNow;
        uow.Repository<JobAdvertisement>().Update(ad);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto());
    }

    public async Task<ServiceResult<JobAdvertisementDto>> CloseAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var (ad, error) = await LoadMineAsync(userId, id, ct);
        if (error is not null) return ServiceResult<JobAdvertisementDto>.NotFound(error);

        if (ad!.Status != JobAdvertisementStatus.Published)
            return ServiceResult<JobAdvertisementDto>.Conflict("Only a published advertisement can be closed.");

        ad.Status = JobAdvertisementStatus.Closed;
        ad.ClosedAt = DateTime.UtcNow;
        uow.Repository<JobAdvertisement>().Update(ad);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(ad.ToDto());
    }

    public async Task<ServiceResult<JobAdvertisementDto>> RenewAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var (ad, error) = await LoadMineAsync(userId, id, ct);
        if (error is not null) return ServiceResult<JobAdvertisementDto>.NotFound(error);

        // WORKFLOWS_AND_STATE_MACHINES.md §5 confirmed item 17: renewal always creates a new row,
        // pre-filled from the expired one, never an update to it — a fresh payment follows the
        // normal submit -> approve -> payment flow on this new row.
        if (ad!.Status != JobAdvertisementStatus.Expired)
            return ServiceResult<JobAdvertisementDto>.Conflict("Only an expired advertisement can be renewed.");

        var renewed = new JobAdvertisement
        {
            Id = Guid.NewGuid(),
            OfficeId = ad.OfficeId,
            EmployerId = ad.EmployerId,
            Title = ad.Title,
            Description = ad.Description,
            ExperienceRequirement = ad.ExperienceRequirement,
            WorkingHours = ad.WorkingHours,
            Address = ad.Address,
            Category = ad.Category,
            City = ad.City,
            EmploymentType = ad.EmploymentType,
            SalaryMin = ad.SalaryMin,
            SalaryMax = ad.SalaryMax,
            VacancyCount = ad.VacancyCount,
            RenewedFromAdvertisementId = ad.Id,
        };
        await uow.Repository<JobAdvertisement>().AddAsync(renewed, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobAdvertisementDto>.Success(renewed.ToDto());
    }

    private async Task<(JobAdvertisement? Ad, string? Error)> LoadMineAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return (null, "No employer account for this user.");

        var ad = await uow.Repository<JobAdvertisement>().FindAsync(a => a.Id == id && a.EmployerId == employer.Id, ct: ct);
        return ad is null ? (null, "Advertisement not found.") : (ad, null);
    }
}
