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
            return ServiceResult<ComplaintDto>.Forbidden("يجب أن يكون المرسل حساب باحث عن عمل أو صاحب عمل لتقديم شكوى.");

        Guid officeId;
        string subjectName;
        // Routed to whichever office the subject applied to first — a subject may now be linked to
        // several (confirmed 2026-11-24), but a complaint still only goes to one.
        if (dto.SubjectType == ComplaintSubjectType.JobSeeker)
        {
            var subject = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == dto.SubjectId, ct: ct);
            if (subject is null) return ServiceResult<ComplaintDto>.Invalid("الباحث عن عمل موضوع هذه الشكوى غير موجود.");
            var subjectOfficeId = (await uow.Repository<JobSeekerOfficeApplication>().ListAsync(a => a.JobSeekerId == subject.Id, ct: ct))
                .OrderBy(a => a.CreationDate).Select(a => (Guid?)a.OfficeId).FirstOrDefault();
            if (subjectOfficeId is null) return ServiceResult<ComplaintDto>.Invalid("الباحث عن عمل موضوع هذه الشكوى لم يتقدم لأي مكتب توظيف بعد.");
            officeId = subjectOfficeId.Value;
            subjectName = subject.FullName;
        }
        else
        {
            var subject = await uow.Repository<Employer>().FindAsync(e => e.Id == dto.SubjectId, ct: ct);
            if (subject is null) return ServiceResult<ComplaintDto>.Invalid("صاحب العمل موضوع هذه الشكوى غير موجود.");
            var subjectOfficeId = (await uow.Repository<EmployerOfficeApplication>().ListAsync(a => a.EmployerId == subject.Id, ct: ct))
                .OrderBy(a => a.CreationDate).Select(a => (Guid?)a.OfficeId).FirstOrDefault();
            // Not yet applied to an office — no office to route the complaint to either.
            if (subjectOfficeId is null) return ServiceResult<ComplaintDto>.Invalid("صاحب العمل موضوع هذه الشكوى لم يتقدم لأي مكتب توظيف بعد.");
            officeId = subjectOfficeId.Value;
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
