import { describe, it, expect } from 'vitest'
import { getApiErrorMessage } from './apiError.js'

describe('getApiErrorMessage', () => {
  it('reads RFC 7807 detail', () => {
    const err = { response: { data: { detail: 'Invalid credentials', title: 'Login failed' } } }
    expect(getApiErrorMessage(err, 'fallback')).toBe('Invalid credentials')
  })

  it('falls back to legacy error string', () => {
    const err = { response: { data: { error: 'Old shape' } } }
    expect(getApiErrorMessage(err, 'fallback')).toBe('Old shape')
  })

  it('joins registration errors array', () => {
    const err = { response: { data: { errors: ['a', 'b'] } } }
    expect(getApiErrorMessage(err, 'fallback')).toBe('a, b')
  })

  it('uses fallback when empty', () => {
    expect(getApiErrorMessage({}, 'x')).toBe('x')
  })
})
