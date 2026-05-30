import httpClient from './httpClient'
import type { GameDto, GameStatsDto, RawgGameLike, UpdateGameDto } from '@/types/api-dtos'

const databaseService = {
  async getSavedGames(): Promise<GameDto[]> {
    try {
      const response = await httpClient.get<GameDto[]>('/saved-games')
      return response.data || []
    } catch (error) {
      console.error('Error retrieving saved games:', error)
      throw error
    }
  },

  async isGameSaved(gameId: number): Promise<boolean> {
    try {
      const response = await httpClient.get<{ isSaved: boolean }>(`/saved-games/check/${gameId}`)
      return response.data.isSaved || false
    } catch (error) {
      console.error('Error checking if game is saved:', error)
      throw error
    }
  },

  async saveGame(game: RawgGameLike): Promise<boolean> {
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
              id: p.platform?.id ?? p.id ?? 0,
              name: p.platform?.name ?? p.name ?? '',
              slug: p.platform?.slug ?? p.slug ?? '',
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
    } catch (error: unknown) {
      const axiosError = error as { response?: { status?: number } }
      if (axiosError.response?.status === 409) {
        return false
      }
      console.error('Error saving game:', error)
      throw error
    }
  },

  async removeGame(gameId: number): Promise<boolean> {
    try {
      const response = await httpClient.delete(`/saved-games/${gameId}`)

      if (response.status === 200) {
        return true
      }
      return false
    } catch (error: unknown) {
      const axiosError = error as { response?: { status?: number } }
      if (axiosError.response?.status === 404) {
        return false
      }
      console.error('Error removing game:', error)
      throw error
    }
  },

  async getSavedGame(gameId: number): Promise<GameDto | null> {
    try {
      const response = await httpClient.get<GameDto>(`/saved-games/${gameId}`)
      return response.data || null
    } catch (error: unknown) {
      const axiosError = error as { response?: { status?: number } }
      if (axiosError.response?.status === 404) {
        return null
      }
      console.error('Error retrieving saved game:', error)
      throw error
    }
  },

  async clearAllSavedGames(): Promise<boolean> {
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

  async getStatistics(): Promise<GameStatsDto> {
    try {
      const response = await httpClient.get<{ statistics: GameStatsDto }>('/saved-games/statistics')
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

  async searchSavedGames(query: string): Promise<GameDto[]> {
    try {
      const response = await httpClient.get<{ games: GameDto[] }>(`/saved-games/search?q=${encodeURIComponent(query)}`)
      return response.data.games || []
    } catch (error) {
      console.error('Error searching saved games:', error)
      throw error
    }
  },

  async updateSavedGame(gameId: number, updateData: UpdateGameDto): Promise<boolean> {
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

  async exportSavedGames(): Promise<unknown> {
    try {
      const response = await httpClient.get('/saved-games/export')
      return response.data
    } catch (error) {
      console.error('Error exporting saved games:', error)
      throw error
    }
  },

  async importSavedGames(importData: unknown): Promise<boolean> {
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
