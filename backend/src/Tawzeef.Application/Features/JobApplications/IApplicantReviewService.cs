using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.JobApplications;

/// <summary>Employer and office applicant views for one advertisement, the employer's outcome
/// update, and the payment-gated CV stream — API_DOCUMENTATION.md §3.8.</summary>
public interface IApplicantReviewService
{
    Task<ServiceResult<PagedResult<ApplicantListItemDto>>> ListForEmployerAsync(Guid userId, Guid adId, JobApplicationQueryDto query, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<ApplicantListItemDto>>> ListForOfficeAsync(Guid officeId, Guid adId, JobApplicationQueryDto query, CancellationToken ct = default);
    Task<ServiceResult> UpdateOutcomeAsync(Guid userId, Guid applicationId, UpdateApplicationOutcomeDto dto, CancellationToken ct = default);

    /// <summary>Null stream with a non-success result means "refuse" — Forbidden before payment,
    /// NotFound otherwise. Never returns a stream without the gate having passed.</summary>
    Task<ServiceResult<CvFileDto>> GetCvAsync(Guid userId, Guid applicationId, CancellationToken ct = default);
}
