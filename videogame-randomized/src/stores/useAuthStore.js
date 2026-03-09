import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authService from '@/services/auth'

/**
 * useAuthStore
 * Centralized auth state management: token, user email, login/logout.
 */
export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('authToken') || null)
  const email = ref(localStorage.getItem('authEmail') || null)

  const isAuthenticated = computed(() => !!token.value)

  /**
   * Login: stores token + email
   */
  const login = async (userEmail, password) => {
    const data = await authService.login(userEmail, password)
    token.value = data.token
    email.value = data.email
    localStorage.setItem('authToken', data.token)
    localStorage.setItem('authEmail', data.email)
    return data
  }

  /**
   * Register: sends registration request
   */
  const register = async (userEmail, password, confirmPassword) => {
    return await authService.register(userEmail, password, confirmPassword)
  }

  /**
   * Logout: clears auth state
   */
  const logout = () => {
    token.value = null
    email.value = null
    localStorage.removeItem('authToken')
    localStorage.removeItem('authEmail')
  }

  /**
   * Load from storage on app init
   */
  const loadFromStorage = () => {
    token.value = localStorage.getItem('authToken') || null
    email.value = localStorage.getItem('authEmail') || null
  }

  return {
    token,
    email,
    isAuthenticated,
    login,
    register,
    logout,
    loadFromStorage,
  }
})
