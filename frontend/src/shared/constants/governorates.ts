/** Egypt's 27 governorates — the one shared list for any "city/governorate" picker (the home
 * page hero's autocomplete, the public job-search filter, …). The backend filters city as a
 * free-text Contains (no City lookup table yet — PROJECT_ARCHITECTURE_FRONTEND.md / DATABASE
 * DOCUMENTATION's documented fallback), so this is purely a UI convenience: picking one of these
 * sends its exact label as the filter value. */
export const EGYPT_GOVERNORATES = [
  'القاهرة',
  'الجيزة',
  'الإسكندرية',
  'الدقهلية',
  'الشرقية',
  'الغربية',
  'المنوفية',
  'القليوبية',
  'البحيرة',
  'كفر الشيخ',
  'دمياط',
  'بورسعيد',
  'الإسماعيلية',
  'السويس',
  'شمال سيناء',
  'جنوب سيناء',
  'الفيوم',
  'بني سويف',
  'المنيا',
  'أسيوط',
  'سوهاج',
  'قنا',
  'الأقصر',
  'أسوان',
  'البحر الأحمر',
  'الوادي الجديد',
  'مطروح',
] as const
