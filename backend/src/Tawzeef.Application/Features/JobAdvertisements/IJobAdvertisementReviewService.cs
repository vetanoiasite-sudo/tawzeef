using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.JobAdvertisements;

/// <summary>Office review queue + manual payment confirmation — API_DOCUMENTATION.md §3.6's
/// /api/office/job-advertisements routes.</summary>
public interface IJobAdvertisementReviewService
{
    Task<ServiceResult<PagedResult<JobAdvertisementListItemDto>>> ListQueueAsync(Guid officeId, JobAdvertisementQueryDto query, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> ApproveAsync(Guid officeId, Guid reviewerId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> RejectAsync(Guid officeId, Guid reviewerId, Guid id, string reason, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> PauseAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> ResumeAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> ConfirmPaymentAsync(Guid officeId, Guid reviewerId, Guid id, ConfirmAdvertisementPaymentDto dto, CancellationToken ct = default);
}
