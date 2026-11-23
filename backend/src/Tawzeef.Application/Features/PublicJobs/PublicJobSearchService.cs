using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.PublicJobs;

public class PublicJobSearchService(IUnitOfWork uow) : IPublicJobSearchService
{
    public async Task<ServiceResult<PagedResult<JobListItemDto>>> SearchAsync(JobSearchQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;
        var now = DateTime.UtcNow;

        // EmployerName has no FK/navigation to filter through directly (Employer is a separate
        // aggregate, same reasoning as City/Category being free text) — resolve matching employer
        // ids first, same two-step approach as everywhere else in this app that filters one
        // entity by another's field without a join.
        List<Guid>? matchingEmployerIds = null;
        if (!string.IsNullOrWhiteSpace(query.EmployerName))
        {
            var matchingEmployers = await uow.Repository<Employer>().ListAsync(
                e => e.DisplayName.Contains(query.EmployerName), ct: ct);
            matchingEmployerIds = matchingEmployers.Select(e => e.Id).ToList();
        }

        // Category is now a real lookup (JobCategory's doc comment) — a Published ad always has
        // CategoryId set (JobAdvertisementReviewService.ApproveAsync requires it before approval),
        // so resolve matching category ids first, same two-step approach as EmployerName above.
        // The query parameter itself is still called "Category" and still free text from the
        // frontend's point of view — just matched against the real category name now, not the
        // employer's own free-text suggestion.
        List<Guid>? matchingCategoryIds = null;
        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            var matchingCategories = await uow.Repository<JobCategory>().ListAsync(
                c => c.Name.Contains(query.Category), ct: ct);
            matchingCategoryIds = matchingCategories.Select(c => c.Id).ToList();
        }

        // No background job exists yet to flip Published -> Expired (WORKFLOWS_AND_STATE_MACHINES.md
        // §5), so "active public search" is a read-time filter on ExpiresAt instead.
        var page = await uow.Repository<JobAdvertisement>().PageAsync(
            pageIndex,
            pageSize,
            a => a.Status == JobAdvertisementStatus.Published
                && (a.ExpiresAt == null || a.ExpiresAt > now)
                && (string.IsNullOrWhiteSpace(query.Keyword) || a.Title.Contains(query.Keyword))
                && (string.IsNullOrWhiteSpace(query.City) || (a.City != null && a.City.Contains(query.City)))
                && (matchingCategoryIds == null || (a.CategoryId != null && matchingCategoryIds.Contains(a.CategoryId.Value)))
                && (!query.EmploymentType.HasValue || a.EmploymentType == query.EmploymentType.Value)
                && (matchingEmployerIds == null || matchingEmployerIds.Contains(a.EmployerId)),
            orderBy: a => a.PublishedAt!,
            descending: true,
            ct: ct);

        var employerIds = page.Items.Select(a => a.EmployerId).Distinct().ToList();
        var employers = employerIds.Count == 0 ? [] : await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var namesByEmployer = employers.ToDictionary(e => e.Id, e => e.DisplayName);
        var logoUrlsByEmployer = employers
            .Where(e => e.LogoStorageKey is not null)
            .ToDictionary(e => e.Id, e => $"/api/employers/{e.Id}/logo");

        var categoryIds = page.Items.Where(a => a.CategoryId.HasValue).Select(a => a.CategoryId!.Value).Distinct().ToList();
        var categoryNamesById = categoryIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await uow.Repository<JobCategory>().ListAsync(c => categoryIds.Contains(c.Id), ct: ct)).ToDictionary(c => c.Id, c => c.Name);

        return ServiceResult<PagedResult<JobListItemDto>>.Success(new PagedResult<JobListItemDto>
        {
            Items = page.Items.Select(a => new JobListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                EmployerDisplayName = namesByEmployer.GetValueOrDefault(a.EmployerId, string.Empty),
                EmployerLogoUrl = logoUrlsByEmployer.GetValueOrDefault(a.EmployerId),
                City = a.City,
                Category = a.CategoryId.HasValue ? categoryNamesById.GetValueOrDefault(a.CategoryId.Value) : null,
                EmploymentType = a.EmploymentType?.ToString(),
                ExperienceRequirement = a.ExperienceRequirement,
                PublishedAt = a.PublishedAt,
                ExpiresAt = a.ExpiresAt,
            }).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<PublicPlatformStatsDto>> GetPlatformStatsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var activeJobs = await uow.Repository<JobAdvertisement>().CountAsync(
            a => a.Status == JobAdvertisementStatus.Published && (a.ExpiresAt == null || a.ExpiresAt > now), ct);
        var approvedEmployers = await uow.Repository<Employer>().CountAsync(
            e => e.VerificationStatus == EmployerVerificationStatus.Approved, ct);
        var approvedJobSeekers = await uow.Repository<JobSeekerProfile>().CountAsync(
            p => p.ReviewStatus == JobSeekerReviewStatus.Approved, ct);

        return ServiceResult<PublicPlatformStatsDto>.Success(new PublicPlatformStatsDto
        {
            ActiveJobsCount = activeJobs,
            ApprovedEmployersCount = approvedEmployers,
            ApprovedJobSeekersCount = approvedJobSeekers,
        });
    }

    public async Task<ServiceResult<JobDetailsDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        // API_DOCUMENTATION.md §3.7: a non-Published (or expired) id must not even confirm its own
        // existence to an anonymous caller — plain 404, same as "not found".
        var ad = await uow.Repository<JobAdvertisement>().FindAsync(
            a => a.Id == id && a.Status == JobAdvertisementStatus.Published && (a.ExpiresAt == null || a.ExpiresAt > now), ct: ct);
        if (ad is null) return ServiceResult<JobDetailsDto>.NotFound("Job not found.");

        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == ad.EmployerId, ct: ct);
        var category = ad.CategoryId.HasValue
            ? await uow.Repository<JobCategory>().FindAsync(c => c.Id == ad.CategoryId.Value, ct: ct)
            : null;

        return ServiceResult<JobDetailsDto>.Success(new JobDetailsDto
        {
            Id = ad.Id,
            Title = ad.Title,
            Description = ad.Description,
            ExperienceRequirement = ad.ExperienceRequirement,
            WorkingHours = ad.WorkingHours,
            Address = ad.Address,
            City = ad.City,
            Category = category?.Name,
            EmploymentType = ad.EmploymentType?.ToString(),
            SalaryMin = ad.SalaryMin,
            SalaryMax = ad.SalaryMax,
            EmployerDisplayName = employer?.DisplayName ?? string.Empty,
            EmployerLogoUrl = employer?.LogoStorageKey is null ? null : $"/api/employers/{employer.Id}/logo",
            PublishedAt = ad.PublishedAt,
            ExpiresAt = ad.ExpiresAt,
        });
    }
}
