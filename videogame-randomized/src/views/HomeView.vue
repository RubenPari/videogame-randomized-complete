<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useVaultStore } from '@/stores/useVaultStore'
import { useGameDiscovery } from '@/composables/useGameDiscovery'
import apiService from '@/services/api'
import type { GenreDto, PlatformDto, GameDto } from '@/types/api-dtos'

import FilterSection from '@/components/FilterSection.vue'
import GameCard from '@/components/GameCard.vue'
import GameStatePlaceholder from '@/components/base/GameStatePlaceholder.vue'

const vault = useVaultStore()
const discovery = useGameDiscovery()

const genres = ref<GenreDto[]>([])
const platforms = ref<PlatformDto[]>([])

const currentGame = computed<GameDto | null>(() => discovery.currentGame.value as unknown as GameDto | null)

onMounted(async () => {
  try {
    const [genresRes, platformsRes] = await Promise.all([
      apiService.getGenres(),
      apiService.getPlatforms()
    ])
    genres.value = (genresRes.data as { results: GenreDto[] }).results
    platforms.value = (platformsRes.data as { results: PlatformDto[] }).results
    await Promise.all([
      vault.loadVault(),
      discovery.loadPastHistory()
    ])
  } catch (err) {
    console.error('Initialization error:', err)
  }
})

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
          v-if="currentGame"
          :game="currentGame"
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

      <!-- Error Reporting -->
      <div v-if="discovery.error.value" class="bg-red-500/10 border-l-2 border-red-500 text-red-400 p-4 rounded-r-xl text-sm font-mono animate-fade-in">
        <p class="font-bold uppercase tracking-wider mb-1">{{ $t('error.system_error') }}</p>
        <p>{{ discovery.error.value }}</p>
      </div>
    </div>
  </div>
</template>
