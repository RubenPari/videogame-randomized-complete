<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useVaultStore } from '@/stores/useVaultStore'
import { useToastStore } from '@/stores/useToastStore'
import { useGameDiscovery } from '@/composables/useGameDiscovery'
import apiService from '@/services/api'

const { t } = useI18n()

// Components
import FilterSection from '@/components/FilterSection.vue'
import GameCard from '@/components/GameCard.vue'
import GameHistory from '@/components/GameHistory.vue'
import GameStatePlaceholder from '@/components/base/GameStatePlaceholder.vue'

// Store & Composable
const vault = useVaultStore()
const toastStore = useToastStore()
const discovery = useGameDiscovery()

// Local State
const genres = ref([])
const platforms = ref([])

/**
 * Initial load: populate genres, platforms and vault data
 */
onMounted(async () => {
  try {
    const [genresRes, platformsRes] = await Promise.all([
      apiService.getGenres(),
      apiService.getPlatforms()
    ])
    genres.value = genresRes.data.results
    platforms.value = platformsRes.data.results
    await Promise.all([
      vault.loadVault(),
      discovery.loadPastHistory()
    ])
  } catch (err) {
    console.error('Initialization error:', err)
  }
})

const handleClearPastHistory = async () => {
  try {
    await discovery.clearPastHistory()
    toastStore.showToast(t('home.history_cleared'), 'success')
  } catch {
    toastStore.showToast(t('home.history_clear_error'), 'error')
  }
}
</script>

<template>
  <div class="flex flex-col lg:flex-row gap-8">
    <!-- Sidebar with Filters -->
    <div class="w-full lg:w-1/3 lg:sticky lg:top-28 h-fit">
      <FilterSection
        v-model="discovery.filters"
        :genres="genres"
        :platforms="platforms"
        :is-loading="discovery.isLoading.value"
        @generate="discovery.generateGame"
      />
    </div>

    <!-- Main Results Column -->
    <div class="w-full lg:w-2/3 space-y-8 flex flex-col">
      <div class="min-h-[400px] flex flex-col relative">
        <GameCard
          v-if="discovery.currentGame.value"
          :game="discovery.currentGame.value"
          :description="discovery.gameDescription.value"
          class="animate-fade-in"
        />

        <!-- Loading State -->
        <GameStatePlaceholder
          v-else-if="discovery.isLoading.value"
          :is-loading="true"
        />

        <!-- Empty State -->
        <GameStatePlaceholder
          v-else
          :is-loading="false"
        />
      </div>

      <!-- History -->
      <GameHistory
        :game-history="discovery.gameHistory.value"
        :past-history="discovery.pastHistory.value"
        @clear-history="discovery.clearHistory"
        @clear-past-history="handleClearPastHistory"
        @select-game="discovery.loadGameById"
      />

      <!-- Error Reporting -->
      <div v-if="discovery.error.value" class="bg-red-500/10 border-l-2 border-red-500 text-red-400 p-4 rounded-r-xl text-sm font-mono animate-fade-in">
        <p class="font-bold uppercase tracking-wider mb-1">{{ $t('error.system_error') }}</p>
        <p>{{ discovery.error.value }}</p>
      </div>
    </div>
  </div>
</template>
