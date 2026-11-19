using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.Interviews;

public interface IHiringFollowUpService
{
    Task<ServiceResult<HiringFollowUpDto>> CreateAsync(Guid officeId, Guid reviewerId, Guid applicationId, CreateHiringFollowUpDto dto, CancellationToken ct = default);
    Task<ServiceResult<HiringFollowUpDto>> CompleteAsync(Guid officeId, Guid reviewerId, Guid applicationId, CompleteHiringFollowUpDto dto, CancellationToken ct = default);

    /// <summary>Office (own office) or Employer (own ad, read-only) — no JobSeeker row in
    /// RBAC_PERMISSIONS.md §3's matrix for this code.</summary>
    Task<ServiceResult<HiringFollowUpDto>> GetAsync(Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct = default);

    Task<ServiceResult<PagedResult<HiringFollowUpListItemDto>>> ListDueAsync(Guid officeId, HiringFollowUpQueryDto query, CancellationToken ct = default);
}
