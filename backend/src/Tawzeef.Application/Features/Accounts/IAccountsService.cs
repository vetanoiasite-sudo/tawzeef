using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.Accounts;

/// <summary>The office's own "الحسابات" ledger of confirmed advertisement payments — office-scoped
/// only (2026-11-25 product decision: no cross-office/super-admin rollup).</summary>
public interface IAccountsService
{
    Task<ServiceResult<PagedResult<AccountsPaymentListItemDto>>> SearchAsync(
        Guid officeId, AccountsQueryDto query, CancellationToken ct = default);

    Task<ServiceResult<AccountsMonthSummaryDto>> GetMonthSummaryAsync(
        Guid officeId, int? year, int? month, CancellationToken ct = default);

    Task<ServiceResult<AccountsReceiptFileDto>> GetReceiptAsync(
        Guid officeId, Guid paymentId, CancellationToken ct = default);
}
