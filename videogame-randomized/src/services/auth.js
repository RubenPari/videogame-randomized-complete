import httpClient from './httpClient'

/**
 * Auth service for communicating with backend auth endpoints
 */
const authService = {
  /**
   * Register a new user
   * @param {string} email
   * @param {string} password
   * @param {string} confirmPassword
   */
  async register(email, password, confirmPassword) {
    const response = await httpClient.post('/auth/register', {
      email,
      password,
      confirmPassword,
    })
    return response.data
  },

  /**
   * Login with email and password
   * @param {string} email
   * @param {string} password
   * @returns {Promise<{token: string, email: string}>}
   */
  async login(email, password) {
    const response = await httpClient.post('/auth/login', {
      email,
      password,
    })
    return response.data
  },

  /**
   * Resend email confirmation for an existing unconfirmed account
   * @param {string} email
   * @param {string} password
   * @returns {Promise<{message: string, confirmationEmailSent: boolean}>}
   */
  async resendConfirmationEmail(email, password) {
    const response = await httpClient.post('/auth/resend-confirmation', {
      email,
      password,
    })
    return response.data
  },

  /**
   * Confirm email with userId and token
   * @param {string} userId
   * @param {string} token
   */
  async confirmEmail(userId, token) {
    const response = await httpClient.get('/auth/confirm-email', {
      params: { userId, token },
    })
    return response.data
  },

  /**
   * Request a password reset email
   * @param {string} email
   */
  async forgotPassword(email) {
    const response = await httpClient.post('/auth/forgot-password', { email })
    return response.data
  },

  /**
   * Reset password with token
   * @param {string} userId
   * @param {string} token
   * @param {string} newPassword
   */
  async resetPassword(userId, token, newPassword) {
    const response = await httpClient.post('/auth/reset-password', {
      userId,
      token,
      newPassword,
    })
    return response.data
  },

  /**
   * Change password (requires auth)
   * @param {string} currentPassword
   * @param {string} newPassword
   */
  async changePassword(currentPassword, newPassword) {
    const response = await httpClient.post('/auth/change-password', {
      currentPassword,
      newPassword,
    })
    return response.data
  },
}

export default authService
