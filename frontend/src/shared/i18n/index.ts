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
      common: {},
      validation: {},
      nav: {},
    },
  },
  interpolation: { escapeValue: false },
})

export default i18n
