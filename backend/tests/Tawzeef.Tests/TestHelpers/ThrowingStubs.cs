using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Employers;
using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Domain.Common;

namespace Tawzeef.Tests.TestHelpers;

/// <summary>ComplaintReviewService's constructor needs these two collaborators, but
/// ComplaintEnforcementTests only exercises TriageAsync, which never calls either of them
/// (only DecideAsync does) — a throwing stub makes that "never called" an assertion in itself,
/// instead of silently accepting a null that would NullReferenceException somewhere unrelated.</summary>
public class ThrowingJobSeekerReviewServiceStub : IJobSeekerReviewService
{
    public Task<ServiceResult<PagedResult<JobSeekerReviewListItemDto>>> ListQueueAsync(Guid officeId, JobSeekerReviewQueryDto query, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerDocumentFileDto>> GetDocumentFileAsync(Guid officeId, Guid profileId, Guid documentId, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerDocumentFileDto>> GetPhotoAsync(Guid officeId, Guid profileId, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerDocumentDto>> ApproveDocumentAsync(Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerDocumentDto>> RejectDocumentAsync(Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, string reason, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerProfileDto>> ApproveProfileAsync(Guid officeId, Guid reviewerId, Guid profileId, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerProfileDto>> RejectProfileAsync(Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerProfileDto>> SuspendAsync(Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<JobSeekerProfileDto>> ReinstateAsync(Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default) =>
        throw new NotImplementedException();
}

public class ThrowingEmployerReviewServiceStub : IEmployerReviewService
{
    public Task<ServiceResult<PagedResult<EmployerReviewListItemDto>>> ListQueueAsync(Guid officeId, EmployerReviewQueryDto query, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<EmployerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<EmployerDocumentFileDto>> GetDocumentFileAsync(Guid officeId, Guid employerId, Guid documentId, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<EmployerDocumentDto>> ApproveDocumentAsync(Guid officeId, Guid reviewerId, Guid employerId, Guid documentId, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<EmployerDocumentDto>> RejectDocumentAsync(Guid officeId, Guid reviewerId, Guid employerId, Guid documentId, string reason, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<EmployerProfileDto>> ApproveProfileAsync(Guid officeId, Guid reviewerId, Guid employerId, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<EmployerProfileDto>> RejectProfileAsync(Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<EmployerProfileDto>> StopCooperationAsync(Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default) =>
        throw new NotImplementedException();
    public Task<ServiceResult<EmployerProfileDto>> ResumeCooperationAsync(Guid officeId, Guid reviewerId, Guid employerId, string reason, CancellationToken ct = default) =>
        throw new NotImplementedException();
}
