<script setup lang="ts">
defineProps<{
  subtitle?: string
}>()
</script>

<template>
  <div class="min-h-screen bg-zinc-950 flex items-center justify-center px-4">
    <div class="w-full max-w-md">
      <!-- Logo -->
      <div class="text-center mb-10">
        <div class="inline-flex items-center gap-3 mb-4">
          <div
            class="w-12 h-12 bg-cyan-500 text-zinc-950 flex items-center justify-center rounded-lg font-black text-2xl transform -skew-x-6"
          >
            VG
          </div>
          <h1 class="text-3xl font-black text-white tracking-tight uppercase italic">
            Random<span class="text-cyan-400">Generator</span>
          </h1>
        </div>
        <p v-if="subtitle" class="text-zinc-500 text-sm">{{ subtitle }}</p>
      </div>

      <!-- Content slot -->
      <slot></slot>
    </div>
  </div>
</template>
