import type { AxiosResponse } from 'axios'
import translationService from './translation'
import httpClient from './httpClient'

interface RawgClient {
  get: (path: string, config?: Record<string, unknown>) => Promise<AxiosResponse<unknown>>
}

const rawgClient: RawgClient = {
  get(path, config) {
    return httpClient.get(`/rawg${path}`, config)
  },
}

interface GameListParams {
  genres?: string
  platforms?: string
  dates?: string
  page_size?: number
}

interface RandomDiscoveryParams {
  genre?: string
  platforms?: string
  startYear?: number
  endYear?: number
  minRating?: number
  excludeIds?: string
}

export default {
  getGenres(): Promise<AxiosResponse<unknown>> {
    return rawgClient.get('/genres')
  },

  getPlatforms(): Promise<AxiosResponse<unknown>> {
    return rawgClient.get('/platforms')
  },

  getGames(params: GameListParams = {}): Promise<AxiosResponse<unknown>> {
    return rawgClient.get('/games', {
      params: { ...params },
    })
  },

  getRandomDiscovery(params: RandomDiscoveryParams = {}): Promise<AxiosResponse<unknown>> {
    return httpClient.get('/discovery/random', { params })
  },

  getGameDetails(gameId: number | string): Promise<AxiosResponse<unknown>> {
    return rawgClient.get(`/games/${gameId}`)
  },

  getGameScreenshots(gameId: number | string): Promise<AxiosResponse<unknown>> {
    return rawgClient.get(`/games/${gameId}/screenshots`)
  },

  getGameMovies(gameId: number | string): Promise<AxiosResponse<unknown>> {
    return rawgClient.get(`/games/${gameId}/movies`)
  },

  async searchYouTubeTrailer(gameName: string): Promise<string | null> {
    if (!gameName) return null
    try {
      const response = await httpClient.get('/youtube/search', {
        params: { q: `${gameName} official trailer` },
      })
      return (response.data as { videoId?: string })?.videoId || null
    } catch {
      return null
    }
  },

  async translateGameDescription(text: string): Promise<string> {
    if (!text) return 'Description not available.'
    try {
      return await translationService.translate(text, 'en', 'it')
    } catch (error) {
      console.error('Error during translation:', error)
      return translationService.fallbackTranslation(text)
    }
  },
}
