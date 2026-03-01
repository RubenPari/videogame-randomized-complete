/**
 * Translation service using Google Cloud Translation API
 *
 * This module provides functionality to translate texts from English to Italian
 * using the Google Cloud Translation API. It includes:
 * - Automatic translation with error handling
 * - Automatic splitting of long texts
 * - Fallback for offline situations or API errors
 * - Handling of authentication and rate limiting errors
 */

// Import axios for HTTP requests
import axios from 'axios'

// Configuration for Google Cloud Translation API
const GOOGLE_TRANSLATE_API_URL = 'https://translation.googleapis.com/language/translate/v2'  // Google API endpoint
const GOOGLE_TRANSLATE_API_KEY = import.meta.env.VITE_GOOGLE_TRANSLATE_API_KEY || ''          // API key from environment variable

/**
 * Translation service that uses Google Cloud Translation API
 * Provides methods to translate texts and handle errors with fallback strategies
 */
const translationService = {
  /**
   * Translates a text from one language to another using Google Cloud Translation API
   * Automatically handles network errors, authentication, and texts that are too long
   *
   * @param {string} text - The text to translate
   * @param {string} sourceLang - Source language (default: 'en' for English)
   * @param {string} targetLang - Target language (default: 'it' for Italian)
   * @returns {Promise<string>} The translated text or a fallback message
   * @example
   * const translation = await translationService.translate('Hello world', 'en', 'it')
   * // Result: "Ciao mondo"
   */
  async translate(text, sourceLang = 'en', targetLang = 'it') {
    // Handles the case of empty text, undefined, or only spaces
    if (!text || text.trim() === '') {
      return 'Description not available.'
    }

    // Checks if the API key is configured in environment variables
    if (!GOOGLE_TRANSLATE_API_KEY) {
      console.error('Google Translate API key not configured')
      // If the API key is missing, use the fallback translation
      return this.fallbackTranslation(text)
    }

    try {
      // Builds the complete URL with the API key as a query parameter
      const url = `${GOOGLE_TRANSLATE_API_URL}?key=${GOOGLE_TRANSLATE_API_KEY}`

      // Prepares the request data according to Google API specifications
      const requestData = {
        q: text,               // Text to translate
        source: sourceLang,    // Source language
        target: targetLang,    // Target language
        format: 'text',        // Text format (text or html)
      }

      // Makes the POST request to Google Cloud Translation API
      const response = await axios.post(url, requestData)

      // Extracts the translated text from the response, verifying the data structure
      // Google's response has the structure: { data: { translations: [{ translatedText: "..." }] } }
      if (
        response.data &&
        response.data.data &&
        response.data.data.translations &&
        response.data.data.translations.length > 0
      ) {
        // Returns the first (and generally only) translation result
        return response.data.data.translations[0].translatedText
      } else {
        // Error log if the response doesn't have the expected structure
        console.error('Invalid response from Google Cloud Translation API:', response.data)
      }
    } catch (error) {
      console.error('Error during translation with Google Cloud Translation API:', error)

      // Handles specific errors from Google Cloud Translation API
      if (error.response) {
        // Authentication errors (invalid or expired API key)
        if (error.response.status === 403 || error.response.status === 401) {
          console.warn('Authentication error with Google Cloud Translation API')
        }

        // Error for text that is too long (Google limit: ~5000 characters)
        if (error.response.status === 413) {
          // Attempts to split the text into smaller parts
          const maxLength = 5000
          if (text.length > maxLength) {
            // Takes only the first part of the text to respect the limits
            const firstPart = text.substring(0, maxLength)
            try {
              // Recursively calls the function with the shortened text
              return await this.translate(firstPart, sourceLang, targetLang)
            } catch {
              // If even the shortened text fails, use the fallback
              return this.fallbackTranslation(text)
            }
          }
        }
      }

      // For all other errors not specifically handled, use the fallback translation
      return this.fallbackTranslation(text)
    }
  },

  /**
   * Fallback function when online translation is not available
   * Provides a shortened version of the original text with warning
   *
   * @param {string} text - The original text to process
   * @returns {string} A truncated version of the original text with warning
   *
   * This function is called when:
   * - The API key is not configured
   * - Network errors occur
   * - The Google API returns errors
   * - The text is too long even after splitting
   */
  fallbackTranslation(text) {
    // Handles the case of empty or undefined text
    if (!text) return 'Description not available.'

    // Truncates the text to 300 characters to maintain readability
    const shortenedText = text.substring(0, 300)

    // Returns the truncated text with a warning that translation is not available
    return `${shortenedText}... [Translation not available. Showing abbreviated original text.]`
  },
}

// Exports the translation service as default export
export default translationService
