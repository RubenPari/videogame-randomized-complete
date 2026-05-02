import { ref, reactive, computed } from 'vue'

export interface DiscoveryEntry {
  id: number
  name: string
}

export const currentGame = ref<Record<string, unknown> | null>(null)
export const gameDescription = ref('')
export const isLoading = ref(false)
export const error = ref<string | null>(null)
export const gameHistory = ref<DiscoveryEntry[]>([])
export const pastHistory = ref<DiscoveryEntry[]>([])
export const totalGamesCount = ref(0)

export const filters = reactive({
  genre: '',
  platforms: [] as number[],
  minRating: 0,
  startYear: 2010,
  endYear: new Date().getFullYear(),
})

export const allExcludedIds = computed(() => {
  const currentIds = gameHistory.value.map((g) => g.id)
  const pastIds = pastHistory.value.map((g) => g.id)
  return new Set([...currentIds, ...pastIds])
})

export const discoveredCount = computed(() => pastHistory.value.length)
