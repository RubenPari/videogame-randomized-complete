import axios, { type AxiosInstance, type InternalAxiosRequestConfig, type AxiosResponse, type AxiosError } from 'axios'
import { storage } from '@/utils/storage'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api'

const httpClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
})

httpClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = storage.getToken()
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error: unknown) => Promise.reject(error),
)

httpClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: AxiosError) => {
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
