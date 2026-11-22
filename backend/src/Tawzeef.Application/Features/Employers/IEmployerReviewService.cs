using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.Employers;

public interface IEmployerReviewService
{
    Task<ServiceResult<PagedResult<EmployerReviewListItemDto>>> ListQueueAsync(
        Guid officeId, EmployerReviewQueryDto query, CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default);

    Task<ServiceResult<EmployerDocumentFileDto>> GetDocumentFileAsync(
        Guid officeId, Guid employerId, Guid documentId, CancellationToken ct = default);

    Task<ServiceResult<EmployerDocumentDto>> ApproveDocumentAsync(
        Guid officeId, Guid reviewerId, Guid employerId, Guid documentId, CancellationToken ct = default);

    Task<ServiceResult<EmployerDocumentDto>> RejectDocumentAsync(
        Guid officeId, Guid reviewerId, Guid employerId, Guid documentId, string reason, CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> ApproveProfileAsync(
        Guid officeId, Guid reviewerId, Guid employerId, CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> RejectProfileAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default);

    /// <summary>WORKFLOWS_AND_STATE_MACHINES.md §4 — independent of VerificationStatus. Called both
    /// from the direct office/employers route and from ComplaintReviewService.DecideAsync, same
    /// "one audited code path, two routes in" pattern as JobSeekerReviewService.SuspendAsync.</summary>
    Task<ServiceResult<EmployerProfileDto>> StopCooperationAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default);

    Task<ServiceResult<EmployerProfileDto>> ResumeCooperationAsync(
        Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default);
}
