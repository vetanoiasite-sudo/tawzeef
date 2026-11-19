using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Complaints;

public class ComplaintService(IUnitOfWork uow) : IComplaintService
{
    public async Task<ServiceResult<ComplaintDto>> CreateAsync(Guid userId, CreateComplaintDto dto, CancellationToken ct = default)
    {
        var jobSeeker = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        var employer = jobSeeker is null ? await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct) : null;
        if (jobSeeker is null && employer is null)
            return ServiceResult<ComplaintDto>.Forbidden("Only a job seeker or an employer account may submit a complaint.");

        Guid officeId;
        string subjectName;
        if (dto.SubjectType == ComplaintSubjectType.JobSeeker)
        {
            var subject = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == dto.SubjectId, ct: ct);
            if (subject is null) return ServiceResult<ComplaintDto>.Invalid("The job seeker this complaint is about was not found.");
            officeId = subject.OfficeId;
            subjectName = subject.FullName;
        }
        else
        {
            var subject = await uow.Repository<Employer>().FindAsync(e => e.Id == dto.SubjectId, ct: ct);
            if (subject is null) return ServiceResult<ComplaintDto>.Invalid("The employer this complaint is about was not found.");
            officeId = subject.OfficeId;
            subjectName = subject.DisplayName;
        }

        var complaint = new Complaint
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            SubmittedByUserId = userId,
            SenderType = jobSeeker is not null ? ComplaintSenderType.JobSeeker : ComplaintSenderType.Employer,
            SubjectType = dto.SubjectType,
            SubjectId = dto.SubjectId,
            ComplaintText = dto.ComplaintText.Trim(),
        };
        await uow.Repository<Complaint>().AddAsync(complaint, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<ComplaintDto>.Success(complaint.ToDto(subjectName));
    }

    public async Task<ServiceResult<PagedResult<ComplaintDto>>> ListMineAsync(
        Guid userId, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        pageIndex = Math.Max(pageIndex, 0);
        pageSize = pageSize is > 0 and <= 100 ? pageSize : 20;

        var page = await uow.Repository<Complaint>().PageAsync(pageIndex, pageSize, c => c.SubmittedByUserId == userId, ct: ct);
        var names = await ComplaintSubjectResolver.ResolveNamesAsync(uow, page.Items, ct);

        return ServiceResult<PagedResult<ComplaintDto>>.Success(new PagedResult<ComplaintDto>
        {
            Items = page.Items.Select(c => c.ToDto(names.GetValueOrDefault(c.SubjectId, string.Empty))).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }
}
