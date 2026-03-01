<script setup>
defineProps({
  isLoading: { type: Boolean, default: false }
})
</script>

<template>
  <div v-if="isLoading" class="flex-1 flex flex-col items-center justify-center border border-dashed border-zinc-800 rounded-2xl bg-zinc-900/30 p-12">
    <div class="w-16 h-16 border-4 border-zinc-800 border-t-cyan-500 rounded-full animate-spin mb-6"></div>
    <p class="text-cyan-400 font-mono text-sm uppercase tracking-widest animate-pulse">Initializing Protocol...</p>
  </div>

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
</template>
