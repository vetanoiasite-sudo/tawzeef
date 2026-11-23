using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.JobAdvertisements;

/// <summary>Office review queue + manual payment confirmation — API_DOCUMENTATION.md §3.6's
/// /api/office/job-advertisements routes.</summary>
public interface IJobAdvertisementReviewService
{
    Task<ServiceResult<PagedResult<JobAdvertisementListItemDto>>> ListQueueAsync(Guid officeId, JobAdvertisementQueryDto query, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default);

    /// <summary>Office-side correction of an ad's own fields — e.g. a typo the employer got
    /// wrong. Unlike the employer's own JobAdvertisementService.UpdateAsync, this does NOT demote
    /// a Published ad back to UnderReview: the office fixing its own already-approved listing
    /// doesn't need to re-approve itself.</summary>
    Task<ServiceResult<JobAdvertisementDto>> UpdateAsync(
        Guid officeId, Guid id, UpdateJobAdvertisementDto dto, CancellationToken ct = default);

    /// <summary>All active categories, for the office's assignment dropdown and the public search
    /// filter — DisplayOrder then Name. Not office-scoped: categories are platform-wide.</summary>
    Task<ServiceResult<List<JobCategoryDto>>> ListCategoriesAsync(CancellationToken ct = default);

    /// <summary>Office-only — the employer never picks the final category, only suggests one as
    /// free text at creation (JobAdvertisement.Category). Exactly one of
    /// AssignJobCategoryDto.ExistingCategoryId/NewCategoryName must be set: the former assigns an
    /// existing JobCategory, the latter creates a new one (next DisplayOrder) and assigns that.
    /// Required before ApproveAsync will succeed.</summary>
    Task<ServiceResult<JobAdvertisementDto>> AssignCategoryAsync(
        Guid officeId, Guid id, AssignJobCategoryDto dto, CancellationToken ct = default);

    Task<ServiceResult<JobAdvertisementDto>> ApproveAsync(Guid officeId, Guid reviewerId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> RejectAsync(Guid officeId, Guid reviewerId, Guid id, string reason, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> PauseAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> ResumeAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<JobAdvertisementDto>> ConfirmPaymentAsync(Guid officeId, Guid reviewerId, Guid id, ConfirmAdvertisementPaymentDto dto, CancellationToken ct = default);
}
