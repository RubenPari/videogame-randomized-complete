import { ref, reactive, computed } from 'vue'
import apiService from '@/services/api'
import httpClient from '@/services/httpClient'

/**
 * useGameDiscovery
 * Composable for managing game discovery logic, including filters,
 * generation, history, and description fetching/translation.
 * Integrates with persistent discovery log for cross-session exclusion.
 */
export function useGameDiscovery() {
  // Reactive state
  const currentGame = ref(null)
  const gameDescription = ref('')
  const isLoading = ref(false)
  const error = ref(null)
  const gameHistory = ref([])       // Current session history
  const pastHistory = ref([])        // Loaded from server (previous sessions)
  const totalGamesCount = ref(0)

  // All excluded IDs = current session + past sessions
  const allExcludedIds = computed(() => {
    const currentIds = gameHistory.value.map(g => g.id)
    const pastIds = pastHistory.value.map(g => g.id)
    return new Set([...currentIds, ...pastIds])
  })

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
   * Load discovery log from the server (previous sessions)
   */
  const loadPastHistory = async () => {
    try {
      const response = await httpClient.get('/discovery-log')
      pastHistory.value = response.data || []
    } catch (err) {
      console.error('Failed to load discovery log:', err)
      pastHistory.value = []
    }
  }

  /**
   * Clear all discovery log entries from the server
   */
  const clearPastHistory = async () => {
    try {
      await httpClient.delete('/discovery-log')
      pastHistory.value = []
    } catch (err) {
      console.error('Failed to clear discovery log:', err)
      throw err
    }
  }

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

      // Filter out already seen games (current session + past sessions)
      const filtered = response.data.results.filter(
        game => game.rating >= filters.minRating &&
                !allExcludedIds.value.has(game.id)
      )

      if (filtered.length === 0) {
        error.value = 'Exhausted unique results for these parameters.'
        return
      }

      // Random selection
      const selected = filtered[Math.floor(Math.random() * filtered.length)]

      // Update history and auto-save to server
      const entry = { id: selected.id, name: selected.name }
      gameHistory.value.push(entry)
      if (!pastHistory.value.some(p => p.id === entry.id)) {
        pastHistory.value = [...pastHistory.value, entry]
      }
      httpClient.post('/discovery-log', [entry]).catch(err => {
        console.error('Failed to auto-save discovery log entry:', err)
      })

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
      const englishDescription = response.data.description

      gameDescription.value = await apiService.translateGameDescription(englishDescription)
    } catch (err) {
      console.error('Translation failure:', err)
      gameDescription.value = 'Data corrupted: unable to translate entry.'
    }
  }

  /**
   * Resets current session history
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
    pastHistory,
    totalGamesCount,
    filters,
    generateGame,
    clearHistory,
    loadPastHistory,
    clearPastHistory
  }
}
