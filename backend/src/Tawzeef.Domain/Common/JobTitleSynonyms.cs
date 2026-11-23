namespace Tawzeef.Domain.Common;

/// <summary>A curated job-title synonym dictionary (confirmed 2026-11-23) — a job seeker searching
/// "شيف" should also find a job ad titled "طباخ", since they're the same job under different
/// names. This is a deliberately simple, static, hand-curated approach: a small code list, not a
/// database table or a full-text/semantic search engine — the product owner explicitly chose this
/// over a heavier search stack, which would be overkill at this platform's current scale. Expand
/// this list as new job titles come up in real postings; it only ever helps a search that would
/// otherwise miss a real match, never hurts one that already worked.
/// <para>Each inner array is one group of interchangeable terms for the same real-world job —
/// formal Arabic, Egyptian colloquial, and the English-loanword spellings Egyptian job ads
/// commonly use side by side. <see cref="Expand"/> is the only thing callers need.</para></summary>
public static class JobTitleSynonyms
{
    private static readonly string[][] Groups =
    [
        // -------------------------------------------------------------- Food & hospitality
        ["شيف", "طباخ", "كوك", "شيف مطبخ", "رئيس طباخين", "مساعد شيف"],
        ["نادل", "جرسون", "ويتر", "خادم"],
        ["باريستا", "كابتن قهوة", "عامل كافيه"],
        ["مضيف مطعم", "هوست", "مضيفة استقبال مطعم"],
        ["عامل مخبز", "خباز", "فرّان"],

        // -------------------------------------------------------------- Sales & retail
        ["مندوب مبيعات", "سيلز", "ممثل مبيعات", "مندوب تسويق ميداني", "سيلز مان", "سيلز ريبريزنتاتيف"],
        ["كاشير", "أمين صندوق", "كاشير محل"],
        ["أمين مخزن", "مسؤول مخزن", "مخزنجي", "ستوك كيبر", "أمين مخازن"],
        ["بائع", "عامل محل", "موظف مبيعات"],
        ["مدير مبيعات", "سيلز مانجر", "مدير منطقة مبيعات"],
        ["عارض منتج", "مروّج", "بروموتر"],

        // -------------------------------------------------------------- Accounting & finance
        ["محاسب", "أكاونتنت", "محاسب عام"],
        ["محاسب أول", "سنيور أكاونتنت", "رئيس حسابات"],
        ["مدقق حسابات", "أودتر", "مراجع حسابات", "مراجع داخلي"],
        ["محلل مالي", "فايننشال أناليست"],
        ["مدير مالي", "فايننشال مانجر", "سي إف أو"],
        ["محصّل", "موظف تحصيل"],

        // -------------------------------------------------------------- Administrative & secretarial
        ["سكرتير", "سكرتيرة", "سكرتير تنفيذي", "أوفيس مانجر", "مدير مكتب"],
        ["موظف استقبال", "ريسبشن", "ريسبشنيست"],
        ["مدخل بيانات", "داتا إنتري", "مدخل بيانات حاسب آلي"],
        ["موظف أرشيف", "أرشفة", "أمين أرشيف"],
        ["مساعد إداري", "أدمن أسيستنت", "موظف إداري"],

        // -------------------------------------------------------------- Information technology
        ["مبرمج", "ديفيلوبر", "مطور برمجيات", "مطور"],
        ["مطور واجهات أمامية", "فرونت إند ديفيلوبر", "فرونت إند", "مطور ويب أمامي"],
        ["مطور باك إند", "باك إند ديفيلوبر", "باك إند"],
        ["مطور متكامل", "فل ستاك ديفيلوبر", "فل ستاك"],
        ["مهندس برمجيات", "سوفت وير إنجينير"],
        ["مطور تطبيقات موبايل", "موبايل ديفيلوبر"],
        ["مصمم جرافيك", "جرافيك ديزاينر", "مصمم إعلانات"],
        ["مصمم واجهات مستخدم", "يو آي يو إكس ديزاينر", "UI/UX ديزاينر", "مصمم تجربة مستخدم"],
        ["فني صيانة كمبيوتر", "تكنيشن", "دعم فني", "آي تي سبورت", "فني كمبيوتر"],
        ["مهندس شبكات", "نتورك إنجينير"],
        ["مختبر برمجيات", "كيو إيه", "QA", "تيستر"],

        // -------------------------------------------------------------- Customer service / call center
        ["خدمة عملاء", "كول سنتر", "كاستمر سيرفيس", "موظف خدمة عملاء", "موظف كول سنتر"],
        ["موظف شكاوى", "كاستمر كومبلينتس", "مسؤول شكاوى"],
        ["موظف دعم فني عبر الهاتف", "تيكنيكال سبورت", "سبورت أجنت"],

        // -------------------------------------------------------------- Logistics & transport
        ["سائق", "درايفر", "سواق"],
        ["عامل لوجستيات", "لوجستيك كوردينيتور", "مسؤول لوجستيات"],
        ["مندوب توصيل", "ديليفري", "عامل توصيل", "ديليفري بوي"],
        ["مشرف مخازن", "ستور سوبرفايزر", "مشرف مستودعات"],
        ["عامل شحن وتفريغ", "عامل مستودع", "وركهاوس وركر"],

        // -------------------------------------------------------------- Engineering & production
        ["مهندس مدني", "سيفيل إنجينير"],
        ["مهندس معماري", "أركيتكت", "مهندس معماري تنفيذي"],
        ["مهندس كهرباء", "إلكتريكال إنجينير"],
        ["مهندس ميكانيكا", "ميكانيكال إنجينير"],
        ["فني صيانة", "مين تنانس تكنيشن", "فني صيانة عامة"],
        ["مشرف إنتاج", "بروداكشن سوبرفايزر", "مشرف خط إنتاج"],
        ["عامل مصنع", "فاكتوري وركر", "عامل إنتاج"],
        ["مهندس جودة", "كواليتي إنجينير", "مسؤول جودة"],

        // -------------------------------------------------------------- Health & nursing
        ["ممرض", "ممرضة", "نيرس"],
        ["طبيب", "دكتور", "طبيب عام"],
        ["صيدلي", "فارماسيست", "صيدلاني"],
        ["فني مختبر", "لاب تكنيشن", "محلل مختبر"],
        ["أخصائي علاج طبيعي", "فيزيوثيرابيست"],
        ["أخصائي تغذية", "نيوتريشنست"],

        // -------------------------------------------------------------- HR & management
        ["موارد بشرية", "إتش آر", "HR", "أخصائي موارد بشرية"],
        ["مسؤول توظيف", "ريكروتر", "أخصائي توظيف"],
        ["مدير", "مانجر", "مدير عام"],
        ["مشرف", "سوبرفايزر", "مشرف فريق"],
        ["مدير فرع", "برانش مانجر"],

        // -------------------------------------------------------------- Marketing
        ["تسويق", "ماركتنج", "أخصائي تسويق"],
        ["أخصائي سوشيال ميديا", "سوشيال ميديا سبيشلست", "مسؤول سوشيال ميديا"],
        ["مسؤول تسويق رقمي", "ديجيتال ماركتنج", "أخصائي تسويق إلكتروني"],
        ["كاتب محتوى", "كونتنت رايتر", "محرر محتوى"],

        // -------------------------------------------------------------- Security & facilities
        ["أمن", "حارس أمن", "سكيورتي", "رجل أمن"],
        ["عامل نظافة", "كلينر", "عامل نظافة عامة"],
        ["بوّاب", "فرّاش", "عامل خدمات عامة"],
    ];

    /// <summary>Returns the given keyword plus every term from any synonym group it matches
    /// (case-insensitive substring match in either direction) — a search caller ORs the Title
    /// against each returned term. Always includes the original, trimmed keyword, even if it
    /// matches no group, so a literal match still works exactly as before.</summary>
    public static IReadOnlyList<string> Expand(string keyword)
    {
        var trimmed = keyword.Trim();
        var terms = new List<string> { trimmed };
        if (trimmed.Length == 0) return terms;

        foreach (var group in Groups)
        {
            var groupMatches = group.Any(term =>
                term.Contains(trimmed, StringComparison.OrdinalIgnoreCase) ||
                trimmed.Contains(term, StringComparison.OrdinalIgnoreCase));
            if (!groupMatches) continue;

            foreach (var term in group)
            {
                if (!terms.Contains(term, StringComparer.OrdinalIgnoreCase)) terms.Add(term);
            }
        }

        return terms;
    }
}
