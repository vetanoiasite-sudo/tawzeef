using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.JobApplications;

/// <summary>Job Seeker self-service — API_DOCUMENTATION.md §3.8's /api/job-seekers/me/applications routes.</summary>
public interface IJobApplicationService
{
    Task<ServiceResult<JobApplicationIdDto>> CreateAsync(Guid userId, CreateJobApplicationDto dto, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<MyApplicationListItemDto>>> ListMineAsync(Guid userId, JobApplicationQueryDto query, CancellationToken ct = default);
    Task<ServiceResult> WithdrawAsync(Guid userId, Guid id, CancellationToken ct = default);
}
