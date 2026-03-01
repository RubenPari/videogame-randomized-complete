<script setup>
import { ref, computed } from 'vue'
import { useVaultStore } from '@/stores/useVaultStore'

const props = defineProps({
  game: { type: Object, default: null },
  description: { type: String, default: '' }
})

const vault = useVaultStore()

// Local UI State
const showNotification = ref(false)
const notificationType = ref('success')
const notificationMessage = ref('')

// Computed
const isSaved = computed(() => vault.isGameSaved(props.game?.id))

// Methods
const toggleSaveGame = async () => {
  if (!props.game) return
  
  try {
    const saved = await vault.toggleGame(props.game)
    showToast(
      saved ? 'Game saved to vault' : 'Game removed from vault',
      'success'
    )
  } catch {
    showToast('Vault error: transmission failed', 'error')
  }
}

const showToast = (message, type = 'success') => {
  notificationMessage.value = message
  notificationType.value = type
  showNotification.value = true
  setTimeout(() => (showNotification.value = false), 3000)
}

const formatDate = (dateString) => {
  if (!dateString) return 'UNKNOWN'
  return new Date(dateString).toLocaleDateString('it-IT', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  }).toUpperCase()
}
</script>

<template>
  <div v-if="game" class="bg-zinc-900 border border-zinc-800 rounded-2xl overflow-hidden shadow-2xl relative group flex flex-col">
    <!-- Image Section -->
    <div class="relative h-72 md:h-96 w-full flex-shrink-0">
      <img :src="game.background_image || '/placeholder-game.jpg'" :alt="game.name"
        class="w-full h-full object-cover transition-transform duration-1000 group-hover:scale-105" />
      <div class="absolute inset-0 bg-gradient-to-t from-zinc-900 via-zinc-900/40 to-transparent"></div>
      
      <!-- Rating Badge -->
      <div class="absolute top-4 right-4 flex gap-2">
        <div class="bg-zinc-950/90 backdrop-blur border border-zinc-800 px-3 py-1.5 rounded-lg flex items-center gap-1.5">
          <svg class="w-4 h-4 text-cyan-400" fill="currentColor" viewBox="0 0 20 20">
            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"></path>
          </svg>
          <span class="text-white font-mono font-bold">{{ Number(game.rating).toFixed(1) }}</span>
        </div>
      </div>

      <!-- Save Button -->
      <div class="absolute top-4 left-4">
        <button @click="toggleSaveGame" 
          class="p-2.5 rounded-lg backdrop-blur border transition-all"
          :class="isSaved ? 'bg-fuchsia-500/20 border-fuchsia-500 text-fuchsia-400 hover:bg-fuchsia-500/30' : 'bg-zinc-950/90 border-zinc-800 text-zinc-400 hover:text-white hover:border-zinc-600'">
          <svg v-if="isSaved" class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
             <path fill-rule="evenodd" d="M3 5a2 2 0 012-2h10a2 2 0 012 2v10l-7-3.5L3 15V5z" clip-rule="evenodd"></path>
          </svg>
          <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
             <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 5a2 2 0 012-2h10a2 2 0 012 2v16l-7-3.5L5 21V5z"></path>
          </svg>
        </button>
      </div>
      
      <!-- Title & Release -->
      <div class="absolute bottom-0 left-0 w-full p-6 md:p-8">
        <div class="flex flex-wrap gap-2 mb-3">
          <span v-for="genre in game.genres?.slice(0,3)" :key="genre.id" 
            class="text-[10px] font-bold uppercase tracking-widest px-2 py-1 bg-zinc-800/80 backdrop-blur border border-zinc-700 text-zinc-300 rounded">
            {{ genre.name }}
          </span>
        </div>
        <h2 class="text-3xl md:text-5xl font-black text-white leading-none mb-3 uppercase tracking-tight">{{ game.name }}</h2>
        <div class="flex items-center gap-2 text-cyan-400 text-xs font-mono font-bold tracking-wider">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
          </svg>
          {{ formatDate(game.released) }}
        </div>
      </div>
    </div>

    <!-- Content Section -->
    <div class="p-6 md:p-8 space-y-8 flex-1 flex flex-col">
      <!-- Hardware -->
      <div>
        <h3 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest mb-3">Compatible Hardware</h3>
        <div class="flex flex-wrap gap-2">
          <span v-for="platform in game.platforms" :key="platform.platform.id"
            class="text-xs font-mono px-2.5 py-1 bg-zinc-950 border border-zinc-800 text-zinc-400 rounded-md flex items-center gap-2">
            <span class="w-1.5 h-1.5 rounded-full bg-cyan-500/50"></span>
            {{ platform.platform.name }}
          </span>
        </div>
      </div>

      <!-- Description -->
      <div class="flex-1">
        <h3 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest mb-3">Database Entry</h3>
        <div class="bg-zinc-950 border border-zinc-800 rounded-xl p-5 text-zinc-400 text-sm leading-relaxed max-h-60 overflow-y-auto custom-scrollbar">
          {{ description || 'Retrieving classified data...' }}
        </div>
      </div>
    </div>

    <!-- Toast -->
    <Transition name="toast">
      <div v-if="showNotification" class="absolute top-4 left-1/2 -translate-x-1/2 px-4 py-2 rounded-lg font-mono text-xs uppercase tracking-wider font-bold shadow-2xl z-10"
        :class="notificationType === 'success' ? 'bg-cyan-500 text-zinc-950' : 'bg-red-500 text-white'">
        {{ notificationMessage }}
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from, .toast-leave-to { opacity: 0; transform: translate(-50%, -10px); }
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #27272a; border-radius: 2px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3f3f46; }
</style>
