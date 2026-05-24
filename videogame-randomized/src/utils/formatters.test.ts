import { describe, it, expect } from 'vitest'
import { formatDate } from '@/utils/formatters'

describe('formatters', () => {
  describe('formatDate', () => {
    it('should format a valid date string', () => {
      const result = formatDate('2024-01-15', 'en')
      expect(result).toBeTruthy()
    })

    it('should return placeholder for null/undefined', () => {
      const result = formatDate(null as unknown as string, 'en')
      expect(result).toBe('UNKNOWN')
    })

    it('should return placeholder for empty string', () => {
      const result = formatDate('', 'en')
      expect(result).toBe('UNKNOWN')
    })
  })
})
