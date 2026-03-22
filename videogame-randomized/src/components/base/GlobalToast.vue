<script setup>
import { useToastStore } from '@/stores/useToastStore'

const toastStore = useToastStore()
</script>

<template>
  <Transition name="toast">
    <div
v-if="toastStore.showNotification"
      class="fixed bottom-6 left-1/2 -translate-x-1/2 px-4 py-3 rounded-xl font-mono text-xs uppercase tracking-wider font-bold shadow-2xl z-50 flex items-center gap-3 backdrop-blur-md border"
      :class="toastStore.notificationType === 'success'
        ? 'bg-cyan-500/90 text-zinc-950 border-cyan-400'
        : 'bg-red-500/90 text-white border-red-400'">

      <svg v-if="toastStore.notificationType === 'success'" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path></svg>
      <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path></svg>

      {{ toastStore.notificationMessage }}
    </div>
  </Transition>
</template>

<style scoped>
.toast-enter-active, .toast-leave-active { transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); }
.toast-enter-from { opacity: 0; transform: translate(-50%, 20px) scale(0.9); }
.toast-leave-to { opacity: 0; transform: translate(-50%, 20px) scale(0.9); }
</style>
