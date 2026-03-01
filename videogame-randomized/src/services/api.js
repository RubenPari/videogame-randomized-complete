/**
 * API service for communicating with RAWG API and managing translations
 *
 * This module provides a centralized interface for:
 * - Retrieving video game data from the RAWG API
 * - Managing authentication with API key
 * - Translating game descriptions
 * - Handling network and translation errors
 */

// Import axios for HTTP requests
import axios from 'axios'
// Import translation service to convert texts to Italian
import translationService from './translation'

// Constants for RAWG API configuration
const API_KEY = import.meta.env.VITE_RAWG_API_KEY || 'YOUR_RAWG_API_KEY'  // API key from environment variable
const BASE_URL = 'https://api.rawg.io/api'                               // Base URL for RAWG API

/**
 * Configured Axios instance for RAWG API
 * Automatically includes API key in all requests
 */
const apiClient = axios.create({
  baseURL: BASE_URL,     // Base URL for all requests
  params: {
    key: API_KEY,        // API key automatically added to every request
  },
})

/**
 * Object with all methods for interacting with the RAWG API
 * Each method returns a Promise with the API response
 */
export default {
  /**
   * Retrieves all available game genres
   * @returns {Promise} Promise that resolves with the list of genres
   * @example
   * const response = await api.getGenres()
   * const genres = response.data.results
   */
  getGenres() {
    return apiClient.get('/genres')
  },

  /**
   * Retrieves all available gaming platforms
   * @returns {Promise} Promise that resolves with the list of platforms
   * @example
   * const response = await api.getPlatforms()
   * const platforms = response.data.results
   */
  getPlatforms() {
    return apiClient.get('/platforms')
  },

  /**
   * Retrieves a list of games with optional filters
   * @param {Object} params - Search and filter parameters
   * @param {string} params.genres - Genre IDs separated by comma
   * @param {string} params.platforms - Platform IDs separated by comma
   * @param {string} params.dates - Date range (YYYY-MM-DD,YYYY-MM-DD)
   * @param {string} params.metacritic - Metacritic score range (min,max)
   * @param {string} params.ordering - Sorting criteria (-rating, -released, etc.)
   * @param {number} params.page_size - Number of results per page
   * @returns {Promise} Promise that resolves with the list of filtered games
   * @example
   * const response = await api.getGames({ genres: '4,51', metacritic: '80,100' })
   * const games = response.data.results
   */
  getGames(params = {}) {
    return apiClient.get('/games', {
      params: {
        ...params,  // Spread the received parameters
      },
    })
  },

  /**
   * Retrieves complete details of a specific game
   * Includes additional information such as full description, screenshots, etc.
   * @param {number|string} gameId - Unique game ID
   * @returns {Promise} Promise that resolves with the game details
   * @example
   * const response = await api.getGameDetails(3498)
   * const gameDetails = response.data
   */
  getGameDetails(gameId) {
    return apiClient.get(`/games/${gameId}`)
  },

  /**
   * Translates a game description from English to Italian
   * Uses the translation service and handles errors with fallback
   * @param {string} text - English text to translate
   * @returns {Promise<string>} Promise that resolves with the translated text in Italian
   * @example
   * const description = await api.translateGameDescription(englishDescription)
   */
  async translateGameDescription(text) {
    // Handle the case of empty or undefined text
    if (!text) return 'Description not available.'

    try {
      // Use the translation service to convert from English to Italian
      return await translationService.translate(text, 'en', 'it')
    } catch (error) {
      console.error('Error during translation:', error)
      // In case of error, use a fallback translation
      // This ensures the user always receives a description, even if not translated
      return translationService.fallbackTranslation(text)
    }
  },
}
