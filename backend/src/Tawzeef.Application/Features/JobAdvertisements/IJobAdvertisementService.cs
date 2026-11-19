using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.JobAdvertisements;

/// <summary>Employer self-service — API_DOCUMENTATION.md §3.6's /api/employer/job-advertisements routes.</summary>
public interface IJobAdvertisementService
{
    Task<ServiceResult<PagedResult<JobAdvertisementListItemDto>>> ListMineAsync(Guid userId, JobAdvertisementQueryDto query, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> GetMineByIdAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> CreateAsync(Guid userId, CreateJobAdvertisementDto dto, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> UpdateAsync(Guid userId, Guid id, UpdateJobAdvertisementDto dto, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> SubmitForReviewAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> CloseAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> RenewAsync(Guid userId, Guid id, CancellationToken ct = default);
}
