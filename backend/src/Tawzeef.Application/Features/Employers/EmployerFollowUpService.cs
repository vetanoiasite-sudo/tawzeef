using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Interviews;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobApplications;

namespace Tawzeef.Application.Features.Employers;

public class EmployerFollowUpService(IUnitOfWork uow) : IEmployerFollowUpService
{
    public async Task<ServiceResult> RespondToHiringFollowUpAsync(
        Guid userId, Guid reportId, RespondEmployerHiringFollowUpDto dto, CancellationToken ct = default)
    {
        var employer = await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct);
        if (employer is null) return ServiceResult.NotFound("No employer account for this user.");

        var report = await uow.Repository<HiringFollowUpReport>().FindAsync(r => r.Id == reportId && r.IsActive, ct: ct);
        if (report is null || report.JobApplicationId is null) return ServiceResult.NotFound("Follow-up report not found.");

        var application = await uow.Repository<JobApplication>().FindAsync(a => a.Id == report.JobApplicationId.Value, ct: ct);
        if (application is null) return ServiceResult.NotFound("Follow-up report not found.");

        var ad = await uow.Repository<JobAdvertisement>().FindAsync(
            a => a.Id == application.JobAdvertisementId && a.EmployerId == employer.Id, ct: ct);
        if (ad is null) return ServiceResult.NotFound("Follow-up report not found.");

        report.EmployerSatisfied = dto.Satisfied;
        report.EmployerNotes = dto.Notes?.Trim();
        report.EmployerRespondedAt = DateTime.UtcNow;
        uow.Repository<HiringFollowUpReport>().Update(report);

        await uow.SaveChangesAsync(ct);
        return ServiceResult.Success();
    }
}
