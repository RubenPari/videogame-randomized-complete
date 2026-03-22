import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authService from '@/services/auth'
import { storage } from '@/utils/storage'

/**
 * useAuthStore
 * Centralized auth state management: token, user email, login/logout.
 */
export const useAuthStore = defineStore('auth', () => {
  const token = ref(storage.getToken())
  const email = ref(storage.getEmail())

  const isAuthenticated = computed(() => !!token.value)

  const login = async (userEmail, password) => {
    const data = await authService.login(userEmail, password)
    token.value = data.token
    email.value = data.email
    storage.setToken(data.token)
    storage.setEmail(data.email)
    return data
  }

  const register = async (userEmail, password, confirmPassword) => {
    return await authService.register(userEmail, password, confirmPassword)
  }

  const logout = () => {
    token.value = null
    email.value = null
    storage.clear()
  }

  const loadFromStorage = () => {
    token.value = storage.getToken()
    email.value = storage.getEmail()
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
