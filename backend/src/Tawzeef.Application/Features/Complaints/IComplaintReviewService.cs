using Tawzeef.Application.Common;
using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Complaints;

public interface IComplaintReviewService
{
    Task<ServiceResult<PagedResult<ComplaintDto>>> ListQueueAsync(
        Guid officeId, ComplaintQueryDto query, CancellationToken ct = default);

    Task<ServiceResult<ComplaintDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default);

    Task<ServiceResult<ComplaintDto>> TriageAsync(
        Guid officeId, Guid reviewerId, Guid id, ComplaintValidityDecision validityDecision, CancellationToken ct = default);

    Task<ServiceResult<ComplaintDto>> ResolveAsync(
        Guid officeId, Guid reviewerId, Guid id, ComplaintStatus outcome, string resolutionNotes, CancellationToken ct = default);

    /// <summary>OfficeAdmin only (RBAC_PERMISSIONS.md §0 item 8). Returns the updated
    /// JobSeekerProfileDto — decide changes the *subject*, not the complaint
    /// (API_DOCUMENTATION.md §3.10: the complaint's own Status is untouched by it).</summary>
    Task<ServiceResult<JobSeekerProfileDto>> DecideAsync(
        Guid officeId, Guid reviewerId, Guid id, OfficeDecisionType decisionType, string reason, CancellationToken ct = default);
}
