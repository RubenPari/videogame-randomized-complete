import type { AxiosInstance, AxiosResponse } from 'axios'
import apiService from '@/services/api'
import httpClient from '@/services/httpClient'
import type { DiscoveryEntry } from './state'

export interface RandomDiscoveryParams {
  genre?: string
  platforms?: string
  startYear?: number
  endYear?: number
  minRating?: number
  excludeIds?: string
  excludeAdditions?: boolean
}

/** RAWG + discovery random + translation behind one port for tests and swaps (DIP). */
export interface DiscoveryApiPort {
  getRandomDiscovery(params?: RandomDiscoveryParams): Promise<AxiosResponse<unknown>>
  getGameDetails(gameId: number): Promise<AxiosResponse<unknown>>
  translateGameDescription(text: string): Promise<string>
}

export const defaultDiscoveryApi: DiscoveryApiPort = {
  getRandomDiscovery: (params) => apiService.getRandomDiscovery(params),
  getGameDetails: (gameId) => apiService.getGameDetails(gameId),
  translateGameDescription: (text) => apiService.translateGameDescription(text),
}

export interface DiscoveryLogPort {
  loadEntries(): Promise<DiscoveryEntry[]>
  clearEntries(): Promise<void>
  removeEntry(gameId: number): Promise<void>
  appendEntries(entries: DiscoveryEntry[]): Promise<void>
}

export function createDiscoveryLogHttpPort(client: AxiosInstance = httpClient): DiscoveryLogPort {
  return {
    async loadEntries() {
      const response = await client.get<DiscoveryEntry[]>('/discovery-log')
      return response.data || []
    },
    async clearEntries() {
      await client.delete('/discovery-log')
    },
    async removeEntry(gameId: number) {
      await client.delete(`/discovery-log/${gameId}`)
    },
    async appendEntries(entries: DiscoveryEntry[]) {
      await client.post('/discovery-log', entries)
    },
  }
}
