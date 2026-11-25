using Microsoft.AspNetCore.Mvc;

namespace Tawzeef.Api.Controllers;

/// <summary>An ASP.NET Core model-binding shape (IFormFile), so it can't live in Application —
/// BACKEND_GUIDELINES.md §9: Application never touches IFormFile directly. The receipt photo is
/// required (2026-11-25 product decision) — a payment can't be confirmed without one.</summary>
public class ConfirmAdvertisementPaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public IFormFile File { get; set; } = null!;
}
