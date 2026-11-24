namespace Tawzeef.Domain.Common;

public sealed record PlatformSettingSeed(string Key, string DefaultValue, string Description);

/// <summary>The platform settings this app actually reads at runtime — not a decorative list.
/// Each key here replaces a value that used to be hardcoded; see the call-sites noted below.</summary>
public static class PlatformSettingKeys
{
    /// <summary>Was a hardcoded <c>AddDays(15)</c> in JobAdvertisementReviewService.PublishAsync.</summary>
    public const string AdvertisementDurationDays = "advertisement.duration_days";

    /// <summary>Suggested default for the office's "confirm payment" form — the office can still
    /// type a different amount per advertisement.</summary>
    public const string AdvertisementDefaultFeeAmount = "advertisement.default_fee_amount";

    /// <summary>Was a hardcoded "EGP" fallback in JobAdvertisementReviewService.ConfirmPaymentAsync.</summary>
    public const string AdvertisementDefaultFeeCurrency = "advertisement.default_fee_currency";

    /// <summary>Was a hardcoded <c>['EGP']</c> array in two frontend components (the payment-confirm
    /// form and the platform-settings page) — comma-separated, so a currency dropdown always
    /// reflects a real, editable setting instead of a value baked into frontend code.</summary>
    public const string SupportedCurrencies = "platform.supported_currencies";

    public static readonly IReadOnlyList<PlatformSettingSeed> All =
    [
        new(AdvertisementDurationDays, "15", "عدد أيام نشر إعلان الوظيفة قبل انتهائه تلقائيًا"),
        new(AdvertisementDefaultFeeAmount, "500", "القيمة المقترحة لرسوم نشر إعلان الوظيفة (يمكن للمكتب تغييرها لكل إعلان)"),
        new(AdvertisementDefaultFeeCurrency, "EGP", "العملة الافتراضية لرسوم نشر الإعلانات"),
        new(SupportedCurrencies, "EGP", "العملات المتاحة في أي قائمة اختيار عملة بالمنصة (مفصولة بفاصلة)"),
    ];
}
