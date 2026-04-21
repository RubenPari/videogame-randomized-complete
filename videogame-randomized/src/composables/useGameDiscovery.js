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

  const highRatingPoolCache = ref(new Map())

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
    endYear: new Date().getFullYear()
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
      const pageSize = 40
      const maxAttempts = 20

      const buildBaseParams = () => {
        const baseParams = {
          page_size: pageSize,
        }

        if (filters.genre) baseParams.genres = filters.genre
        if (filters.platforms?.length > 0) baseParams.platforms = filters.platforms.join(',')

        const startDate = filters.startYear ? `${filters.startYear}-01-01` : '1980-01-01'
        const endDate = filters.endYear
          ? `${filters.endYear}-12-31`
          : `${new Date().getFullYear()}-12-31`
        baseParams.dates = `${startDate},${endDate}`

        return baseParams
      }

      const buildFilterKey = (ordering) => {
        const genre = filters.genre || ''
        const platforms = (filters.platforms || []).slice().sort((a, b) => a - b).join(',')
        const startYear = filters.startYear ?? ''
        const endYear = filters.endYear ?? ''
        const minRating = Number(filters.minRating || 0).toFixed(1)
        return `${genre}|${platforms}|${startYear}|${endYear}|${minRating}|${ordering || ''}`
      }

      const randomInt = (min, max) =>
        Math.floor(Math.random() * (max - min + 1)) + min

      const is404 = (e) => e?.response?.status === 404

      const shouldUseHighRatingMode = Number(filters.minRating || 0) > 0
      const baseParams = buildBaseParams()

      const findMaxValidPage = async (upperBound) => {
        // RAWG can return 404 for out-of-range pages even when `count` is large.
        // We binary-search the highest page that returns a 2xx response.
        let lo = 1
        let hi = upperBound
        let best = 1

        // Limit probes to keep UX snappy (\(\log_2(upperBound)\) capped).
        for (let i = 0; i < 14 && lo <= hi; i++) {
          const mid = Math.floor((lo + hi) / 2)
          try {
            await apiService.getGames({
              ...baseParams,
              page: mid,
              page_size: 1,
            })
            best = mid
            lo = mid + 1
          } catch (e) {
            if (is404(e)) {
              hi = mid - 1
              continue
            }
            throw e
          }
        }

        return Math.max(1, best)
      }

      const pickFromResults = (results) => {
        if (!Array.isArray(results) || results.length === 0) return null
        const candidates = results.filter(
          (g) =>
            g &&
            typeof g.rating === 'number' &&
            g.rating >= filters.minRating &&
            !allExcludedIds.value.has(g.id),
        )
        if (candidates.length === 0) return null
        return candidates[Math.floor(Math.random() * candidates.length)]
      }

      const ensureHighRatingScanProgress = async () => {
        const ordering = '-rating'
        const key = buildFilterKey(ordering)

        const cached = highRatingPoolCache.value.get(key) || null
        const state = cached || {
          allAboveMin: [],
          anyAboveMin: false,
          nextPage: 1,
          done: false,
          seenIds: new Set(),
        }

        const params = {
          ...baseParams,
          ordering,
          page_size: pageSize,
        }

        // Scan forward until we either find at least one not-excluded candidate
        // or we are certain there are no more games above the rating threshold.
        while (!state.done) {
          let res
          try {
            res = await apiService.getGames({ ...params, page: state.nextPage })
          } catch (e) {
            if (is404(e)) {
              state.done = true
              break
            }
            throw e
          }

          const results = Array.isArray(res.data?.results) ? res.data.results : []
          if (results.length === 0) {
            state.done = true
            break
          }

          // RAWG ordering=-rating means pages are descending by rating.
          // Once the first rated element drops below the threshold, we can stop scanning.
          let firstRated = null

          for (const g of results) {
            if (!g || typeof g.rating !== 'number') continue

            if (g.rating >= filters.minRating) {
              if (firstRated === null) firstRated = g.rating
              state.anyAboveMin = true
              if (!state.seenIds.has(g.id)) {
                state.seenIds.add(g.id)
                state.allAboveMin.push(g)
              }
            } else if (firstRated === null) {
              // The first rated element is already below threshold => we're done.
              firstRated = g.rating
            }
          }

          if (typeof firstRated === 'number' && firstRated < filters.minRating) {
            state.done = true
          } else {
            state.nextPage += 1
          }

          // Stop scanning early if we already have at least one not-excluded candidate available.
          if (state.allAboveMin.some((g) => g && !allExcludedIds.value.has(g.id))) {
            break
          }
        }

        // Simple cache bound to avoid unbounded growth.
        if (highRatingPoolCache.value.size >= 20) highRatingPoolCache.value.clear()
        highRatingPoolCache.value.set(key, state)
        return { state, key }
      }

      let selected = null

      if (shouldUseHighRatingMode) {
        const { state } = await ensureHighRatingScanProgress()
        const pool = state.allAboveMin.filter((g) => g && !allExcludedIds.value.has(g.id))
        if (pool.length > 0) {
          selected = pool[Math.floor(Math.random() * pool.length)]
        } else {
          error.value = state.anyAboveMin
            ? 'Exhausted unique results for these parameters.'
            : 'No games found. Adjust your parameters.'
          return
        }
      } else {
        // First, get count cheaply
        const countRes = await apiService.getGames({
          ...baseParams,
          page: 1,
          page_size: 1,
        })

        const count = Number(countRes.data?.count || 0)
        totalGamesCount.value = count

        if (count <= 0) {
          error.value = 'No games found. Adjust your parameters.'
          return
        }

        const maxPage = Math.max(1, Math.ceil(count / pageSize))
        const maxValidPage = await findMaxValidPage(maxPage)

        for (let attempt = 0; attempt < maxAttempts; attempt++) {
          const page = randomInt(1, maxValidPage)
          try {
            const res = await apiService.getGames({
              ...baseParams,
              page,
              page_size: pageSize,
            })

            selected = pickFromResults(res.data?.results)
          } catch {
            // RAWG can return 404 for out-of-range pages on some queries.
            selected = null
          }
          if (selected) break
        }
      }

      if (!selected) {
        // In high-rating mode, absence of selection at this point means we scanned everything above threshold.
        if (shouldUseHighRatingMode) {
          error.value = 'Exhausted unique results for these parameters.'
          return
        }

        // Fallback: sample a few pages and pick from the union
        const countRes = await apiService.getGames({
          ...baseParams,
          page: 1,
          page_size: 1,
        })
        const count = Number(countRes.data?.count || 0)
        if (count <= 0) {
          error.value = 'No games found. Adjust your parameters.'
          return
        }
        const maxPage = Math.max(1, Math.ceil(count / pageSize))
        const maxValidPage = await findMaxValidPage(maxPage)

        const pagesToTry = Math.min(5, maxValidPage)
        const pages = new Set()
        while (pages.size < pagesToTry) pages.add(randomInt(1, maxValidPage))

        const responses = await Promise.allSettled(
          [...pages].map((page) => apiService.getGames({ ...baseParams, page, page_size: pageSize })),
        )

        const union = responses
          .filter((r) => r.status === 'fulfilled')
          .flatMap((r) => r.value.data?.results || [])
        selected = pickFromResults(union)
      }

      if (!selected) {
        error.value = 'Exhausted unique results for these parameters.'
        return
      }

      // Update history and auto-save to server
      const entry = { id: selected.id, name: selected.name }
      gameHistory.value.push(entry)
      if (!pastHistory.value.some(p => p.id === entry.id)) {
        pastHistory.value = [...pastHistory.value, entry]
      }
      try {
        await httpClient.post('/discovery-log', [entry])
      } catch (err) {
        console.error('Failed to auto-save discovery log entry:', err)
      }

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
   * Loads a specific game by ID from the RAWG API and displays it
   * @param {number} gameId
   */
  const loadGameById = async (gameId) => {
    isLoading.value = true
    error.value = null

    try {
      const response = await apiService.getGameDetails(gameId)
      currentGame.value = response.data
      await fetchGameDetails(gameId)
    } catch (err) {
      console.error('Failed to load game:', err)
      error.value = 'System failure: unable to retrieve classified data.'
    } finally {
      isLoading.value = false
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
    loadGameById,
    clearHistory,
    loadPastHistory,
    clearPastHistory
  }
}
