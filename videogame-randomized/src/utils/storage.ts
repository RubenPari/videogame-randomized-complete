const TOKEN_KEY = 'authToken'
const EMAIL_KEY = 'authEmail'

export const storage = {
  getToken: (): string | null => localStorage.getItem(TOKEN_KEY),
  setToken: (token: string): void => localStorage.setItem(TOKEN_KEY, token),
  removeToken: (): void => localStorage.removeItem(TOKEN_KEY),

  getEmail: (): string | null => localStorage.getItem(EMAIL_KEY),
  setEmail: (email: string): void => localStorage.setItem(EMAIL_KEY, email),
  removeEmail: (): void => localStorage.removeItem(EMAIL_KEY),

  clear(): void {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(EMAIL_KEY)
  },
}
