namespace Tawzeef.Application.Features.Accounts;

/// <summary>Defaults to the current month server-side when Year/Month are both omitted
/// (2026-11-25 product decision: "this month, last month, and so on").</summary>
public class AccountsQueryDto
{
    public int? Year { get; set; }

    /// <summary>1-12.</summary>
    public int? Month { get; set; }

    public string? EmployerName { get; set; }

    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}

/// <summary>One row of the office's own "الحسابات" ledger — every advertisement payment this
/// office has confirmed, scoped to the office itself (never another office's, never the
/// platform-wide total). Never a verification document, same rule as every other cross-role
/// surface in this app.</summary>
public class AccountsPaymentListItemDto
{
    public Guid Id { get; set; }
    public string EmployerDisplayName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }

    /// <summary>The office staff member who confirmed this payment — AdvertisementPayment.RecordedBy
    /// resolved to a name, not a manually-typed field (2026-11-25: "the person who received the
    /// payment at the office" is simply whoever was logged in and clicked confirm).</summary>
    public string RecordedByName { get; set; } = string.Empty;

    /// <summary>A relative, authenticated API path (GET /api/office/accounts/payments/{id}/receipt).</summary>
    public string ReceiptUrl { get; set; } = string.Empty;
}

public class AccountsMonthSummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public int PaymentsCount { get; set; }
}

public class AccountsReceiptFileDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
