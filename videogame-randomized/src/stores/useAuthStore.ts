import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthResponseDto } from '@/types/api-dtos'
import { storage } from '@/utils/storage'
import authService from '@/services/auth'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(storage.getToken())
  const email = ref<string | null>(storage.getEmail())

  const isAuthenticated = computed<boolean>(() => !!token.value)

  async function login(emailValue: string, password: string): Promise<AuthResponseDto> {
    const response = await authService.login(emailValue, password)
    token.value = response.token
    email.value = response.email
    storage.setToken(response.token)
    storage.setEmail(response.email)
    return response
  }

  async function register(emailValue: string, password: string, confirmPassword: string): Promise<unknown> {
    return await authService.register(emailValue, password, confirmPassword)
  }

  function logout(): void {
    token.value = null
    email.value = null
    storage.clear()
  }

  function loadFromStorage(): void {
    token.value = storage.getToken()
    email.value = storage.getEmail()
  }

  return { token, email, isAuthenticated, login, register, logout, loadFromStorage }
})
