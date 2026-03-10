import { createI18n } from 'vue-i18n'
import en from './locales/en.json'
import it from './locales/it.json'
import es from './locales/es.json'

// Get preferred language from localStorage or browser settings
const getBrowserLang = () => {
  const lang = navigator.language || navigator.userLanguage
  return lang.substring(0, 2)
}

const savedLang = localStorage.getItem('user-locale')
const defaultLang = ['en', 'it', 'es'].includes(savedLang || getBrowserLang()) ? (savedLang || getBrowserLang()) : 'en'

const i18n = createI18n({
  legacy: false, // Use Composition API
  locale: defaultLang,
  fallbackLocale: 'en',
  messages: {
    en,
    it,
    es
  }
})

export default i18n
