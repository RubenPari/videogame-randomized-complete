<script setup lang="ts">
defineProps<{
  show?: boolean
  maxWidth?: string
}>()

const emit = defineEmits<{
  close: []
}>()
</script>

<template>
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="show" class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6" @click.self="emit('close')">
        <div class="absolute inset-0 bg-zinc-950/90 backdrop-blur-sm"></div>

        <div
          class="relative w-full flex flex-col bg-zinc-900 border border-zinc-800 rounded-2xl shadow-2xl overflow-hidden"
          :class="maxWidth || 'max-w-5xl'"
          :style="{ maxHeight: '90vh' }"
          @click.stop
        >
          <slot name="header"></slot>
          <slot></slot>
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
</style>
