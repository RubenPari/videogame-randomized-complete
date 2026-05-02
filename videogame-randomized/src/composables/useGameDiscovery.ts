import { ref, reactive, computed } from 'vue'
import apiService from '@/services/api'
import httpClient from '@/services/httpClient'

interface DiscoveryEntry {
  id: number
  name: string
}

const currentGame = ref<Record<string, unknown> | null>(null)
const gameDescription = ref('')
const isLoading = ref(false)
const error = ref<string | null>(null)
const gameHistory = ref<DiscoveryEntry[]>([])
const pastHistory = ref<DiscoveryEntry[]>([])
const totalGamesCount = ref(0)

const filters = reactive({
  genre: '',
  platforms: [] as number[],
  minRating: 0,
  startYear: 2010,
  endYear: new Date().getFullYear(),
})

const allExcludedIds = computed(() => {
  const currentIds = gameHistory.value.map((g) => g.id)
  const pastIds = pastHistory.value.map((g) => g.id)
  return new Set([...currentIds, ...pastIds])
})

const discoveredCount = computed(() => pastHistory.value.length)

export function useGameDiscovery() {
  const loadPastHistory = async () => {
    try {
      const response = await httpClient.get<DiscoveryEntry[]>('/discovery-log')
      pastHistory.value = response.data || []
    } catch (err) {
      console.error('Failed to load discovery log:', err)
      pastHistory.value = []
    }
  }

  const clearPastHistory = async () => {
    try {
      await httpClient.delete('/discovery-log')
      pastHistory.value = []
    } catch (err) {
      console.error('Failed to clear discovery log:', err)
      throw err
    }
  }

  const removeDiscovered = async (gameId: number) => {
    const id = Number(gameId)
    if (!Number.isFinite(id)) return

    try {
      await httpClient.delete(`/discovery-log/${id}`)
    } catch (err) {
      console.error('Failed to remove discovery log entry:', err)
      throw err
    } finally {
      pastHistory.value = pastHistory.value.filter((g) => g?.id !== id)
      gameHistory.value = gameHistory.value.filter((g) => g?.id !== id)
    }
  }

  const fetchGameDetails = async (gameId: number) => {
    try {
      gameDescription.value = 'Decrypting database entry...'
      const response = await apiService.getGameDetails(gameId)
      const data = response.data as { description?: string }
      const englishDescription = data.description
      gameDescription.value = await apiService.translateGameDescription(englishDescription || '')
    } catch (err) {
      console.error('Translation failure:', err)
      gameDescription.value = 'Data corrupted: unable to translate entry.'
    }
  }

  const generateGame = async () => {
    isLoading.value = true
    error.value = null

    try {
      const excludeIds = [...allExcludedIds.value].join(',')
      const res = await apiService.getRandomDiscovery({
        genre: filters.genre || undefined,
        platforms:
          filters.platforms?.length > 0 ? filters.platforms.join(',') : undefined,
        startYear: filters.startYear,
        endYear: filters.endYear,
        minRating: filters.minRating,
        excludeIds: excludeIds || undefined,
      })

      const payload = res.data as { success?: boolean; error?: string; game?: Record<string, unknown> }
      if (!payload?.success) {
        error.value = payload?.error || 'No games found. Adjust your parameters.'
        return
      }

      const selected = payload.game
      if (!selected?.id) {
        error.value = 'No games found. Adjust your parameters.'
        return
      }

      const entry: DiscoveryEntry = { id: selected.id as number, name: selected.name as string }
      gameHistory.value.push(entry)
      if (!pastHistory.value.some((p) => p.id === entry.id)) {
        pastHistory.value = [...pastHistory.value, entry]
      }
      try {
        await httpClient.post('/discovery-log', [entry])
      } catch (err) {
        console.error('Failed to auto-save discovery log entry:', err)
      }

      await fetchGameDetails(selected.id as number)
      currentGame.value = selected
    } catch (err) {
      console.error('Discovery failure:', err)
      error.value = 'System failure: unable to retrieve classified data.'
    } finally {
      isLoading.value = false
    }
  }

  const loadGameById = async (gameId: number) => {
    isLoading.value = true
    error.value = null

    try {
      const response = await apiService.getGameDetails(gameId)
      currentGame.value = response.data as Record<string, unknown>
      await fetchGameDetails(gameId)
    } catch (err) {
      console.error('Failed to load game:', err)
      error.value = 'System failure: unable to retrieve classified data.'
    } finally {
      isLoading.value = false
    }
  }

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
    discoveredCount,
    totalGamesCount,
    filters,
    generateGame,
    loadGameById,
    clearHistory,
    loadPastHistory,
    clearPastHistory,
    removeDiscovered,
  }
}
