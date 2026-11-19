using Tawzeef.Application.Common;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Suggestions;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Suggestions;

public class SuggestionReviewService(IUnitOfWork uow, INotificationService notificationService) : ISuggestionReviewService
{
    public async Task<ServiceResult<PagedResult<SuggestionDto>>> ListQueueAsync(Guid officeId, SuggestionQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<Suggestion>().PageAsync(
            pageIndex, pageSize,
            s => s.OfficeId == officeId && (!query.Status.HasValue || s.Status == query.Status.Value),
            orderBy: s => s.CreationDate, descending: true, ct: ct);

        return ServiceResult<PagedResult<SuggestionDto>>.Success(new PagedResult<SuggestionDto>
        {
            Items = page.Items.Select(s => s.ToDto()).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<SuggestionDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var suggestion = await uow.Repository<Suggestion>().FindAsync(s => s.Id == id && s.OfficeId == officeId, ct: ct);
        if (suggestion is null) return ServiceResult<SuggestionDto>.NotFound("Suggestion not found.");

        // Same implicit-transition convention as Complaints: opening a New suggestion is
        // "Office opens it" (WORKFLOWS_AND_STATE_MACHINES.md §10.2).
        if (suggestion.Status == SuggestionStatus.New)
        {
            suggestion.Status = SuggestionStatus.UnderReview;
            uow.Repository<Suggestion>().Update(suggestion);
            await uow.SaveChangesAsync(ct);
        }

        return ServiceResult<SuggestionDto>.Success(suggestion.ToDto());
    }

    public async Task<ServiceResult<SuggestionDto>> RespondAsync(Guid officeId, Guid id, string officeNotes, CancellationToken ct = default)
    {
        var suggestion = await uow.Repository<Suggestion>().FindAsync(s => s.Id == id && s.OfficeId == officeId, ct: ct);
        if (suggestion is null) return ServiceResult<SuggestionDto>.NotFound("Suggestion not found.");

        if (suggestion.Status is not (SuggestionStatus.New or SuggestionStatus.UnderReview))
            return ServiceResult<SuggestionDto>.Conflict("This suggestion has already been responded to.");

        suggestion.Status = SuggestionStatus.Responded;
        suggestion.OfficeNotes = officeNotes.Trim();
        uow.Repository<Suggestion>().Update(suggestion);

        await notificationService.CreateAsync(
            officeId, suggestion.SubmittedByUserId, "Suggestion.Responded",
            "تم الرد على اقتراحك.", nameof(Suggestion), suggestion.Id, ct);

        await uow.SaveChangesAsync(ct);

        return ServiceResult<SuggestionDto>.Success(suggestion.ToDto());
    }
}
