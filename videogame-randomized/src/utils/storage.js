const TOKEN_KEY = 'authToken'
const EMAIL_KEY = 'authEmail'

export const storage = {
  getToken: () => localStorage.getItem(TOKEN_KEY),
  setToken: (token) => localStorage.setItem(TOKEN_KEY, token),
  removeToken: () => localStorage.removeItem(TOKEN_KEY),

  getEmail: () => localStorage.getItem(EMAIL_KEY),
  setEmail: (email) => localStorage.setItem(EMAIL_KEY, email),
  removeEmail: () => localStorage.removeItem(EMAIL_KEY),

  clear() {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(EMAIL_KEY)
  },
}
