<script setup>
import { ref, onMounted, defineAsyncComponent } from 'vue'
import { useVaultStore } from '@/stores/useVaultStore'
import { useGameDiscovery } from '@/composables/useGameDiscovery'
import apiService from '@/services/api'

// Components
import FilterSection from './components/FilterSection.vue'
import GameCard from './components/GameCard.vue'
import GameHistory from './components/GameHistory.vue'

// Async Modal (Optimization)
const SaveGamesModal = defineAsyncComponent(() => 
  import('./components/SaveGamesModal.vue')
)

// Store & Composable
const vault = useVaultStore()
const discovery = useGameDiscovery()

// Local State
const genres = ref([])
const platforms = ref([])
const showSavedGamesModal = ref(false)

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
    await vault.loadVault()
  } catch (err) {
    console.error('Initialization error:', err)
  }
})

// UI Event Handlers
const openVault = () => (showSavedGamesModal.value = true)
const closeVault = () => (showSavedGamesModal.value = false)
</script>

<template>
  <div class="min-h-screen bg-zinc-950 text-zinc-300 font-sans selection:bg-cyan-500/30">
    <header class="sticky top-0 z-50 bg-zinc-950/90 backdrop-blur-md border-b border-zinc-800">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-20 flex items-center justify-between">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 bg-cyan-500 text-zinc-950 flex items-center justify-center rounded-lg font-black text-xl transform -skew-x-6">
            VG
          </div>
          <h1 class="text-2xl font-black text-white tracking-tight uppercase italic">
            Random<span class="text-cyan-400">Generator</span>
          </h1>
        </div>
        <button @click="openVault"
          class="group flex items-center gap-2 px-4 py-2.5 bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 hover:border-fuchsia-500/50 rounded-xl transition-all">
          <svg class="w-5 h-5 text-fuchsia-500 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="square" stroke-linejoin="miter" stroke-width="2" d="M5 5a2 2 0 012-2h10a2 2 0 012 2v16l-7-3.5L5 21V5z"></path>
          </svg>
          <span class="font-bold text-white uppercase text-sm tracking-wider hidden sm:block">Vault</span>
          <span v-if="vault.count > 0" class="flex h-5 w-5 items-center justify-center rounded-md bg-fuchsia-500 text-[10px] font-black text-zinc-950 ml-1">
            {{ vault.count }}
          </span>
        </button>
      </div>
    </header>

    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12 flex flex-col lg:flex-row gap-8">
      <!-- Sidebar with Filters -->
      <div class="w-full lg:w-1/3 lg:sticky lg:top-28 h-fit">
        <FilterSection 
          v-model="discovery.filters" 
          :genres="genres" 
          :platforms="platforms" 
          :isLoading="discovery.isLoading.value"
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
          <div v-else-if="discovery.isLoading.value" class="flex-1 flex flex-col items-center justify-center border border-dashed border-zinc-800 rounded-2xl bg-zinc-900/30 p-12">
            <div class="w-16 h-16 border-4 border-zinc-800 border-t-cyan-500 rounded-full animate-spin mb-6"></div>
            <p class="text-cyan-400 font-mono text-sm uppercase tracking-widest animate-pulse">Initializing Protocol...</p>
          </div>

          <!-- Empty State -->
          <div v-else class="flex-1 flex flex-col items-center justify-center border border-dashed border-zinc-800 rounded-2xl bg-zinc-900/30 p-12 text-center">
            <div class="w-20 h-20 bg-zinc-900 border border-zinc-800 rounded-2xl flex items-center justify-center mb-6 transform rotate-3">
              <svg class="w-10 h-10 text-zinc-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z"></path>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <h3 class="text-lg font-black text-white mb-2 uppercase tracking-widest">Awaiting Input</h3>
            <p class="text-zinc-500 text-sm max-w-sm">Configure parameters and engage sequence to discover your next adventure.</p>
          </div>
        </div>

        <!-- History -->
        <GameHistory 
          :gameHistory="discovery.gameHistory.value" 
          @clear-history="discovery.clearHistory" 
        />
        
        <!-- Error Reporting -->
        <div v-if="discovery.error.value" class="bg-red-500/10 border-l-2 border-red-500 text-red-400 p-4 rounded-r-xl text-sm font-mono animate-fade-in">
          <p class="font-bold uppercase tracking-wider mb-1">System Error</p>
          <p>{{ discovery.error.value }}</p>
        </div>
      </div>
    </main>

    <!-- Modal (Async) -->
    <SaveGamesModal 
      v-if="showSavedGamesModal" 
      :show="showSavedGamesModal" 
      @close="closeVault" 
    />
  </div>
</template>

<style>
body { background-color: #09090b; }
.animate-fade-in { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { 
  from { opacity: 0; transform: translateY(10px); } 
  to { opacity: 1; transform: translateY(0); } 
}
::-webkit-scrollbar { width: 8px; }
::-webkit-scrollbar-track { background: #09090b; }
::-webkit-scrollbar-thumb { background: #27272a; border-radius: 4px; }
::-webkit-scrollbar-thumb:hover { background: #3f3f46; }
</style>
