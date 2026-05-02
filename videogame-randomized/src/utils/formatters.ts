export const formatDate = (dateString: string | undefined | null, locale = 'en'): string => {
  if (!dateString) return 'UNKNOWN'
  return new Date(dateString)
    .toLocaleDateString(locale, {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    })
    .toUpperCase()
}
