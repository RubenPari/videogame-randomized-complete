import {
  currentGame,
  gameDescription,
  isLoading,
  error,
  gameHistory,
  pastHistory,
  totalGamesCount,
  filters,
  allExcludedIds,
  discoveredCount,
} from '@/composables/gameDiscovery/state'
import {
  defaultDiscoveryApi,
  createDiscoveryLogHttpPort,
  type DiscoveryApiPort,
  type DiscoveryLogPort,
} from '@/composables/gameDiscovery/ports'
import { loadTranslatedGameDescription } from '@/composables/gameDiscovery/external'

const defaultDiscoveryLog = createDiscoveryLogHttpPort()

export interface UseGameDiscoveryOptions {
  api?: DiscoveryApiPort
  discoveryLog?: DiscoveryLogPort
}

export function useGameDiscovery(options?: UseGameDiscoveryOptions) {
  const api = options?.api ?? defaultDiscoveryApi
  const log = options?.discoveryLog ?? defaultDiscoveryLog

  const loadPastHistory = async () => {
    try {
      pastHistory.value = await log.loadEntries()
    } catch (err) {
      console.error('Failed to load discovery log:', err)
      pastHistory.value = []
    }
  }

  const clearPastHistory = async () => {
    try {
      await log.clearEntries()
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
      await log.removeEntry(id)
    } catch (err) {
      console.error('Failed to remove discovery log entry:', err)
      throw err
    } finally {
      pastHistory.value = pastHistory.value.filter((g) => g?.id !== id)
      gameHistory.value = gameHistory.value.filter((g) => g?.id !== id)
    }
  }

  const generateGame = async () => {
    isLoading.value = true
    error.value = null

    try {
      const excludeIds = [...allExcludedIds.value].join(',')
      const res = await api.getRandomDiscovery({
        genre: filters.genre || undefined,
        platforms:
          filters.platforms?.length > 0 ? filters.platforms.join(',') : undefined,
        startYear: filters.startYear,
        endYear: filters.endYear,
        minRating: filters.minRating,
        excludeIds: excludeIds || undefined,
        excludeAdditions: filters.excludeAdditions,
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

      const entry = { id: selected.id as number, name: selected.name as string }
      gameHistory.value.push(entry)
      if (!pastHistory.value.some((p) => p.id === entry.id)) {
        pastHistory.value = [...pastHistory.value, entry]
      }
      try {
        await log.appendEntries([entry])
      } catch (err) {
        console.error('Failed to auto-save discovery log entry:', err)
      }

      await loadTranslatedGameDescription(selected.id as number, api)
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
      const response = await api.getGameDetails(gameId)
      currentGame.value = response.data as Record<string, unknown>
      await loadTranslatedGameDescription(gameId, api)
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
