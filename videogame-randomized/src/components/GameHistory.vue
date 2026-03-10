<script setup>
import { computed } from 'vue'

const props = defineProps({
  gameHistory: { type: Array, default: () => [] },
  pastHistory: { type: Array, default: () => [] },
  isSavingLog: { type: Boolean, default: false }
})

const emit = defineEmits(['clear-history', 'save-session', 'clear-past-history'])

const onClear = () => emit('clear-history')
const onSave = () => emit('save-session')
const onClearPast = () => emit('clear-past-history')

const pastCount = computed(() => props.pastHistory.length)
</script>

<template>
  <div class="space-y-4">
    <!-- Current Session Log -->
    <div v-if="gameHistory.length > 0" class="bg-zinc-900 border border-zinc-800 rounded-2xl p-6 shadow-lg">
      <div class="flex items-center justify-between mb-4 pb-4 border-b border-zinc-800">
        <h3 class="text-sm font-black text-white uppercase tracking-widest flex items-center gap-2">
          <svg class="w-4 h-4 text-fuchsia-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="square" stroke-linejoin="miter" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
          </svg>
          Session Log
        </h3>
        <div class="flex items-center gap-2">
          <!-- Save Session Button -->
          <button @click="onSave" :disabled="isSavingLog"
            class="text-[10px] font-bold uppercase tracking-widest transition-all px-3 py-1.5 rounded border flex items-center gap-1.5"
            :class="isSavingLog
              ? 'text-zinc-600 bg-zinc-950 border-zinc-800 cursor-not-allowed'
              : 'text-cyan-400 bg-zinc-950 border-cyan-500/30 hover:border-cyan-500 hover:bg-cyan-500/10'">
            <svg v-if="!isSavingLog" class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
            <svg v-else class="w-3 h-3 animate-spin" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            {{ isSavingLog ? 'Saving...' : 'Save' }}
          </button>
          <!-- Purge Button -->
          <button @click="onClear"
            class="text-[10px] font-bold text-zinc-600 hover:text-red-400 uppercase tracking-widest transition-colors bg-zinc-950 px-2 py-1.5 rounded border border-zinc-800 hover:border-red-500/50">
            Purge
          </button>
        </div>
      </div>

      <div class="space-y-2 max-h-60 overflow-y-auto custom-scrollbar pr-2">
        <div v-for="(game, index) in gameHistory.slice().reverse()" :key="game.id"
          class="flex items-center justify-between p-3 bg-zinc-950 border border-zinc-800 rounded-lg group hover:border-zinc-700 transition-colors">
          <div class="flex items-center gap-3 overflow-hidden">
            <span class="text-xs font-mono text-zinc-600">#{{ gameHistory.length - index }}</span>
            <span class="text-sm font-bold text-zinc-400 truncate group-hover:text-white transition-colors">{{ game.name }}</span>
          </div>
          <div class="w-1.5 h-1.5 bg-fuchsia-500/50 group-hover:bg-fuchsia-500 rounded-full transition-colors shadow-[0_0_5px_rgba(217,70,239,0.5)]"></div>
        </div>
      </div>
    </div>

    <!-- Past Sessions (Saved Log) -->
    <div v-if="pastCount > 0" class="bg-zinc-900/50 border border-zinc-800/60 rounded-2xl p-5">
      <div class="flex items-center justify-between mb-3">
        <h4 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest flex items-center gap-2">
          <svg class="w-3.5 h-3.5 text-cyan-500/60" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4" />
          </svg>
          Giochi esclusi (sessioni precedenti)
          <span class="text-cyan-400/60 font-mono">{{ pastCount }}</span>
        </h4>
        <button @click="onClearPast"
          class="text-[10px] font-bold text-zinc-600 hover:text-red-400 uppercase tracking-widest transition-colors px-2 py-1 rounded border border-zinc-800/50 hover:border-red-500/50">
          Reset
        </button>
      </div>
      <div class="flex flex-wrap gap-1.5">
        <span v-for="game in pastHistory.slice(0, 20)" :key="game.id"
          class="text-[10px] font-mono px-2 py-0.5 bg-zinc-950/80 border border-zinc-800/50 text-zinc-500 rounded truncate max-w-[150px]"
          :title="game.name">
          {{ game.name }}
        </span>
        <span v-if="pastCount > 20"
          class="text-[10px] font-mono px-2 py-0.5 text-zinc-600">
          +{{ pastCount - 20 }} altri
        </span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #27272a; border-radius: 2px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3f3f46; }
</style>
