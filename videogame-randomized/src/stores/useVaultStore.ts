import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { GameDto, GameStatsDto } from '@/types/api-dtos'
import databaseService from '@/services/database'

export const useVaultStore = defineStore('vault', () => {
  const savedGames = ref<GameDto[]>([])
  const isLoading = ref<boolean>(false)
  const statistics = ref<GameStatsDto>({
    totalGames: 0,
    averageRating: 0,
    genreCount: {},
    platformCount: {},
  })

  const count = computed<number>(() => savedGames.value.length)

  async function loadVault(): Promise<void> {
    isLoading.value = true
    try {
      const [games, stats] = await Promise.all([
        databaseService.getSavedGames(),
        databaseService.getStatistics(),
      ])
      savedGames.value = games
      statistics.value = stats
    } catch (error) {
      console.error('Error loading vault:', error)
    } finally {
      isLoading.value = false
    }
  }

  async function toggleGame(game: GameDto): Promise<boolean> {
    const isSaved = isGameSaved(game.id)
    if (isSaved) {
      await removeGame(game.id)
      return false
    } else {
      const rawgGameLike = {
        id: game.id,
        name: game.name,
        background_image: game.backgroundImage || '',
        rating: game.rating,
        released: game.released,
        genres: game.genres || [],
        platforms: game.platforms ? game.platforms.map((p) => ({ platform: p })) : [],
        metacritic: game.metacritic,
        description_raw: game.descriptionRaw,
      }
      await databaseService.saveGame(rawgGameLike)
      await loadVault()
      return true
    }
  }

  async function removeGame(gameId: number): Promise<void> {
    await databaseService.removeGame(gameId)
    savedGames.value = savedGames.value.filter((g) => g.id !== gameId)
    await loadVault()
  }

  async function clearVault(): Promise<void> {
    await databaseService.clearAllSavedGames()
    savedGames.value = []
    statistics.value = {
      totalGames: 0,
      averageRating: 0,
      genreCount: {},
      platformCount: {},
    }
  }

  function isGameSaved(gameId: number): boolean {
    return savedGames.value.some((g) => g.id === gameId)
  }

  return { savedGames, isLoading, statistics, count, loadVault, toggleGame, removeGame, clearVault, isGameSaved }
})
