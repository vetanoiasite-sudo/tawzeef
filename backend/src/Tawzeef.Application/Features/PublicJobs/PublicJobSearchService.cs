using System.Linq.Expressions;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;
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
        Expression<Func<JobAdvertisement, bool>> predicate = a =>
            a.Status == JobAdvertisementStatus.Published
            && (a.ExpiresAt == null || a.ExpiresAt > now)
            && (string.IsNullOrWhiteSpace(query.City) || (a.City != null && a.City.Contains(query.City)))
            && (matchingCategoryIds == null || (a.CategoryId != null && matchingCategoryIds.Contains(a.CategoryId.Value)))
            && (!query.EmploymentType.HasValue || a.EmploymentType == query.EmploymentType.Value)
            && (matchingEmployerIds == null || matchingEmployerIds.Contains(a.EmployerId));

        // A keyword search also matches a known synonym — "شيف" finds a job titled "طباخ"
        // (JobTitleSynonyms' doc comment: a curated list, confirmed 2026-11-23, not a search
        // engine). Each expanded term becomes its own Title.Contains(term) check, OR'd together
        // via PredicateCombinator into one flat expression EF Core can still translate to SQL.
        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var terms = JobTitleSynonyms.Expand(query.Keyword);
            Expression<Func<JobAdvertisement, bool>>? keywordPredicate = null;
            foreach (var term in terms)
            {
                Expression<Func<JobAdvertisement, bool>> termPredicate = a => a.Title.Contains(term);
                keywordPredicate = keywordPredicate is null ? termPredicate : keywordPredicate.OrElse(termPredicate);
            }
            predicate = predicate.AndAlso(keywordPredicate!);
        }

        var page = await uow.Repository<JobAdvertisement>().PageAsync(
            pageIndex,
            pageSize,
            predicate,
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

        var officeIds = page.Items.Select(a => a.OfficeId).Distinct().ToList();
        var officeNamesById = officeIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await uow.Repository<EmploymentOffice>().ListAsync(o => officeIds.Contains(o.Id), ct: ct))
                .ToDictionary(o => o.Id, o => o.ArName ?? o.Name);

        return ServiceResult<PagedResult<JobListItemDto>>.Success(new PagedResult<JobListItemDto>
        {
            Items = page.Items.Select(a => new JobListItemDto
            {
                Id = a.Id,
                Title = a.Title,
                EmployerDisplayName = namesByEmployer.GetValueOrDefault(a.EmployerId, string.Empty),
                EmployerLogoUrl = logoUrlsByEmployer.GetValueOrDefault(a.EmployerId),
                OfficeId = a.OfficeId,
                OfficeName = officeNamesById.GetValueOrDefault(a.OfficeId, string.Empty),
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
        // Approved at ANY office counts once each here — a platform-wide headline stat, not scoped
        // to one office (confirmed 2026-11-24: an employer/job seeker may be approved at several).
        var approvedEmployers = (await uow.Repository<EmployerOfficeApplication>().ListAsync(
            a => a.VerificationStatus == EmployerVerificationStatus.Approved, ct: ct))
            .Select(a => a.EmployerId).Distinct().Count();
        var approvedJobSeekers = (await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => a.ReviewStatus == JobSeekerReviewStatus.Approved, ct: ct))
            .Select(a => a.JobSeekerId).Distinct().Count();

        return ServiceResult<PublicPlatformStatsDto>.Success(new PublicPlatformStatsDto
        {
            ActiveJobsCount = activeJobs,
            ApprovedEmployersCount = approvedEmployers,
            ApprovedJobSeekersCount = approvedJobSeekers,
        });
    }

    /// <summary>Distinct cities actually carried by a live ad, with how many — drives the home
    /// page "browse by city" section instead of a static governorate list.</summary>
    public async Task<ServiceResult<List<CityCountDto>>> GetCitiesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var liveAds = await uow.Repository<JobAdvertisement>().ListAsync(
            a => a.Status == JobAdvertisementStatus.Published
                && (a.ExpiresAt == null || a.ExpiresAt > now)
                && a.City != null && a.City != string.Empty,
            ct: ct);

        var cities = liveAds
            .GroupBy(a => a.City!)
            .Select(g => new CityCountDto { City = g.Key, JobCount = g.Count() })
            .OrderByDescending(c => c.JobCount)
            .ToList();

        return ServiceResult<List<CityCountDto>>.Success(cities);
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
        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == ad.OfficeId, ct: ct);

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
            OfficeId = ad.OfficeId,
            OfficeName = office?.ArName ?? office?.Name ?? string.Empty,
            PublishedAt = ad.PublishedAt,
            ExpiresAt = ad.ExpiresAt,
        });
    }
}
