import httpClient from './httpClient'
import type { AuthResponseDto } from '@/types/api-dtos'

const authService = {
  async register(email: string, password: string, confirmPassword: string): Promise<unknown> {
    const response = await httpClient.post('/auth/register', {
      email,
      password,
      confirmPassword,
    })
    return response.data
  },

  async login(email: string, password: string): Promise<AuthResponseDto> {
    const response = await httpClient.post('/auth/login', {
      email,
      password,
    })
    return response.data as AuthResponseDto
  },

  async resendConfirmationEmail(email: string, password: string): Promise<{ message: string; confirmationEmailSent: boolean }> {
    const response = await httpClient.post('/auth/resend-confirmation', {
      email,
      password,
    })
    return response.data as { message: string; confirmationEmailSent: boolean }
  },

  async confirmEmail(userId: string, token: string): Promise<unknown> {
    const response = await httpClient.get('/auth/confirm-email', {
      params: { userId, token },
    })
    return response.data
  },

  async forgotPassword(email: string): Promise<unknown> {
    const response = await httpClient.post('/auth/forgot-password', { email })
    return response.data
  },

  async resetPassword(userId: string, token: string, newPassword: string): Promise<unknown> {
    const response = await httpClient.post('/auth/reset-password', {
      userId,
      token,
      newPassword,
    })
    return response.data
  },

  async changePassword(currentPassword: string, newPassword: string): Promise<unknown> {
    const response = await httpClient.post('/auth/change-password', {
      currentPassword,
      newPassword,
    })
    return response.data
  },
}

export default authService
