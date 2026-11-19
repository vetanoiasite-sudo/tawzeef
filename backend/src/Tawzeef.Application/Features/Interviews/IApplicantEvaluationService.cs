using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Interviews;

/// <summary>DATABASE_DOCUMENTATION.md §6/§9: one per JobApplication — a re-submit replaces
/// Outcome/Notes on the existing row instead of creating a second one.</summary>
public interface IApplicantEvaluationService
{
    Task<ServiceResult<ApplicantEvaluationDto>> CreateOrReplaceAsync(Guid userId, Guid applicationId, CreateApplicantEvaluationDto dto, CancellationToken ct = default);

    /// <summary>Office (own office) or Employer (own ad) — no JobSeeker row for this code.</summary>
    Task<ServiceResult<ApplicantEvaluationDto>> GetAsync(Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct = default);
}
