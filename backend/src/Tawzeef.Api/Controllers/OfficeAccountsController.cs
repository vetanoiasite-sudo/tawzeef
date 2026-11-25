using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Accounts;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>The office's own "الحسابات" ledger of confirmed advertisement payments — office-scoped
/// only (2026-11-25 product decision: no cross-office/super-admin rollup).</summary>
[Route("api/office/accounts")]
public class OfficeAccountsController(IAccountsService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("payments")]
    [RequiresPermission("advertisementpayments.view")]
    public async Task<IActionResult> ListPayments([FromQuery] AccountsQueryDto query, CancellationToken ct)
        => RespondPaged(await service.SearchAsync(RequireOfficeId(), query, ct));

    [HttpGet("summary")]
    [RequiresPermission("advertisementpayments.view")]
    public async Task<IActionResult> GetSummary([FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
        => Respond(await service.GetMonthSummaryAsync(RequireOfficeId(), year, month, ct));

    /// <summary>Not a signed, short-lived URL token — an ordinary authenticated GET, re-checking
    /// office ownership on every call, streamed through IFileStorage. Same pattern as every other
    /// private file download in this app.</summary>
    [HttpGet("payments/{id:guid}/receipt")]
    [RequiresPermission("advertisementpayments.view")]
    public async Task<IActionResult> GetReceipt(Guid id, CancellationToken ct)
    {
        var result = await service.GetReceiptAsync(RequireOfficeId(), id, ct);
        if (!result.IsSuccess) return Respond(result);

        var file = result.Data!;
        return File(file.Content, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.FileName);
    }

    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}
