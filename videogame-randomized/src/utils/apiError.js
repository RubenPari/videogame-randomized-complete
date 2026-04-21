/**
 * Normalizes API error payloads from ASP.NET (RFC 7807 Problem Details) and legacy shapes.
 * @param {unknown} err
 * @param {string} fallback
 * @returns {string}
 */
export function getApiErrorMessage(err, fallback) {
  const ax = err && typeof err === 'object' && 'response' in err ? err.response : null
  const data = ax && typeof ax === 'object' && 'data' in ax ? ax.data : null
  if (!data || typeof data !== 'object') {
    return err && typeof err === 'object' && 'message' in err && typeof err.message === 'string'
      ? err.message
      : fallback
  }
  if (typeof data.detail === 'string' && data.detail.length > 0) return data.detail
  if (typeof data.title === 'string' && data.title.length > 0 && typeof data.detail !== 'string') {
    return data.title
  }
  if (typeof data.error === 'string' && data.error.length > 0) return data.error
  if (Array.isArray(data.errors)) return data.errors.filter(Boolean).join(', ')
  if (data.errors && typeof data.errors === 'object') {
    const flat = Object.values(data.errors).flat()
    const first = flat.find((x) => typeof x === 'string')
    if (first) return first
  }
  return fallback
}

const LOGIN_EMAIL_NOT_CONFIRMED_TYPE = 'urn:videogame-randomizer:login:email-not-confirmed'

/**
 * Detects a failed login due to unconfirmed email (Problem Details `type`).
 * @param {unknown} err
 * @returns {boolean}
 */
export function isEmailNotConfirmedLoginError(err) {
  const ax = err && typeof err === 'object' && 'response' in err ? err.response : null
  const data = ax && typeof ax === 'object' && 'data' in ax ? ax.data : null
  const type = data && typeof data === 'object' && 'type' in data ? data.type : null
  return type === LOGIN_EMAIL_NOT_CONFIRMED_TYPE
}
