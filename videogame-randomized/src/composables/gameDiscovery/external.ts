import type { DiscoveryApiPort } from './ports'
import { gameDescription } from './state'

export async function loadTranslatedGameDescription(
  gameId: number,
  api: DiscoveryApiPort,
): Promise<void> {
  try {
    gameDescription.value = 'Decrypting database entry...'
    const response = await api.getGameDetails(gameId)
    const data = response.data as { description?: string }
    const englishDescription = data.description
    gameDescription.value = await api.translateGameDescription(englishDescription || '')
  } catch (err) {
    console.error('Translation failure:', err)
    gameDescription.value = 'Data corrupted: unable to translate entry.'
  }
}
