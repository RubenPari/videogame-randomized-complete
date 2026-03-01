import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import databaseService from '@/services/database'

/**
 * useVaultStore
 * Centralized store for managing the user's saved games collection.
 * Replaces old state management patterns with a modern Pinia store.
 */
export const useVaultStore = defineStore('vault', () => {
  // State: Reactive list of saved games
  const savedGames = ref([])
  const isLoading = ref(false)

  // Getters: Computed property for total saved games count
  const count = computed(() => savedGames.value.length)

  // Getters: Statistics about the collection
  const statistics = ref({
    totalGames: 0,
    averageRating: 0,
    genreCount: {},
    platformCount: {}
  })

  /**
   * Loads all saved games and statistics from the database service
   */
  const loadVault = async () => {
    isLoading.value = true
    try {
      savedGames.value = await databaseService.getSavedGames()
      statistics.value = await databaseService.getStatistics()
    } catch (error) {
      console.error('Failed to load vault data:', error)
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Toggles the save status of a game in the collection
   * @param {Object} game - The game object to save or remove
   * @returns {Promise<boolean>} - New save status (true if saved, false if removed)
   */
  const toggleGame = async (game) => {
    const isSaved = savedGames.value.some(g => g.id === game.id)
    
    if (isSaved) {
      await databaseService.removeGame(game.id)
    } else {
      await databaseService.saveGame(game)
    }
    
    // Refresh local state
    await loadVault()
    return !isSaved
  }

  /**
   * Removes a specific game by ID
   * @param {number} gameId - ID of the game to remove
   */
  const removeGame = async (gameId) => {
    await databaseService.removeGame(gameId)
    await loadVault()
  }

  /**
   * Clears the entire saved games collection
   */
  const clearVault = async () => {
    await databaseService.clearAllSavedGames()
    await loadVault()
  }

  /**
   * Checks if a specific game is currently saved
   * @param {number} gameId - ID of the game to check
   * @returns {boolean}
   */
  const isGameSaved = (gameId) => {
    return savedGames.value.some(g => g.id === gameId)
  }

  return {
    savedGames,
    isLoading,
    count,
    statistics,
    loadVault,
    toggleGame,
    removeGame,
    clearVault,
    isGameSaved
  }
})
