using Tawzeef.Application.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

public interface IJobSeekerProfileService
{
    Task<ServiceResult<JobSeekerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateJobSeekerProfileDto dto, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentDto>> UploadDocumentAsync(
        Guid userId,
        JobSeekerDocumentType documentType,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> SubmitForReviewAsync(Guid userId, CancellationToken ct = default);
}
