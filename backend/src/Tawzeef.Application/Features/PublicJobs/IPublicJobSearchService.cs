using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.PublicJobs;

/// <summary>Anonymous, no-login public job board — API_DOCUMENTATION.md §3.7.</summary>
public interface IPublicJobSearchService
{
    Task<ServiceResult<PagedResult<JobListItemDto>>> SearchAsync(JobSearchQueryDto query, CancellationToken ct = default);
    Task<ServiceResult<JobDetailsDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<PublicPlatformStatsDto>> GetPlatformStatsAsync(CancellationToken ct = default);
    Task<ServiceResult<List<CityCountDto>>> GetCitiesAsync(CancellationToken ct = default);
}
