import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'

// Arabic-only, RTL (RBAC_PERMISSIONS.md §0 item 22) — one locale today, but the plumbing (and the
// namespace split) stays in place in case English is ever asked for, per
// docs/UI_UX_GUIDELINES.md §8.
export const defaultNS = 'common'

void i18n.use(initReactI18next).init({
  lng: 'ar',
  fallbackLng: 'ar',
  defaultNS,
  ns: ['common', 'validation', 'nav'],
  resources: {
    ar: {
      common: {
        logout: 'تسجيل الخروج',
        welcomeUser: 'مرحبًا، {{name}}',
        prevPage: 'السابق',
        nextPage: 'التالي',
        pageOf: 'صفحة {{current}} من {{total}}',
        cancel: 'إلغاء',
      },
      validation: {
        required: 'هذا الحقل مطلوب',
        phoneFormat: 'رقم الهاتف غير صالح',
        nationalIdFormat: 'الرقم القومي يجب أن يكون 14 رقمًا',
        otpCodeFormat: 'أدخل الرمز المكوّن من 6 أرقام',
        emailFormat: 'البريد الإلكتروني غير صالح',
        passwordTooShort: 'كلمة المرور يجب أن تكون 8 أحرف على الأقل',
        passwordMismatch: 'كلمتا المرور غير متطابقتين',
        tooLong: 'هذا النص طويل جدًا',
        invalidNumber: 'أدخل رقمًا صحيحًا',
      },
      nav: {},
    },
  },
  interpolation: { escapeValue: false },
})

// A feature registers its own namespace by calling this once, at module scope, from its own
// `i18n/` folder (PROJECT_ARCHITECTURE_FRONTEND.md §5) — this module stays feature-agnostic
// (§6) and never imports a feature's content itself.
export function registerNamespace(ns: string, resources: Record<string, unknown>) {
  i18n.addResourceBundle('ar', ns, resources, true, true)
}

export default i18n
