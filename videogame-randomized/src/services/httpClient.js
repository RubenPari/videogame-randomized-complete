import axios from 'axios'

// Backend API configuration from environment variables
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080/api'

/**
 * Centralized Axios instance with interceptors for authentication and error handling
 */
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
    const token = localStorage.getItem('authToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response Interceptor: Handle Global Errors (e.g., 401 Unauthorized)
httpClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    // Dynamically import store to avoid Pinia initialization issues
    const { useToastStore } = await import('@/stores/useToastStore')
    const toastStore = useToastStore()

    if (error.response) {
      // Handle 401 Unauthorized - redirect to login
      if (error.response.status === 401) {
        console.warn('Unauthorized access. Redirecting to login...')
        toastStore.showToast('Session expired. Please sign in again.', 'error')

        // Clear auth state
        localStorage.removeItem('authToken')
        localStorage.removeItem('authEmail')

        // Redirect to login (use dynamic import to avoid circular deps)
        const { default: router } = await import('@/router')
        if (router.currentRoute.value.name !== 'Login') {
          router.push('/login')
        }
      }

      // Handle 403 Forbidden
      else if (error.response.status === 403) {
        console.warn('Access forbidden.')
        toastStore.showToast('Forbidden access.', 'error')
      }

      // Handle 500 Server Error
      else if (error.response.status >= 500) {
        console.error('Server error occurred.')
        toastStore.showToast('Server error encountered. Please try again later.', 'error')
      }
    } else if (error.request) {
      // Network error (no response received)
      console.error('Network error. Please check your connection.')
      toastStore.showToast('Network error. Connection failed.', 'error')
    } else {
      console.error('Error setting up request:', error.message)
      toastStore.showToast('Client request error.', 'error')
    }
    return Promise.reject(error)
  }
)

export default httpClient
