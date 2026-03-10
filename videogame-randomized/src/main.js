import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import './assets/main.css'
import i18n from './i18n'

const app = createApp(App)
app.use(i18n)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.mount('#app')
