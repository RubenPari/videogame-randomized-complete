import type { AxiosResponse } from 'axios'
import translationService from './translation'
import httpClient from './httpClient'
import type {
  RawgGameLike,
  RawgGenreLike,
  RawgListResponse,
  RawgPlatformLike,
  DiscoveryRandomResponse,
} from '@/types/api-dtos'

interface Screenshot {
  id: number
  image: string
}

interface Movie {
  name?: string
  preview?: string
  data?: {
    max?: string
    '480'?: string
  }
}

interface RawgClient {
  get<T>(path: string, config?: Record<string, unknown>): Promise<AxiosResponse<T>>
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
  excludeAdditions?: boolean
}

let youtubeQuotaExceeded = false

export default {
  getGenres(): Promise<AxiosResponse<RawgListResponse<RawgGenreLike>>> {
    return rawgClient.get<RawgListResponse<RawgGenreLike>>('/genres')
  },

  getPlatforms(): Promise<AxiosResponse<RawgListResponse<RawgPlatformLike>>> {
    return rawgClient.get<RawgListResponse<RawgPlatformLike>>('/platforms')
  },

  getGames(params: GameListParams = {}): Promise<AxiosResponse<RawgListResponse<RawgGameLike>>> {
    return rawgClient.get<RawgListResponse<RawgGameLike>>('/games', {
      params: { ...params },
    })
  },

  getRandomDiscovery(params: RandomDiscoveryParams = {}): Promise<AxiosResponse<DiscoveryRandomResponse>> {
    return httpClient.get<DiscoveryRandomResponse>('/discovery/random', { params })
  },

  getGameDetails(gameId: number | string): Promise<AxiosResponse<RawgGameLike>> {
    return rawgClient.get<RawgGameLike>(`/games/${gameId}`)
  },

  getGameScreenshots(gameId: number | string): Promise<AxiosResponse<RawgListResponse<Screenshot>>> {
    return rawgClient.get<RawgListResponse<Screenshot>>(`/games/${gameId}/screenshots`)
  },

  getGameMovies(gameId: number | string): Promise<AxiosResponse<RawgListResponse<Movie>>> {
    return rawgClient.get<RawgListResponse<Movie>>(`/games/${gameId}/movies`)
  },

  async searchYouTubeTrailer(gameName: string): Promise<string | null> {
    if (!gameName || youtubeQuotaExceeded) return null
    try {
      const response = await httpClient.get('/youtube/search', {
        params: { q: `${gameName} official trailer` },
      })
      return (response.data as { videoId?: string })?.videoId || null
    } catch (error: any) {
      if (error?.response?.status === 403) {
        console.warn('YouTube API quota exceeded. Disabling trailer search for this session.')
        youtubeQuotaExceeded = true
      }
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
