import httpClient from './httpClient'

/**
 * Saved games API client (ASP.NET Core `/api/saved-games`). Uses camelCase JSON matching backend DTOs.
 */
const databaseService = {
  /**
   * @returns {Promise<import('@/types/api-dtos').GameDto[]>}
   */
  async getSavedGames() {
    try {
      const response = await httpClient.get('/saved-games')
      return response.data || []
    } catch (error) {
      console.error('Error retrieving saved games:', error)
      throw error
    }
  },

  /**
   * @param {number} gameId
   * @returns {Promise<boolean>}
   */
  async isGameSaved(gameId) {
    try {
      const response = await httpClient.get(`/saved-games/check/${gameId}`)
      return response.data.isSaved || false
    } catch (error) {
      console.error('Error checking if game is saved:', error)
      throw error
    }
  },

  /**
   * @param {import('@/types/api-dtos').RawgGameLike} game
   * @returns {Promise<boolean>}
   */
  async saveGame(game) {
    try {
      const gameToSave = {
        id: game.id,
        name: game.name,
        backgroundImage: game.background_image,
        rating: game.rating,
        released: game.released,
        genres: game.genres ? game.genres.map((g) => ({ id: g.id, name: g.name, slug: g.slug })) : [],
        platforms: game.platforms
          ? game.platforms.map((p) => ({
              id: p.platform.id,
              name: p.platform.name,
              slug: p.platform.slug,
            }))
          : [],
        metacritic: game.metacritic,
        descriptionRaw: game.description_raw,
      }

      const response = await httpClient.post('/saved-games', gameToSave)

      if (response.status === 201) {
        return true
      }
      return false
    } catch (error) {
      if (error.response?.status === 409) {
        return false
      }
      console.error('Error saving game:', error)
      throw error
    }
  },

  /**
   * @param {number} gameId
   * @returns {Promise<boolean>}
   */
  async removeGame(gameId) {
    try {
      const response = await httpClient.delete(`/saved-games/${gameId}`)

      if (response.status === 200) {
        return true
      }
      return false
    } catch (error) {
      if (error.response?.status === 404) {
        return false
      }
      console.error('Error removing game:', error)
      throw error
    }
  },

  /**
   * @param {number} gameId
   * @returns {Promise<import('@/types/api-dtos').GameDto|null>}
   */
  async getSavedGame(gameId) {
    try {
      const response = await httpClient.get(`/saved-games/${gameId}`)
      return response.data || null
    } catch (error) {
      if (error.response?.status === 404) {
        return null
      }
      console.error('Error retrieving saved game:', error)
      throw error
    }
  },

  /**
   * @returns {Promise<boolean>}
   */
  async clearAllSavedGames() {
    try {
      const response = await httpClient.delete('/saved-games')

      if (response.status === 200) {
        return true
      }
      return false
    } catch (error) {
      console.error('Error clearing saved games:', error)
      throw error
    }
  },

  /**
   * @returns {Promise<import('@/types/api-dtos').GameStatsDto>}
   */
  async getStatistics() {
    try {
      const response = await httpClient.get('/saved-games/statistics')
      return (
        response.data.statistics || {
          totalGames: 0,
          averageRating: 0,
          genreCount: {},
          platformCount: {},
        }
      )
    } catch (error) {
      console.error('Error retrieving statistics:', error)
      throw error
    }
  },

  /**
   * @param {string} query
   * @returns {Promise<import('@/types/api-dtos').GameDto[]>}
   */
  async searchSavedGames(query) {
    try {
      const response = await httpClient.get(`/saved-games/search?q=${encodeURIComponent(query)}`)
      return response.data.games || []
    } catch (error) {
      console.error('Error searching saved games:', error)
      throw error
    }
  },

  /**
   * Personal note and rating use `PUT /saved-games/{id}` with UpdateGameDto (`personalRating`, `note`).
   * @param {number} gameId
   * @param {import('@/types/api-dtos').UpdateGameDto} updateData
   * @returns {Promise<boolean>}
   */
  async updateSavedGame(gameId, updateData) {
    try {
      const response = await httpClient.put(`/saved-games/${gameId}`, updateData)

      if (response.status === 200) {
        return true
      }
      return false
    } catch (error) {
      console.error('Error updating saved game:', error)
      throw error
    }
  },

  /**
   * @returns {Promise<unknown>}
   */
  async exportSavedGames() {
    try {
      const response = await httpClient.get('/saved-games/export')
      return response.data
    } catch (error) {
      console.error('Error exporting saved games:', error)
      throw error
    }
  },

  /**
   * @param {unknown} importData
   * @returns {Promise<boolean>}
   */
  async importSavedGames(importData) {
    try {
      const response = await httpClient.post('/saved-games/import', importData)

      if (response.status === 200) {
        return true
      }
      return false
    } catch (error) {
      console.error('Error importing saved games:', error)
      throw error
    }
  },
}

export default databaseService
