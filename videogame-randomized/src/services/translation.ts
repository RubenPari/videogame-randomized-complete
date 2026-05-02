import httpClient from './httpClient'

const translationService = {
  async translate(text: string, sourceLang = 'en', targetLang = 'it'): Promise<string> {
    if (!text || text.trim() === '') {
      return 'Description not available.'
    }

    try {
      const response = await httpClient.post('/translate', {
        text,
        source: sourceLang,
        target: targetLang,
      })

      if (
        response.data &&
        response.data.data &&
        response.data.data.translations &&
        response.data.data.translations.length > 0
      ) {
        return response.data.data.translations[0].translatedText
      } else {
        console.error('Invalid response from Google Cloud Translation API:', response.data)
      }
    } catch (error: unknown) {
      console.error('Error during translation with backend proxy:', error)
      const axiosError = error as { response?: { status?: number } }
      if (axiosError.response) {
        if (axiosError.response.status === 403 || axiosError.response.status === 401) {
          console.warn('Authentication error with Google Cloud Translation API')
        }

        if (axiosError.response.status === 413) {
          const maxLength = 5000
          if (text.length > maxLength) {
            const firstPart = text.substring(0, maxLength)
            try {
              return await this.translate(firstPart, sourceLang, targetLang)
            } catch {
              return this.fallbackTranslation(text)
            }
          }
        }
      }

      return this.fallbackTranslation(text)
    }
    return this.fallbackTranslation(text)
  },

  fallbackTranslation(text: string): string {
    if (!text) return 'Description not available.'

    const shortenedText = text.substring(0, 300)

    return `${shortenedText}... [Translation not available. Showing abbreviated original text.]`
  },
}

export default translationService
