<script setup>
import { computed } from 'vue'

const props = defineProps({
  genres: { type: Array, default: () => [] },
  platforms: { type: Array, default: () => [] },
  isLoading: { type: Boolean, default: false }
})

// Two-way binding for filters using defineModel (Vue 3.4+)
const filters = defineModel({ required: true })

const emit = defineEmits(['generate'])

// Computed
const availableYears = computed(() => {
  const currentYear = new Date().getFullYear()
  return Array.from({ length: currentYear - 1980 + 1 }, (_, i) => currentYear - i)
})

// Methods
const selectAllPlatforms = () => {
  filters.value.platforms = props.platforms.map(p => p.id)
}

const deselectAllPlatforms = () => {
  filters.value.platforms = []
}

const resetYearRange = () => {
  filters.value.startYear = null
  filters.value.endYear = null
}

const onGenerate = () => emit('generate')
</script>

<template>
  <div class="bg-zinc-900 border border-zinc-800 rounded-2xl p-6 shadow-xl">
    <div class="mb-6 pb-4 border-b border-zinc-800">
      <h2 class="text-sm font-black text-white uppercase tracking-widest flex items-center gap-2">
        <svg class="w-4 h-4 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="square" stroke-linejoin="miter" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 100 4m0-4v2m0-6V4"></path>
        </svg>
        Search Parameters
      </h2>
    </div>

    <div class="space-y-6">
      <!-- Genre -->
      <div class="space-y-2">
        <label class="block text-[10px] font-bold text-zinc-500 uppercase tracking-widest">Target Genre</label>
        <div class="relative">
          <select v-model="filters.genre" class="w-full appearance-none bg-zinc-950 border border-zinc-800 text-zinc-300 text-sm px-4 py-3 rounded-xl focus:outline-none focus:border-cyan-500 focus:ring-1 focus:ring-cyan-500 transition-colors">
            <option value="">ALL CLASSIFICATIONS</option>
            <option v-for="genre in genres" :key="genre.id" :value="genre.id">{{ genre.name.toUpperCase() }}</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-4 text-zinc-600">
            <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path></svg>
          </div>
        </div>
      </div>

      <!-- Rating -->
      <div class="space-y-2">
        <div class="flex justify-between items-center">
          <label class="block text-[10px] font-bold text-zinc-500 uppercase tracking-widest">Min Rating</label>
          <span class="text-cyan-400 font-mono text-sm font-bold">{{ Number(filters.minRating).toFixed(1) }}</span>
        </div>
        <input type="range" min="0" max="5" step="0.1" v-model="filters.minRating"
          class="w-full h-1.5 bg-zinc-800 rounded-full appearance-none cursor-pointer accent-cyan-500" />
      </div>

      <!-- Timeline -->
      <div class="space-y-2">
        <div class="flex justify-between items-center">
          <label class="block text-[10px] font-bold text-zinc-500 uppercase tracking-widest">Timeline</label>
          <button @click="resetYearRange" class="text-[10px] text-zinc-600 hover:text-cyan-400 uppercase font-bold tracking-wider transition-colors">Reset</button>
        </div>
        <div class="flex gap-2">
          <select v-model="filters.startYear" class="w-full appearance-none bg-zinc-950 border border-zinc-800 text-zinc-300 px-3 py-2.5 rounded-lg focus:outline-none focus:border-cyan-500 transition-colors text-xs font-mono">
            <option :value="null">START</option>
            <option v-for="year in availableYears" :key="'s'+year" :value="year">{{ year }}</option>
          </select>
          <div class="flex items-center text-zinc-700 text-xs">-</div>
          <select v-model="filters.endYear" class="w-full appearance-none bg-zinc-950 border border-zinc-800 text-zinc-300 px-3 py-2.5 rounded-lg focus:outline-none focus:border-cyan-500 transition-colors text-xs font-mono">
            <option :value="null">END</option>
            <option v-for="year in availableYears" :key="'e'+year" :value="year">{{ year }}</option>
          </select>
        </div>
      </div>

      <!-- Hardware -->
      <div class="space-y-2">
        <div class="flex justify-between items-center">
          <label class="block text-[10px] font-bold text-zinc-500 uppercase tracking-widest">Hardware</label>
          <span class="text-[10px] font-mono text-zinc-500 bg-zinc-950 px-1.5 py-0.5 rounded">{{ filters.platforms.length }} SEL</span>
        </div>
        <div class="bg-zinc-950 border border-zinc-800 rounded-xl p-2 h-40 overflow-y-auto custom-scrollbar">
          <div class="space-y-0.5">
            <label v-for="platform in platforms" :key="platform.id" class="flex items-center gap-3 p-2 rounded-lg hover:bg-zinc-900 cursor-pointer transition-colors">
              <div class="relative flex items-center">
                <input type="checkbox" :value="platform.id" v-model="filters.platforms"
                  class="peer appearance-none w-4 h-4 border border-zinc-700 rounded bg-zinc-950 checked:bg-cyan-500 checked:border-cyan-500 transition-all cursor-pointer" />
                <svg class="absolute w-3 h-3 text-zinc-950 opacity-0 peer-checked:opacity-100 left-0.5 top-0.5 pointer-events-none" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="3">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7"></path>
                </svg>
              </div>
              <span class="text-xs text-zinc-400 select-none peer-checked:text-white">{{ platform.name }}</span>
            </label>
          </div>
        </div>
        <div class="flex gap-2 pt-1">
          <button @click="selectAllPlatforms" class="flex-1 py-1.5 bg-zinc-950 border border-zinc-800 hover:border-zinc-600 text-zinc-400 hover:text-white text-[10px] font-bold uppercase tracking-wider rounded-lg transition-colors">All</button>
          <button @click="deselectAllPlatforms" class="flex-1 py-1.5 bg-zinc-950 border border-zinc-800 hover:border-zinc-600 text-zinc-400 hover:text-white text-[10px] font-bold uppercase tracking-wider rounded-lg transition-colors">None</button>
        </div>
      </div>

      <!-- Sorting -->
      <div class="space-y-2">
        <label class="block text-[10px] font-bold text-zinc-500 uppercase tracking-widest">Sort Protocol</label>
        <div class="relative">
          <select v-model="filters.ordering" class="w-full appearance-none bg-zinc-950 border border-zinc-800 text-zinc-300 text-sm px-4 py-3 rounded-xl focus:outline-none focus:border-cyan-500 transition-colors">
            <option value="-rating">Highest Rating</option>
            <option value="rating">Lowest Rating</option>
            <option value="-released">Latest Release</option>
            <option value="released">Earliest Release</option>
            <option value="-added">Most Popular</option>
          </select>
          <div class="pointer-events-none absolute inset-y-0 right-0 flex items-center px-4 text-zinc-600">
            <svg class="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 9l4-4 4 4m0 6l-4 4-4-4"></path></svg>
          </div>
        </div>
      </div>
    </div>

    <button @click="onGenerate" :disabled="isLoading"
      class="mt-8 w-full py-4 bg-cyan-500 hover:bg-cyan-400 text-zinc-950 font-black text-sm uppercase tracking-widest rounded-xl transition-all transform active:scale-95 disabled:opacity-50 disabled:active:scale-100 flex items-center justify-center gap-2 shadow-[0_0_15px_rgba(6,182,212,0.3)] hover:shadow-[0_0_25px_rgba(6,182,212,0.5)]">
      <span v-if="isLoading" class="flex items-center gap-2">
        <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        PROCESSING...
      </span>
      <span v-else class="flex items-center gap-2">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"></path></svg>
        ENGAGE SCANNER
      </span>
    </button>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #27272a; border-radius: 2px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3f3f46; }
</style>
