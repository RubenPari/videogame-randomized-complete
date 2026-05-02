import { createI18n } from 'vue-i18n'
import en from './locales/en.json'
import it from './locales/it.json'
import es from './locales/es.json'

const messages = { en, it, es }

const browserLang = navigator.language.split('-')[0] ?? 'en'
const storedLang = localStorage.getItem('user-locale')
const defaultLang = storedLang || (browserLang in messages ? browserLang : 'en')

const i18n = createI18n({
  legacy: false,
  locale: defaultLang,
  fallbackLocale: 'en',
  messages,
})

export default i18n
