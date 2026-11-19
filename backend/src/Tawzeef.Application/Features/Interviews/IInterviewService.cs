using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Interviews;

/// <summary>API_DOCUMENTATION.md §3.9's shared /api/applications/{id}/interview* routes.</summary>
public interface IInterviewService
{
    Task<ServiceResult<InterviewDto>> ScheduleAsync(Guid userId, Guid applicationId, CreateInterviewDto dto, CancellationToken ct = default);
    Task<ServiceResult<InterviewDto>> UpdateAsync(Guid userId, Guid applicationId, UpdateInterviewDto dto, CancellationToken ct = default);

    /// <summary>Shared by Employer (own ad), Office (own office) and JobSeeker (own application,
    /// read-only) — RBAC_PERMISSIONS.md §3's "interviews.view ... read own interview only".</summary>
    Task<ServiceResult<InterviewDto>> GetAsync(Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct = default);

    Task<ServiceResult<InterviewReportDto>> CreateReportAsync(Guid userId, Guid applicationId, CreateInterviewReportDto dto, CancellationToken ct = default);
    Task<ServiceResult<InterviewReportDto>> GetReportAsync(Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct = default);
}
