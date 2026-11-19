using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.JobSeekers;

public interface IJobSeekerReviewService
{
    Task<ServiceResult<PagedResult<JobSeekerReviewListItemDto>>> ListQueueAsync(
        Guid officeId, JobSeekerReviewQueryDto query, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentDto>> ApproveDocumentAsync(
        Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentDto>> RejectDocumentAsync(
        Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, string reason, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> ApproveProfileAsync(
        Guid officeId, Guid reviewerId, Guid profileId, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> RejectProfileAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default);
}
