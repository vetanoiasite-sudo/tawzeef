namespace Tawzeef.Domain.Common;

/// <summary>Egypt's 27 real governorates — seeded once into the Governorates table by
/// DbInitializer. Used only there; this is not a runtime fallback (nothing reads this list
/// directly — every "pick a governorate" UI goes through the database table instead, same
/// reasoning as JobCategory's doc comment).</summary>
public static class EgyptGovernorateSeed
{
    public static readonly IReadOnlyList<string> Names =
    [
        "القاهرة",
        "الجيزة",
        "الإسكندرية",
        "الدقهلية",
        "الشرقية",
        "الغربية",
        "المنوفية",
        "القليوبية",
        "البحيرة",
        "كفر الشيخ",
        "دمياط",
        "بورسعيد",
        "الإسماعيلية",
        "السويس",
        "شمال سيناء",
        "جنوب سيناء",
        "الفيوم",
        "بني سويف",
        "المنيا",
        "أسيوط",
        "سوهاج",
        "قنا",
        "الأقصر",
        "أسوان",
        "البحر الأحمر",
        "الوادي الجديد",
        "مطروح",
    ];
}
