using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Interviews;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobApplications;

namespace Tawzeef.Application.Features.Interviews;

public class ApplicantEvaluationService(IUnitOfWork uow) : IApplicantEvaluationService
{
    private static ApplicantEvaluationDto ToDto(ApplicantEvaluationReport r) => new()
    {
        Id = r.Id,
        Outcome = r.Outcome.ToString(),
        Notes = r.Notes,
        SubmittedAt = r.SubmittedAt,
    };

    public async Task<ServiceResult<ApplicantEvaluationDto>> CreateOrReplaceAsync(
        Guid userId, Guid applicationId, CreateApplicantEvaluationDto dto, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult<ApplicantEvaluationDto>.NotFound("No employer account for this user.");

        var application = await uow.Repository<JobApplication>().GetByIdAsync(applicationId, ct);
        if (application is null) return ServiceResult<ApplicantEvaluationDto>.NotFound("Application not found.");

        var ownsAd = await uow.Repository<JobAdvertisement>().AnyAsync(a => a.Id == application.JobAdvertisementId && a.EmployerId == employer.Id, ct);
        if (!ownsAd) return ServiceResult<ApplicantEvaluationDto>.NotFound("Application not found.");

        var existing = await uow.Repository<ApplicantEvaluationReport>().FindAsync(r => r.JobApplicationId == applicationId && r.IsActive, ct: ct);
        if (existing is not null)
        {
            // DATABASE_DOCUMENTATION.md §6: a re-evaluation replaces Outcome/Notes on the existing
            // row rather than creating a second one. The doc's "keep prior values in AuditLog"
            // aside isn't honored here — no AuditLog table/write path exists yet in this codebase
            // (a known gap flagged separately); only ModifiedBy/ModifiedDate are stamped.
            existing.Outcome = dto.Outcome;
            existing.Notes = dto.Notes?.Trim();
            existing.SubmittedBy = userId;
            existing.SubmittedAt = DateTime.UtcNow;
            uow.Repository<ApplicantEvaluationReport>().Update(existing);
            await uow.SaveChangesAsync(ct);
            return ServiceResult<ApplicantEvaluationDto>.Success(ToDto(existing));
        }

        var report = new ApplicantEvaluationReport
        {
            Id = Guid.NewGuid(),
            OfficeId = application.OfficeId,
            JobApplicationId = application.Id,
            EmployerId = employer.Id,
            Outcome = dto.Outcome,
            Notes = dto.Notes?.Trim(),
            SubmittedBy = userId,
            SubmittedAt = DateTime.UtcNow,
        };
        await uow.Repository<ApplicantEvaluationReport>().AddAsync(report, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<ApplicantEvaluationDto>.Success(ToDto(report));
    }

    public async Task<ServiceResult<ApplicantEvaluationDto>> GetAsync(Guid userId, Guid? officeId, Guid applicationId, CancellationToken ct = default)
    {
        var application = await uow.Repository<JobApplication>().GetByIdAsync(applicationId, ct);
        if (application is null) return ServiceResult<ApplicantEvaluationDto>.NotFound("Application not found.");

        bool allowed;
        if (officeId.HasValue)
        {
            allowed = application.OfficeId == officeId.Value;
        }
        else
        {
            var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
            allowed = employer is not null && await uow.Repository<JobAdvertisement>().AnyAsync(
                a => a.Id == application.JobAdvertisementId && a.EmployerId == employer.Id, ct);
        }
        if (!allowed) return ServiceResult<ApplicantEvaluationDto>.NotFound("Application not found.");

        var report = await uow.Repository<ApplicantEvaluationReport>().FindAsync(r => r.JobApplicationId == applicationId && r.IsActive, ct: ct);
        if (report is null) return ServiceResult<ApplicantEvaluationDto>.NotFound("No evaluation exists for this application.");

        return ServiceResult<ApplicantEvaluationDto>.Success(ToDto(report));
    }
}
