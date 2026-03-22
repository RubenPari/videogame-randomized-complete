/**
 * Format a date string using the given locale.
 * Falls back to 'en' if no locale is provided.
 */
export const formatDate = (dateString, locale = 'en') => {
  if (!dateString) return 'UNKNOWN'
  return new Date(dateString)
    .toLocaleDateString(locale, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
    .toUpperCase()
}
