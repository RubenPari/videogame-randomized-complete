import httpClient from './httpClient'

/**
 * Database service that manages saved games via PostgreSQL backend
 * Provides methods to add, remove, and check saved games
 */
const databaseService = {
  /**
   * Retrieves all saved games from backend
   * @returns {Promise<Array>} Promise that resolves with array of saved game objects
   */
  async getSavedGames() {
    try {
      const response = await httpClient.get('/saved-games')
      // Minimal API returns array directly or wrapped?
      // Endpoints: TypedResults.Ok(mapper.GamesToDtos(games)) -> This usually serializes to array directly or depends on wrapper?
      // Our endpoint returns TypedResults.Ok(mapper.GamesToDtos(games)) which is list.
      // But typically JSON is pure list or object.
      // Let's check API. It returns list.
      // But wait, the previous implementation expected `response.data.games`.
      // The new endpoint: `TypedResults.Ok(mapper.GamesToDtos(games))` returns the list as body.
      // BUT `SavedGamesEndpoints.cs` had:
      // private static async Task<Ok<List<GameDto>>> GetSavedGames(...)
      // return TypedResults.Ok(mapper.GamesToDtos(games));
      // This serializes the list as the root JSON element.
      // HOWEVER, looking at `SavedGamesController` refactoring in `SavedGamesEndpoints.cs` step, I wrote:
      // return TypedResults.Ok(mapper.GamesToDtos(games));
      // Wait, let me check the written file content again.
      // In `SavedGamesEndpoints.cs`: `return TypedResults.Ok(mapper.GamesToDtos(games));`
      // This means the response.data IS the array.
      // OLD code expected: response.data.games
      // I should align.
      // To keep frontend changes minimal, maybe I should wrap it?
      // The prompt asked for optimization. Returning array directly is standard.
      // I will update frontend to expect array directly.
      return response.data || []
    } catch (error) {
      console.error('Error retrieving saved games:', error)
      return []
    }
  },

  /**
   * Checks if a game is already saved in the database
   * @param {number} gameId - ID of the game to check
   * @returns {Promise<boolean>} Promise that resolves with true if saved, false otherwise
   */
  async isGameSaved(gameId) {
    try {
      const response = await httpClient.get(`/saved-games/check/${gameId}`)
      // Endpoint returns { isSaved: bool }
      return response.data.isSaved || false
    } catch (error) {
      console.error('Error checking if game is saved:', error)
      return false
    }
  },

  /**
   * Saves a game to the backend
   * @param {Object} game - Game object to save
   * @returns {Promise<boolean>} Promise that resolves with true if saved successfully
   */
  async saveGame(game) {
    try {
      // Create a clean game object for storage matching CreateGameDto
      const gameToSave = {
        id: game.id,
        name: game.name,
        backgroundImage: game.background_image, // camelCase mapping for backend
        rating: game.rating,
        released: game.released,
        genres: game.genres ? game.genres.map(g => ({ id: g.id, name: g.name, slug: g.slug })) : [],
        platforms: game.platforms ? game.platforms.map(p => ({ id: p.platform.id, name: p.platform.name, slug: p.platform.slug })) : [],
        metacritic: game.metacritic,
        descriptionRaw: game.description_raw, // camelCase
      }

      const response = await httpClient.post('/saved-games', gameToSave)

      if (response.status === 201) {
        console.log('Game saved successfully:', game.name)
        return true
      }
      return false
    } catch (error) {
      if (error.response?.status === 409) {
        console.warn('Game is already saved:', game.name)
        return false
      }
      console.error('Error saving game:', error)
      return false
    }
  },

  /**
   * Removes a game from the saved collection
   * @param {number} gameId - ID of the game to remove
   * @returns {Promise<boolean>} Promise that resolves with true if removed successfully
   */
  async removeGame(gameId) {
    try {
      const response = await httpClient.delete(`/saved-games/${gameId}`)

      if (response.status === 200) {
        console.log('Game removed successfully:', gameId)
        return true
      }
      return false
    } catch (error) {
      if (error.response?.status === 404) {
        console.warn('Game not found in saved games:', gameId)
        return false
      }
      console.error('Error removing game:', error)
      return false
    }
  },

  /**
   * Gets a saved game by ID
   * @param {number} gameId - ID of the game to retrieve
   * @returns {Promise<Object|null>} Promise that resolves with game object or null
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
      return null
    }
  },

  /**
   * Clears all saved games for the current user
   * @returns {Promise<boolean>} Promise that resolves with true if cleared successfully
   */
  async clearAllSavedGames() {
    try {
      const response = await httpClient.delete('/saved-games')

      if (response.status === 200) {
        console.log('All saved games cleared')
        return true
      }
      return false
    } catch (error) {
      console.error('Error clearing saved games:', error)
      return false
    }
  },

  /**
   * Gets statistics about saved games
   * @returns {Promise<Object>} Promise that resolves with statistics object
   */
  async getStatistics() {
    try {
      const response = await httpClient.get('/saved-games/statistics')
      // Endpoint returns { statistics: ... }
      return response.data.statistics || {
        totalGames: 0,
        averageRating: 0,
        genreCount: {},
        platformCount: {}
      }
    } catch (error) {
      console.error('Error retrieving statistics:', error)
      return {
        totalGames: 0,
        averageRating: 0,
        genreCount: {},
        platformCount: {}
      }
    }
  },

  /**
   * Searches saved games by name
   * @param {string} query - Search query
   * @returns {Promise<Array>} Promise that resolves with filtered games array
   */
  async searchSavedGames(query) {
    try {
      const response = await httpClient.get(`/saved-games/search?q=${encodeURIComponent(query)}`)
      // Endpoint returns { games: [...] }
      return response.data.games || []
    } catch (error) {
      console.error('Error searching saved games:', error)
      return []
    }
  },

  /**
   * Updates a saved game's information
   * @param {number} gameId - ID of the game to update
   * @param {Object} updateData - Data to update (e.g. { personalRating, note })
   * @returns {Promise<boolean>} Promise that resolves with true if updated successfully
   */
  async updateSavedGame(gameId, updateData) {
    try {
      const response = await httpClient.put(`/saved-games/${gameId}`, updateData)

      if (response.status === 200) {
        console.log('Game updated successfully:', gameId)
        return true
      }
      return false
    } catch (error) {
      console.error('Error updating saved game:', error)
      return false
    }
  },

  /**
   * Exports all saved games as JSON
   * @returns {Promise<Object>} Promise that resolves with export data
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
   * Imports saved games from JSON data
   * @param {Object} importData - Data to import (List of games)
   * @returns {Promise<boolean>} Promise that resolves with true if imported successfully
   */
  async importSavedGames(importData) {
    try {
      // Ensure data structure matches CreateGameDto
      // Import data usually comes from export, which matches GameDto.
      // CreateGameDto is subset.
      // Need to ensure keys are camelCase if they weren't already?
      // Export is serialized by backend -> camelCase.
      // So importData should be camelCase list of games.
      const response = await httpClient.post('/saved-games/import', importData)

      if (response.status === 200) {
        console.log('Games imported successfully')
        return true
      }
      return false
    } catch (error) {
      console.error('Error importing saved games:', error)
      return false
    }
  },

  /**
   * Adds a personal note to a saved game
   * @param {number} gameId - ID of the game
   * @param {string} note - Personal note to add
   * @returns {Promise<boolean>} Promise that resolves with true if note added successfully
   */
  async addGameNote(gameId, note) {
    try {
      const response = await httpClient.post(`/saved-games/${gameId}/note`, { note })

      if (response.status === 200) {
        console.log('Note added successfully to game:', gameId)
        return true
      }
      return false
    } catch (error) {
      console.error('Error adding note to game:', error)
      return false
    }
  },

  /**
   * Adds a rating to a saved game
   * @param {number} gameId - ID of the game
   * @param {number} personalRating - Personal rating (1-5)
   * @returns {Promise<boolean>} Promise that resolves with true if rating added successfully
   */
  async rateGame(gameId, personalRating) {
    try {
      const response = await httpClient.post(`/saved-games/${gameId}/rating`, {
        personalRating: Math.max(1, Math.min(5, personalRating))
      })

      if (response.status === 200) {
        console.log('Rating added successfully to game:', gameId)
        return true
      }
      return false
    } catch (error) {
      console.error('Error rating game:', error)
      return false
    }
  }
}

export default databaseService
