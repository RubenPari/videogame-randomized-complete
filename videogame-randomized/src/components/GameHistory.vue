<script setup>
defineProps({
  gameHistory: { type: Array, default: () => [] }
})

const emit = defineEmits(['clear-history'])

const onClear = () => emit('clear-history')
</script>

<template>
  <div v-if="gameHistory.length > 0" class="bg-zinc-900 border border-zinc-800 rounded-2xl p-6 shadow-lg">
    <div class="flex items-center justify-between mb-4 pb-4 border-b border-zinc-800">
      <h3 class="text-sm font-black text-white uppercase tracking-widest flex items-center gap-2">
        <svg class="w-4 h-4 text-fuchsia-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="square" stroke-linejoin="miter" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>
        Session Log
      </h3>
      <button @click="onClear" class="text-[10px] font-bold text-zinc-600 hover:text-red-400 uppercase tracking-widest transition-colors bg-zinc-950 px-2 py-1 rounded border border-zinc-800 hover:border-red-500/50">
        Purge
      </button>
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
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #27272a; border-radius: 2px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3f3f46; }
</style>
