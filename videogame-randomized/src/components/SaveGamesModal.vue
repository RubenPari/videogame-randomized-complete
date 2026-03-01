<script setup>
import { ref, computed } from 'vue'
import { useVaultStore } from '@/stores/useVaultStore'

defineProps({
  show: { type: Boolean, default: false }
})

const emit = defineEmits(['close'])

const vault = useVaultStore()
const searchQuery = ref('')

// Computed
const filteredGames = computed(() => {
  if (!searchQuery.value.trim()) return vault.savedGames
  const query = searchQuery.value.toLowerCase()
  return vault.savedGames.filter(game => 
    game.name.toLowerCase().includes(query) ||
    game.genres?.some(genre => genre.name.toLowerCase().includes(query))
  )
})

// Methods
const removeGame = async (id) => await vault.removeGame(id)
const clearAll = async () => {
  if (confirm('Initiate purge of all vault records?')) {
    await vault.clearVault()
  }
}

const exportGames = () => {
  if (vault.savedGames.length === 0) return
  const data = vault.savedGames.map(g => ({
    name: g.name,
    rating: g.rating,
    released: g.released
  }))
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `vault-export-${new Date().toISOString().split('T')[0]}.json`
  link.click()
  URL.revokeObjectURL(url)
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
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="show" class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6" @click.self="emit('close')">
        <div class="absolute inset-0 bg-zinc-950/90 backdrop-blur-sm"></div>
        
        <div class="relative w-full max-w-5xl max-h-[90vh] flex flex-col bg-zinc-900 border border-zinc-800 rounded-2xl shadow-2xl overflow-hidden" @click.stop>
          
          <!-- Header -->
          <div class="flex items-center justify-between p-6 border-b border-zinc-800 bg-zinc-950">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 bg-fuchsia-500 text-zinc-950 flex items-center justify-center rounded-lg">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="square" stroke-linejoin="miter" stroke-width="2" d="M5 5a2 2 0 012-2h10a2 2 0 012 2v10l-7-3.5L5 21V5z"></path>
                </svg>
              </div>
              <div>
                <h2 class="text-xl font-black text-white uppercase tracking-widest">Personal Vault</h2>
                <p class="text-xs font-mono text-zinc-500">Total Entries: {{ vault.count }}</p>
              </div>
            </div>
            <button @click="emit('close')" class="p-2 bg-zinc-900 border border-zinc-800 rounded-lg text-zinc-500 hover:text-white hover:border-zinc-700 transition-colors">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
            </button>
          </div>

          <div class="p-6 flex-1 overflow-hidden flex flex-col gap-6">
            <!-- Search & Actions -->
            <div class="flex flex-col sm:flex-row gap-4 justify-between flex-shrink-0">
              <div class="relative w-full sm:w-96">
                <input v-model="searchQuery" type="text" placeholder="QUERY DATABASE..." 
                  class="w-full bg-zinc-950 border border-zinc-800 text-white font-mono text-sm px-4 py-3 pl-10 rounded-xl focus:outline-none focus:border-fuchsia-500 focus:ring-1 focus:ring-fuchsia-500 transition-colors">
                <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-zinc-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
              </div>
              <div class="flex gap-2">
                <button v-if="vault.savedGames.length" @click="exportGames" class="px-4 py-2 bg-zinc-950 border border-zinc-800 hover:border-zinc-600 text-zinc-300 hover:text-white text-[10px] font-bold uppercase tracking-widest rounded-xl transition-colors flex items-center gap-2">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"></path></svg>
                  Export
                </button>
                <button v-if="vault.savedGames.length" @click="clearAll" class="px-4 py-2 border border-red-500/30 text-red-400 hover:bg-red-500/10 hover:border-red-500 text-[10px] font-bold uppercase tracking-widest rounded-xl transition-colors flex items-center gap-2">
                  Purge All
                </button>
              </div>
            </div>

            <!-- Empty State -->
            <div v-if="filteredGames.length === 0" class="flex-1 flex flex-col items-center justify-center py-12 text-center bg-zinc-950 border border-zinc-800 rounded-xl">
               <p class="text-zinc-600 font-mono text-sm uppercase tracking-widest">No matching records found in vault.</p>
            </div>
            
            <!-- List -->
            <div v-else class="flex-1 overflow-y-auto custom-scrollbar pr-2 pb-2">
              <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
                <div v-for="game in filteredGames" :key="game.id" class="flex bg-zinc-950 border border-zinc-800 rounded-xl overflow-hidden group hover:border-zinc-700 transition-all h-32">
                  <div class="w-32 h-full flex-shrink-0 bg-zinc-900">
                    <img :src="game.background_image || '/placeholder-game.jpg'" class="w-full h-full object-cover opacity-70 group-hover:opacity-100 transition-opacity" />
                  </div>
                  <div class="p-4 flex-1 flex flex-col justify-between min-w-0">
                    <div>
                      <h3 class="text-white font-bold text-sm truncate uppercase tracking-tight">{{ game.name }}</h3>
                      <p class="text-[10px] text-zinc-500 font-mono mt-1">{{ formatDate(game.released) }}</p>
                    </div>
                    <div class="flex items-center justify-between">
                      <span class="text-[10px] font-bold text-cyan-400 font-mono flex items-center gap-1 bg-zinc-900 px-2 py-1 rounded">
                        ★ {{ Number(game.rating).toFixed(1) }}
                      </span>
                      <button @click="removeGame(game.id)" class="p-1.5 text-zinc-600 hover:text-red-400 hover:bg-red-400/10 rounded transition-colors" title="Delete Record">
                        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg>
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.fade-enter-active .relative, .fade-leave-active .relative { transition: all 0.2s ease; }
.fade-enter-from .relative, .fade-leave-to .relative { transform: scale(0.98) translateY(10px); }
.custom-scrollbar::-webkit-scrollbar { width: 6px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #27272a; border-radius: 3px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3f3f46; }
</style>
