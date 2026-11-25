using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Enums;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Application.Features.Accounts;

public class AccountsService(IUnitOfWork uow, IFileStorage fileStorage) : IAccountsService
{
    public async Task<ServiceResult<PagedResult<AccountsPaymentListItemDto>>> SearchAsync(
        Guid officeId, AccountsQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;
        var (rangeStart, rangeEnd) = ResolveMonthRange(query.Year, query.Month);

        List<Guid>? matchingEmployerIds = null;
        if (!string.IsNullOrWhiteSpace(query.EmployerName))
        {
            var name = query.EmployerName.Trim();
            var employers = await uow.Repository<Employer>().ListAsync(e => e.DisplayName.Contains(name), ct: ct);
            matchingEmployerIds = employers.Select(e => e.Id).ToList();
        }

        var page = await uow.Repository<AdvertisementPayment>().PageAsync(
            pageIndex,
            pageSize,
            p => p.OfficeId == officeId
                && p.PaymentStatus == PaymentStatus.Paid
                && p.PaidAt >= rangeStart && p.PaidAt < rangeEnd
                && (matchingEmployerIds == null || matchingEmployerIds.Contains(p.EmployerId)),
            orderBy: p => p.PaidAt ?? DateTime.MinValue,
            descending: true,
            ct: ct);

        return ServiceResult<PagedResult<AccountsPaymentListItemDto>>.Success(new PagedResult<AccountsPaymentListItemDto>
        {
            Items = await ToListItemDtosAsync(page.Items, ct),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<AccountsMonthSummaryDto>> GetMonthSummaryAsync(
        Guid officeId, int? year, int? month, CancellationToken ct = default)
    {
        var (rangeStart, rangeEnd) = ResolveMonthRange(year, month);

        var payments = await uow.Repository<AdvertisementPayment>().ListAsync(
            p => p.OfficeId == officeId && p.PaymentStatus == PaymentStatus.Paid && p.PaidAt >= rangeStart && p.PaidAt < rangeEnd,
            ct: ct);

        return ServiceResult<AccountsMonthSummaryDto>.Success(new AccountsMonthSummaryDto
        {
            Year = rangeStart.Year,
            Month = rangeStart.Month,
            TotalAmount = payments.Sum(p => p.Amount),
            Currency = payments.Select(p => p.Currency).FirstOrDefault() ?? "EGP",
            PaymentsCount = payments.Count,
        });
    }

    public async Task<ServiceResult<AccountsReceiptFileDto>> GetReceiptAsync(
        Guid officeId, Guid paymentId, CancellationToken ct = default)
    {
        var payment = await uow.Repository<AdvertisementPayment>().FindAsync(p => p.Id == paymentId && p.OfficeId == officeId, ct: ct);
        if (payment is null) return ServiceResult<AccountsReceiptFileDto>.NotFound("Payment not found.");

        var content = await fileStorage.OpenReadAsync(payment.ReceiptStorageKey, ct);
        return ServiceResult<AccountsReceiptFileDto>.Success(new AccountsReceiptFileDto
        {
            Content = content,
            FileName = payment.ReceiptOriginalFileName,
            ContentType = payment.ReceiptContentType,
        });
    }

    private async Task<List<AccountsPaymentListItemDto>> ToListItemDtosAsync(IReadOnlyList<AdvertisementPayment> payments, CancellationToken ct)
    {
        var employerIds = payments.Select(p => p.EmployerId).Distinct().ToList();
        var employers = employerIds.Count == 0
            ? []
            : await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var namesByEmployer = employers.ToDictionary(e => e.Id, e => e.DisplayName);

        var recorderIds = payments.Select(p => p.RecordedBy).Distinct().ToList();
        var recorders = recorderIds.Count == 0
            ? []
            : await uow.Repository<ApplicationUser>().ListAsync(u => recorderIds.Contains(u.Id), ct: ct);
        var namesByRecorder = recorders.ToDictionary(u => u.Id, u => u.FullName);

        return payments.Select(p => new AccountsPaymentListItemDto
        {
            Id = p.Id,
            EmployerDisplayName = namesByEmployer.GetValueOrDefault(p.EmployerId, string.Empty),
            Amount = p.Amount,
            Currency = p.Currency,
            PaidAt = p.PaidAt,
            RecordedByName = namesByRecorder.GetValueOrDefault(p.RecordedBy, string.Empty),
            ReceiptUrl = $"/api/office/accounts/payments/{p.Id}/receipt",
        }).ToList();
    }

    /// <summary>Defaults to the current UTC month when both are omitted — the "this month, last
    /// month, and so on" navigation the product asked for.</summary>
    private static (DateTime Start, DateTime End) ResolveMonthRange(int? year, int? month)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month is >= 1 and <= 12 ? month.Value : now.Month;
        var start = new DateTime(y, m, 1, 0, 0, 0, DateTimeKind.Utc);
        return (start, start.AddMonths(1));
    }
}
