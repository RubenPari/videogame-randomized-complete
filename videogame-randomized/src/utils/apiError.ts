export function getApiErrorMessage(err: unknown, fallback: string): string {
  const ax = err && typeof err === 'object' && 'response' in err ? (err.response as { data?: unknown } | null) : null
  const data = ax && typeof ax === 'object' && 'data' in ax ? ax.data : null
  if (!data || typeof data !== 'object') {
    return err && typeof err === 'object' && 'message' in err && typeof err.message === 'string'
      ? err.message
      : fallback
  }
  const obj = data as Record<string, unknown>
  if (typeof obj.detail === 'string' && obj.detail.length > 0) return obj.detail
  if (typeof obj.title === 'string' && obj.title.length > 0 && typeof obj.detail !== 'string') {
    return obj.title
  }
  if (typeof obj.error === 'string' && obj.error.length > 0) return obj.error
  if (Array.isArray(obj.errors)) return obj.errors.filter(Boolean).join(', ')
  if (obj.errors && typeof obj.errors === 'object') {
    const flat = Object.values(obj.errors).flat()
    const first = flat.find((x) => typeof x === 'string')
    if (first) return first
  }
  return fallback
}

const LOGIN_EMAIL_NOT_CONFIRMED_TYPE = 'urn:videogame-randomizer:login:email-not-confirmed'

export function isEmailNotConfirmedLoginError(err: unknown): boolean {
  const ax = err && typeof err === 'object' && 'response' in err ? (err.response as { data?: unknown } | null) : null
  const data = ax && typeof ax === 'object' && 'data' in ax ? ax.data : null
  const type = data && typeof data === 'object' && 'type' in data ? data.type : null
  return type === LOGIN_EMAIL_NOT_CONFIRMED_TYPE
}
