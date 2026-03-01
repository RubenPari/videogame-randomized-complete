import { ref, reactive } from 'vue'
import apiService from '@/services/api'

/**
 * useGameDiscovery
 * Composable for managing game discovery logic, including filters,
 * generation, history, and description fetching/translation.
 */
export function useGameDiscovery() {
  // Reactive state
  const currentGame = ref(null)
  const gameDescription = ref('')
  const isLoading = ref(false)
  const error = ref(null)
  const gameHistory = ref([])
  const totalGamesCount = ref(0)

  // Centralized filters using reactive object
  const filters = reactive({
    genre: '',
    platforms: [],
    minRating: 0,
    startYear: 2010,
    endYear: new Date().getFullYear(),
    ordering: '-rating'
  })

  /**
   * Generates a new random game based on current filters
   */
  const generateGame = async () => {
    isLoading.value = true
    error.value = null

    try {
      // Build search parameters
      const params = {
        ordering: filters.ordering,
        page_size: 40,
        metacritic: filters.minRating > 0 
          ? `${Math.floor(filters.minRating * 20)},100` 
          : undefined
      }

      if (filters.genre) params.genres = filters.genre
      if (filters.platforms?.length > 0) params.platforms = filters.platforms.join(',')
      
      const startDate = filters.startYear ? `${filters.startYear}-01-01` : '1980-01-01'
      const endDate = filters.endYear ? `${filters.endYear}-12-31` : `${new Date().getFullYear()}-12-31`
      params.dates = `${startDate},${endDate}`

      // Fetch games
      const response = await apiService.getGames(params)
      
      if (!response.data.results || response.data.results.length === 0) {
        error.value = 'No games found. Adjust your parameters.'
        return
      }

      totalGamesCount.value = response.data.count

      // Filter out already seen games
      const filtered = response.data.results.filter(
        game => game.rating >= filters.minRating && 
                !gameHistory.value.some(h => h.id === game.id)
      )

      if (filtered.length === 0) {
        error.value = 'Exhausted unique results for these parameters.'
        return
      }

      // Random selection
      const selected = filtered[Math.floor(Math.random() * filtered.length)]
      
      // Update history
      gameHistory.value.push({ id: selected.id, name: selected.name })
      
      // Fetch details & translate
      await fetchGameDetails(selected.id)
      
      // Set as current
      currentGame.value = selected
    } catch (err) {
      console.error('Discovery failure:', err)
      error.value = 'System failure: unable to retrieve classified data.'
    } finally {
      isLoading.value = false
    }
  }

  /**
   * Fetches detailed information and handles translation
   * @param {number} gameId 
   */
  const fetchGameDetails = async (gameId) => {
    try {
      gameDescription.value = 'Decrypting database entry...'
      const response = await apiService.getGameDetails(gameId)
      const englishDescription = response.data.description_raw
      
      gameDescription.value = await apiService.translateGameDescription(englishDescription)
    } catch (err) {
      console.error('Translation failure:', err)
      gameDescription.value = 'Data corrupted: unable to translate entry.'
    }
  }

  /**
   * Resets session history
   */
  const clearHistory = () => {
    gameHistory.value = []
  }

  return {
    currentGame,
    gameDescription,
    isLoading,
    error,
    gameHistory,
    totalGamesCount,
    filters,
    generateGame,
    clearHistory
  }
}
