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
    // Placeholder for retrieving token from store or localStorage
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
  (error) => {
    if (error.response) {
      // Handle 401 Unauthorized
      if (error.response.status === 401) {
        // Redirect to login or refresh token logic
        console.warn('Unauthorized access. Redirecting to login...')
        // window.location.href = '/login' // Uncomment when auth routes exist
      }

      // Handle 403 Forbidden
      if (error.response.status === 403) {
        console.warn('Access forbidden.')
      }

      // Handle 500 Server Error
      if (error.response.status >= 500) {
        console.error('Server error occurred.')
      }
    } else if (error.request) {
      // Network error (no response received)
      console.error('Network error. Please check your connection.')
    } else {
      console.error('Error setting up request:', error.message)
    }
    return Promise.reject(error)
  }
)

export default httpClient
