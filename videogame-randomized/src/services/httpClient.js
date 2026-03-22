import axios from 'axios'
import { storage } from '@/utils/storage'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api'

const httpClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
})

// Request Interceptor: Attach Authorization Token
httpClient.interceptors.request.use(
  (config) => {
    const token = storage.getToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  },
)

// Response Interceptor: Handle Global Errors
httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    // Dynamic imports to avoid circular dependencies with Pinia/Router
    const { useToastStore } = await import('@/stores/useToastStore')
    const { default: i18n } = await import('@/i18n')
    const toastStore = useToastStore()
    const t = i18n.global.t

    if (error.response) {
      if (error.response.status === 401) {
        console.warn('Unauthorized access. Redirecting to login...')
        toastStore.showToast(t('errors.session_expired'), 'error')

        storage.clear()

        const { default: router } = await import('@/router')
        if (router.currentRoute.value.name !== 'Login') {
          router.push('/login')
        }
      } else if (error.response.status === 403) {
        console.warn('Access forbidden.')
        toastStore.showToast(t('errors.forbidden'), 'error')
      } else if (error.response.status >= 500) {
        console.error('Server error occurred.')
        toastStore.showToast(t('errors.server_error'), 'error')
      }
    } else if (error.request) {
      console.error('Network error. Please check your connection.')
      toastStore.showToast(t('errors.network_error'), 'error')
    } else {
      console.error('Error setting up request:', error.message)
      toastStore.showToast(t('errors.client_error'), 'error')
    }
    return Promise.reject(error)
  },
)

export default httpClient
