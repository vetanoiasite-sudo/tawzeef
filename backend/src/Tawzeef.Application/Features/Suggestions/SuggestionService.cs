using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Suggestions;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Suggestions;

public class SuggestionService(IUnitOfWork uow, INotificationService notificationService) : ISuggestionService
{
    public async Task<ServiceResult<SuggestionDto>> CreateAsync(Guid userId, CreateSuggestionDto dto, CancellationToken ct = default)
    {
        var jobSeeker = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        var employer = jobSeeker is null ? await uow.Repository<Employer>().FindAsync(e => e.UserId == userId, ct: ct) : null;
        if (jobSeeker is null && employer is null)
            return ServiceResult<SuggestionDto>.Forbidden("Only a job seeker or an employer account may submit a suggestion.");

        var officeId = jobSeeker?.OfficeId ?? employer!.OfficeId;

        var suggestion = new Suggestion
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            SubmittedByUserId = userId,
            SenderType = jobSeeker is not null ? ComplaintSenderType.JobSeeker : ComplaintSenderType.Employer,
            Content = dto.Content.Trim(),
        };
        await uow.Repository<Suggestion>().AddAsync(suggestion, ct);

        await notificationService.CreateForOfficeAdminsAsync(
            officeId, "Suggestion.Submitted", "تم استلام اقتراح جديد من مستخدم.", nameof(Suggestion), suggestion.Id, ct);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<SuggestionDto>.Success(suggestion.ToDto());
    }

    public async Task<ServiceResult<PagedResult<SuggestionDto>>> ListMineAsync(
        Guid userId, int pageIndex, int pageSize, CancellationToken ct = default)
    {
        pageIndex = Math.Max(pageIndex, 0);
        pageSize = pageSize is > 0 and <= 100 ? pageSize : 20;

        var page = await uow.Repository<Suggestion>().PageAsync(
            pageIndex, pageSize, s => s.SubmittedByUserId == userId, orderBy: s => s.CreationDate, descending: true, ct: ct);

        return ServiceResult<PagedResult<SuggestionDto>>.Success(new PagedResult<SuggestionDto>
        {
            Items = page.Items.Select(s => s.ToDto()).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }
}
