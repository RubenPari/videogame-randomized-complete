/**
 * API service for communicating with RAWG API and managing translations
 *
 * This module provides a centralized interface for:
 * - Retrieving video game data from the RAWG API
 * - Managing authentication with API key
 * - Translating game descriptions
 * - Handling network and translation errors
 */

// Import translation service to convert texts to Italian
import translationService from './translation'
import httpClient from './httpClient'

const rawgClient = {
  get(path, config) {
    return httpClient.get(`/rawg${path}`, config)
  },
}

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
    return rawgClient.get('/genres')
  },

  /**
   * Retrieves all available gaming platforms
   * @returns {Promise} Promise that resolves with the list of platforms
   * @example
   * const response = await api.getPlatforms()
   * const platforms = response.data.results
   */
  getPlatforms() {
    return rawgClient.get('/platforms')
  },

  /**
   * Retrieves a list of games with optional filters
   * @param {Object} params - Search and filter parameters
   * @param {string} params.genres - Genre IDs separated by comma
   * @param {string} params.platforms - Platform IDs separated by comma
   * @param {string} params.dates - Date range (YYYY-MM-DD,YYYY-MM-DD)
   * @param {number} params.page_size - Number of results per page
   * @returns {Promise} Promise that resolves with the list of filtered games
   * @example
   * const response = await api.getGames({ genres: '4,51' })
   * const games = response.data.results
   */
  getGames(params = {}) {
    return rawgClient.get('/games', {
      params: {
        ...params, // Spread the received parameters
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
    return rawgClient.get(`/games/${gameId}`)
  },

  /**
   * Retrieves screenshots of a specific game
   * @param {number|string} gameId - Unique game ID
   * @returns {Promise} Promise that resolves with the game screenshots
   */
  getGameScreenshots(gameId) {
    return rawgClient.get(`/games/${gameId}/screenshots`)
  },

  /**
   * Retrieves trailers/movies of a specific game
   * @param {number|string} gameId - Unique game ID
   * @returns {Promise} Promise that resolves with the game movies
   */
  getGameMovies(gameId) {
    return rawgClient.get(`/games/${gameId}/movies`)
  },

  /**
   * Searches YouTube for a game trailer
   * @param {string} gameName - Name of the game
   * @returns {Promise<string|null>} YouTube video ID or null
   */
  async searchYouTubeTrailer(gameName) {
    if (!gameName) return null

    try {
      const response = await httpClient.get('/youtube/search', {
        params: { q: `${gameName} official trailer` },
      })
      return response.data?.videoId || null
    } catch (error) {
      console.error('YouTube trailer search failed:', error)
      return null
    }
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
