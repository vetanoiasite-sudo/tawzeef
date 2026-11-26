using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Employers;

/// <summary>The employer's own reply to the "are you satisfied with this hire?" notification —
/// independent of the job seeker's own reply to the same HiringFollowUpReport row (see
/// JobSeekerFollowUpService.RespondToHiringFollowUpAsync).</summary>
public interface IEmployerFollowUpService
{
    Task<ServiceResult> RespondToHiringFollowUpAsync(Guid userId, Guid reportId, RespondEmployerHiringFollowUpDto dto, CancellationToken ct = default);
}
